using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Teacher.Submissions.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Teacher.Submissions;

public class TeacherSubmissionService : ITeacherSubmissionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TeacherSubmissionService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<TeacherSubmissionResponse>> GetSubmissionsForAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var teacherId = RequireCurrentTeacherId();

        var assignmentOwnedByTeacher = await _context.Assignments
            .AnyAsync(a => a.Id == assignmentId && a.TeacherId == teacherId, cancellationToken);

        if (!assignmentOwnedByTeacher)
        {
            var assignmentExists = await _context.Assignments.AnyAsync(a => a.Id == assignmentId, cancellationToken);
            if (!assignmentExists)
            {
                throw new NotFoundException(nameof(Assignment), assignmentId);
            }

            throw new ForbiddenException("You do not have access to this assignment.");
        }

        var submissions = await _context.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .Where(s => s.AssignmentId == assignmentId)
            .OrderBy(s => s.Student.FullName)
            .ToListAsync(cancellationToken);

        return submissions.Select(ToResponse).ToList();
    }

    public async Task<TeacherSubmissionResponse> GetByIdAsync(Guid submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await FindOwnedSubmissionOrThrowAsync(submissionId, cancellationToken);
        return ToResponse(submission);
    }

    public async Task<TeacherSubmissionResponse> GradeAsync(Guid submissionId, GradeSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var submission = await FindOwnedSubmissionOrThrowAsync(submissionId, cancellationToken);

        if (request.MarksObtained > submission.Assignment.MaxMarks)
        {
            throw new BadRequestException($"Marks obtained cannot exceed the assignment's maximum of {submission.Assignment.MaxMarks}.");
        }

        submission.MarksObtained = request.MarksObtained;
        submission.Feedback = string.IsNullOrWhiteSpace(request.Feedback) ? null : request.Feedback.Trim();
        submission.Status = SubmissionStatus.Graded;

        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(submission);
    }

    public async Task<TeacherSubmissionResponse> UpdateStatusAsync(Guid submissionId, UpdateSubmissionStatusRequest request, CancellationToken cancellationToken = default)
    {
        var submission = await FindOwnedSubmissionOrThrowAsync(submissionId, cancellationToken);

        if (request.Status == SubmissionStatus.Graded)
        {
            throw new BadRequestException("Use the grade endpoint to mark a submission as Graded.");
        }

        // Moving away from Graded (e.g. sending it back for changes) means
        // any previous marks/feedback are no longer valid.
        if (submission.Status == SubmissionStatus.Graded && request.Status != SubmissionStatus.Graded)
        {
            submission.MarksObtained = null;
            submission.Feedback = null;
        }

        submission.Status = request.Status;

        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(submission);
    }

    private Guid RequireCurrentTeacherId() =>
        _currentUserService.UserId ?? throw new UnauthorizedException("Unable to determine the current user.");

    private async Task<Submission> FindOwnedSubmissionOrThrowAsync(Guid submissionId, CancellationToken cancellationToken)
    {
        var teacherId = RequireCurrentTeacherId();

        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Submission), submissionId);

        if (submission.Assignment.TeacherId != teacherId)
        {
            throw new ForbiddenException("You do not have access to this submission.");
        }

        return submission;
    }

    private static TeacherSubmissionResponse ToResponse(Submission submission) => new()
    {
        Id = submission.Id,
        AssignmentId = submission.AssignmentId,
        AssignmentTitle = submission.Assignment.Title,
        MaxMarks = submission.Assignment.MaxMarks,
        StudentId = submission.StudentId,
        StudentName = submission.Student.FullName,
        StudentEmail = submission.Student.Email,
        AnswerText = submission.AnswerText,
        Status = submission.Status.ToString(),
        SubmittedAtUtc = submission.SubmittedAtUtc,
        UpdatedAtUtc = submission.UpdatedAtUtc,
        MarksObtained = submission.MarksObtained,
        Feedback = submission.Feedback
    };
}