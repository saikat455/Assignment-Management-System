using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Student.Submissions.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Student.Submissions;

public class StudentSubmissionService : IStudentSubmissionService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StudentSubmissionService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SubmissionResponse> SubmitAsync(Guid assignmentId, CreateSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var studentId = RequireCurrentStudentId();
        var assignment = await GetAccessibleAssignmentOrThrowAsync(assignmentId, studentId, cancellationToken);

        var alreadySubmitted = await _context.Submissions
            .AnyAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, cancellationToken);

        if (alreadySubmitted)
        {
            throw new ConflictException("You have already submitted this assignment. Use update instead.");
        }

        var now = DateTime.UtcNow;

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText = request.AnswerText.Trim(),
            SubmittedAtUtc = now,
            Status = now > assignment.DeadlineUtc ? SubmissionStatus.Late : SubmissionStatus.Submitted
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(submission, assignment);
    }

    public async Task<SubmissionResponse> UpdateAsync(Guid assignmentId, UpdateSubmissionRequest request, CancellationToken cancellationToken = default)
    {
        var studentId = RequireCurrentStudentId();
        var assignment = await GetAccessibleAssignmentOrThrowAsync(assignmentId, studentId, cancellationToken);

        var submission = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, cancellationToken)
            ?? throw new NotFoundException("No submission found for this assignment yet. Submit it first.");

        if (submission.Status == SubmissionStatus.Graded)
        {
            throw new ConflictException("This submission has already been graded and can no longer be edited.");
        }

        if (submission.Status != SubmissionStatus.Returned && DateTime.UtcNow > assignment.DeadlineUtc)
        {
            throw new BadRequestException("The deadline has passed; you can no longer update your submission.");
        }

        submission.AnswerText = request.AnswerText.Trim();

        if (submission.Status == SubmissionStatus.Returned)
        {
            submission.Status = SubmissionStatus.Submitted;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(submission, assignment);
    }

    public async Task<List<SubmissionResponse>> GetMySubmissionsAsync(CancellationToken cancellationToken = default)
    {
        var studentId = RequireCurrentStudentId();

        var submissions = await _context.Submissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.SubmittedAtUtc)
            .ToListAsync(cancellationToken);

        return submissions.Select(s => ToResponse(s, s.Assignment)).ToList();
    }

    public async Task<SubmissionResponse> GetMySubmissionForAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var studentId = RequireCurrentStudentId();

        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, cancellationToken)
            ?? throw new NotFoundException("No submission found for this assignment.");

        return ToResponse(submission, submission.Assignment);
    }

    private Guid RequireCurrentStudentId() =>
        _currentUserService.UserId ?? throw new UnauthorizedException("Unable to determine the current user.");

    private async Task<Assignment> GetAccessibleAssignmentOrThrowAsync(Guid assignmentId, Guid studentId, CancellationToken cancellationToken)
    {
        var classId = await _context.Users
            .Where(u => u.Id == studentId)
            .Select(u => u.ClassId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new BadRequestException("Your account is not assigned to a class. Contact an administrator.");

        return await _context.Assignments
            .Include(a => a.Subject)
            .FirstOrDefaultAsync(
                a => a.Id == assignmentId && a.Status == AssignmentStatus.Published && a.Subject.ClassId == classId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Assignment), assignmentId);
    }

    private static SubmissionResponse ToResponse(Submission submission, Assignment assignment) => new()
    {
        Id = submission.Id,
        AssignmentId = submission.AssignmentId,
        AssignmentTitle = assignment.Title,
        AnswerText = submission.AnswerText,
        Status = submission.Status.ToString(),
        SubmittedAtUtc = submission.SubmittedAtUtc,
        UpdatedAtUtc = submission.UpdatedAtUtc,
        MarksObtained = submission.MarksObtained,
        MaxMarks = assignment.MaxMarks,
        Feedback = submission.Feedback
    };
}