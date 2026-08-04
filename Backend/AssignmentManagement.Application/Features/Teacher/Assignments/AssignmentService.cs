using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Teacher.Assignments.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Teacher.Assignments;

public class AssignmentService : IAssignmentService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public AssignmentService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<AssignmentResponse> CreateAsync(CreateAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var teacherId = RequireCurrentTeacherId();

        if (request.DeadlineUtc <= DateTime.UtcNow)
        {
            throw new BadRequestException("Deadline must be in the future.");
        }

        var subject = await _context.Subjects.Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Subject), request.SubjectId);

        var isAssignedToSubject = await _context.TeacherAssignments.AnyAsync(
            t => t.TeacherId == teacherId && t.SubjectId == request.SubjectId,
            cancellationToken);

        if (!isAssignedToSubject)
        {
            throw new ForbiddenException("You are not assigned to teach this subject.");
        }

        var assignment = new Assignment
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            DeadlineUtc = request.DeadlineUtc,
            MaxMarks = request.MaxMarks,
            SubjectId = request.SubjectId,
            TeacherId = teacherId,
            Status = request.PublishImmediately ? AssignmentStatus.Published : AssignmentStatus.Draft
        };

        _context.Assignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        var teacherName = _context.Users.First(u => u.Id == teacherId).FullName;

        return ToResponse(assignment, subject, teacherName);
    }

    public async Task<List<AssignmentResponse>> GetMyAssignmentsAsync(
        Guid? subjectId, AssignmentStatus? status, CancellationToken cancellationToken = default)
    {
        var teacherId = RequireCurrentTeacherId();

        var query = _context.Assignments
            .Include(a => a.Subject).ThenInclude(s => s.Class)
            .Include(a => a.Teacher)
            .Where(a => a.TeacherId == teacherId);

        if (subjectId is not null)
        {
            query = query.Where(a => a.SubjectId == subjectId);
        }

        if (status is not null)
        {
            query = query.Where(a => a.Status == status);
        }

        var assignments = await query.OrderByDescending(a => a.CreatedAtUtc).ToListAsync(cancellationToken);

        return assignments.Select(a => ToResponse(a, a.Subject, a.Teacher.FullName)).ToList();
    }

    public async Task<AssignmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await FindOwnedAssignmentOrThrowAsync(id, cancellationToken);
        return ToResponse(assignment, assignment.Subject, assignment.Teacher.FullName);
    }

    public async Task<AssignmentResponse> UpdateAsync(Guid id, UpdateAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var assignment = await FindOwnedAssignmentOrThrowAsync(id, cancellationToken);

        if (request.DeadlineUtc <= DateTime.UtcNow)
        {
            throw new BadRequestException("Deadline must be in the future.");
        }

        assignment.Title = request.Title.Trim();
        assignment.Description = request.Description.Trim();
        assignment.DeadlineUtc = request.DeadlineUtc;
        assignment.MaxMarks = request.MaxMarks;

        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(assignment, assignment.Subject, assignment.Teacher.FullName);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await FindOwnedAssignmentOrThrowAsync(id, cancellationToken);
        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AssignmentResponse> PublishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await FindOwnedAssignmentOrThrowAsync(id, cancellationToken);

        if (assignment.Status == AssignmentStatus.Published)
        {
            throw new ConflictException("This assignment is already published.");
        }

        assignment.Status = AssignmentStatus.Published;
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(assignment, assignment.Subject, assignment.Teacher.FullName);
    }

    public async Task<AssignmentResponse> UnpublishAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await FindOwnedAssignmentOrThrowAsync(id, cancellationToken);

        if (assignment.Status == AssignmentStatus.Draft)
        {
            throw new ConflictException("This assignment is already a draft.");
        }

        assignment.Status = AssignmentStatus.Draft;
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(assignment, assignment.Subject, assignment.Teacher.FullName);
    }

    private Guid RequireCurrentTeacherId() =>
        _currentUserService.UserId ?? throw new UnauthorizedException("Unable to determine the current user.");

    private async Task<Assignment> FindOwnedAssignmentOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var teacherId = RequireCurrentTeacherId();

        var assignment = await _context.Assignments
            .Include(a => a.Subject).ThenInclude(s => s.Class)
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Assignment), id);

        if (assignment.TeacherId != teacherId)
        {
            throw new ForbiddenException("You do not have access to this assignment.");
        }

        return assignment;
    }

    private static AssignmentResponse ToResponse(Assignment assignment, Subject subject, string teacherName) => new()
    {
        Id = assignment.Id,
        Title = assignment.Title,
        Description = assignment.Description,
        DeadlineUtc = assignment.DeadlineUtc,
        MaxMarks = assignment.MaxMarks,
        Status = assignment.Status.ToString(),
        SubjectId = subject.Id,
        SubjectName = subject.Name,
        ClassId = subject.ClassId,
        ClassName = string.IsNullOrWhiteSpace(subject.Class.Section) ? subject.Class.Name : $"{subject.Class.Name} - {subject.Class.Section}",
        TeacherId = assignment.TeacherId,
        TeacherName = teacherName,
        CreatedAtUtc = assignment.CreatedAtUtc,
        UpdatedAtUtc = assignment.UpdatedAtUtc
    };
}