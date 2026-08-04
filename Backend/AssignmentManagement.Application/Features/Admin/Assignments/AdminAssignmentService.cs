using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Teacher.Assignments.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Admin.Assignments;

public class AdminAssignmentService : IAdminAssignmentService
{
    private readonly IApplicationDbContext _context;

    public AdminAssignmentService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssignmentResponse>> GetAllAsync(
        Guid? teacherId, Guid? subjectId, Guid? classId, AssignmentStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Assignments
            .Include(a => a.Subject).ThenInclude(s => s.Class)
            .Include(a => a.Teacher)
            .AsQueryable();

        if (teacherId is not null)
        {
            query = query.Where(a => a.TeacherId == teacherId);
        }

        if (subjectId is not null)
        {
            query = query.Where(a => a.SubjectId == subjectId);
        }

        if (classId is not null)
        {
            query = query.Where(a => a.Subject.ClassId == classId);
        }

        if (status is not null)
        {
            query = query.Where(a => a.Status == status);
        }

        var assignments = await query.OrderByDescending(a => a.CreatedAtUtc).ToListAsync(cancellationToken);

        return assignments.Select(a => new AssignmentResponse
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            DeadlineUtc = a.DeadlineUtc,
            MaxMarks = a.MaxMarks,
            Status = a.Status.ToString(),
            SubjectId = a.SubjectId,
            SubjectName = a.Subject.Name,
            ClassId = a.Subject.ClassId,
            ClassName = string.IsNullOrWhiteSpace(a.Subject.Class.Section) ? a.Subject.Class.Name : $"{a.Subject.Class.Name} - {a.Subject.Class.Section}",
            TeacherId = a.TeacherId,
            TeacherName = a.Teacher.FullName,
            CreatedAtUtc = a.CreatedAtUtc,
            UpdatedAtUtc = a.UpdatedAtUtc
        }).ToList();
    }
}