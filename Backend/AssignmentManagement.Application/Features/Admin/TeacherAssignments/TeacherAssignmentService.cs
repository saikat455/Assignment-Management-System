using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Admin.TeacherAssignments.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Admin.TeacherAssignments;

public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly IApplicationDbContext _context;

    public TeacherAssignmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TeacherAssignmentResponse> AssignAsync(CreateTeacherAssignmentRequest request, CancellationToken cancellationToken = default)
    {
        var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.TeacherId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.TeacherId);

        if (teacher.Role != Role.Teacher)
        {
            throw new BadRequestException("The selected user is not a Teacher.");
        }

        if (!teacher.IsActive)
        {
            throw new BadRequestException("This teacher account is deactivated.");
        }

        var subject = await _context.Subjects.Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == request.SubjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Subject), request.SubjectId);

        var alreadyAssigned = await _context.TeacherAssignments.AnyAsync(
            t => t.TeacherId == request.TeacherId && t.SubjectId == request.SubjectId,
            cancellationToken);

        if (alreadyAssigned)
        {
            throw new ConflictException("This teacher is already assigned to this subject.");
        }

        var assignment = new TeacherSubjectAssignment
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId
        };

        _context.TeacherAssignments.Add(assignment);
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(assignment, teacher, subject);
    }

    public async Task<List<TeacherAssignmentResponse>> GetAllAsync(
        Guid? teacherId, Guid? subjectId, Guid? classId, CancellationToken cancellationToken = default)
    {
        var query = _context.TeacherAssignments
            .Include(t => t.Teacher)
            .Include(t => t.Subject).ThenInclude(s => s.Class)
            .AsQueryable();

        if (teacherId is not null)
        {
            query = query.Where(t => t.TeacherId == teacherId);
        }

        if (subjectId is not null)
        {
            query = query.Where(t => t.SubjectId == subjectId);
        }

        if (classId is not null)
        {
            query = query.Where(t => t.Subject.ClassId == classId);
        }

        var assignments = await query.OrderBy(t => t.Subject.Name).ToListAsync(cancellationToken);

        return assignments.Select(a => ToResponse(a, a.Teacher, a.Subject)).ToList();
    }

    public async Task UnassignAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var assignment = await _context.TeacherAssignments.FirstOrDefaultAsync(t => t.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(TeacherSubjectAssignment), id);

        _context.TeacherAssignments.Remove(assignment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private static TeacherAssignmentResponse ToResponse(TeacherSubjectAssignment assignment, User teacher, Subject subject) => new()
    {
        Id = assignment.Id,
        TeacherId = teacher.Id,
        TeacherName = teacher.FullName,
        SubjectId = subject.Id,
        SubjectName = subject.Name,
        ClassId = subject.ClassId,
        ClassName = string.IsNullOrWhiteSpace(subject.Class.Section) ? subject.Class.Name : $"{subject.Class.Name} - {subject.Class.Section}",
        CreatedAtUtc = assignment.CreatedAtUtc
    };
}