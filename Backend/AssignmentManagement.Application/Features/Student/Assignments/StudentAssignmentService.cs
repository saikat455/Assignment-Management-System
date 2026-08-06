using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Student.Assignments.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Student.Assignments;

public class StudentAssignmentService : IStudentAssignmentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public StudentAssignmentService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<StudentAssignmentResponse>> GetAssignmentsAsync(Guid? subjectId, CancellationToken cancellationToken = default)
    {
        var classId = await RequireStudentClassIdAsync(cancellationToken);
        var studentId = RequireCurrentStudentId();

        var query = _context.Assignments
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Where(a => a.Status == AssignmentStatus.Published && a.Subject.ClassId == classId);

        if (subjectId is not null)
        {
            query = query.Where(a => a.SubjectId == subjectId);
        }

        var assignments = await query.OrderBy(a => a.DeadlineUtc).ToListAsync(cancellationToken);

        var submissions = await _context.Submissions
            .Where(s => s.StudentId == studentId)
            .ToDictionaryAsync(s => s.AssignmentId, cancellationToken);

        return assignments.Select(a => ToResponse(a, submissions.GetValueOrDefault(a.Id))).ToList();
    }

    public async Task<StudentAssignmentResponse> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default)
    {
        var classId = await RequireStudentClassIdAsync(cancellationToken);
        var studentId = RequireCurrentStudentId();
        var assignment = await _context.Assignments
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(
                a => a.Id == assignmentId && a.Status == AssignmentStatus.Published && a.Subject.ClassId == classId,
                cancellationToken)
            ?? throw new NotFoundException(nameof(Assignment), assignmentId);

        var submission = await _context.Submissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, cancellationToken);

        return ToResponse(assignment, submission);
    }

    private Guid RequireCurrentStudentId() =>
        _currentUserService.UserId ?? throw new UnauthorizedException("Unable to determine the current user.");

    private async Task<Guid> RequireStudentClassIdAsync(CancellationToken cancellationToken)
    {
        var studentId = RequireCurrentStudentId();

        var classId = await _context.Users
            .Where(u => u.Id == studentId)
            .Select(u => u.ClassId)
            .FirstOrDefaultAsync(cancellationToken);

        return classId ?? throw new BadRequestException("Your account is not assigned to a class. Contact an administrator.");
    }

    private static StudentAssignmentResponse ToResponse(Assignment assignment, Submission? submission) => new()
    {
        Id = assignment.Id,
        Title = assignment.Title,
        Description = assignment.Description,
        DeadlineUtc = assignment.DeadlineUtc,
        IsOverdue = DateTime.UtcNow > assignment.DeadlineUtc,
        MaxMarks = assignment.MaxMarks,
        SubjectId = assignment.SubjectId,
        SubjectName = assignment.Subject.Name,
        TeacherName = assignment.Teacher.FullName,
        HasSubmitted = submission is not null,
        SubmissionStatus = submission?.Status.ToString(),
        CreatedAtUtc = assignment.CreatedAtUtc
    };
}