using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Admin.Subjects.DTOs;
using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Admin.Subjects;

public class SubjectService : ISubjectService
{
    private readonly IApplicationDbContext _context;

    public SubjectService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SubjectResponse> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var schoolClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == request.ClassId, cancellationToken)
            ?? throw new NotFoundException(nameof(SchoolClass), request.ClassId);

        var code = request.Code.Trim().ToUpperInvariant();

        var duplicate = await _context.Subjects.AnyAsync(
            s => s.ClassId == request.ClassId && s.Code.ToUpper() == code,
            cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Subject code '{code}' already exists for this class.");
        }

        var subject = new Subject
        {
            Name = request.Name.Trim(),
            Code = code,
            ClassId = request.ClassId
        };

        _context.Subjects.Add(subject);
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(subject, schoolClass, teacherCount: 0);
    }

    public async Task<List<SubjectResponse>> GetAllAsync(Guid? classId, CancellationToken cancellationToken = default)
    {
        var query = _context.Subjects.Include(s => s.Class).AsQueryable();

        if (classId is not null)
        {
            query = query.Where(s => s.ClassId == classId);
        }

        var subjects = await query
            .Select(s => new { Subject = s, TeacherCount = s.TeacherAssignments.Count })
            .OrderBy(x => x.Subject.Name)
            .ToListAsync(cancellationToken);

        return subjects.Select(x => ToResponse(x.Subject, x.Subject.Class, x.TeacherCount)).ToList();
    }

    public async Task<SubjectResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subject = await FindSubjectOrThrowAsync(id, cancellationToken);
        var teacherCount = await _context.TeacherAssignments.CountAsync(t => t.SubjectId == id, cancellationToken);

        return ToResponse(subject, subject.Class, teacherCount);
    }

    public async Task<SubjectResponse> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken = default)
    {
        var subject = await FindSubjectOrThrowAsync(id, cancellationToken);

        var code = request.Code.Trim().ToUpperInvariant();

        var duplicate = await _context.Subjects.AnyAsync(
            s => s.Id != id && s.ClassId == subject.ClassId && s.Code.ToUpper() == code,
            cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"Subject code '{code}' already exists for this class.");
        }

        subject.Name = request.Name.Trim();
        subject.Code = code;

        await _context.SaveChangesAsync(cancellationToken);

        var teacherCount = await _context.TeacherAssignments.CountAsync(t => t.SubjectId == id, cancellationToken);

        return ToResponse(subject, subject.Class, teacherCount);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var subject = await FindSubjectOrThrowAsync(id, cancellationToken);

        var hasTeacherAssignments = await _context.TeacherAssignments.AnyAsync(t => t.SubjectId == id, cancellationToken);
        if (hasTeacherAssignments)
        {
            throw new ConflictException("This subject cannot be deleted while it still has a teacher assigned.");
        }

        _context.Subjects.Remove(subject);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<Subject> FindSubjectOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var subject = await _context.Subjects.Include(s => s.Class)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        return subject ?? throw new NotFoundException(nameof(Subject), id);
    }

    private static SubjectResponse ToResponse(Subject subject, SchoolClass schoolClass, int teacherCount) => new()
    {
        Id = subject.Id,
        Name = subject.Name,
        Code = subject.Code,
        ClassId = subject.ClassId,
        ClassName = string.IsNullOrWhiteSpace(schoolClass.Section) ? schoolClass.Name : $"{schoolClass.Name} - {schoolClass.Section}",
        TeacherCount = teacherCount,
        CreatedAtUtc = subject.CreatedAtUtc
    };
}