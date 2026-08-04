using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Features.Admin.Classes.DTOs;
using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Admin.Classes;

public class ClassService : IClassService
{
    private readonly IApplicationDbContext _context;

    public ClassService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var section = string.IsNullOrWhiteSpace(request.Section) ? null : request.Section.Trim();

        var duplicate = await _context.Classes.AnyAsync(
            c => c.Name.ToLower() == name.ToLower() && c.Section == section,
            cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"A class named '{name}'{(section is null ? "" : $" (section {section})")} already exists.");
        }

        var schoolClass = new SchoolClass { Name = name, Section = section };

        _context.Classes.Add(schoolClass);
        await _context.SaveChangesAsync(cancellationToken);

        return ToResponse(schoolClass, studentCount: 0, subjectCount: 0);
    }

    public async Task<List<ClassResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var classes = await _context.Classes
            .Select(c => new
            {
                Class = c,
                StudentCount = c.Students.Count,
                SubjectCount = c.Subjects.Count
            })
            .OrderBy(x => x.Class.Name)
            .ToListAsync(cancellationToken);

        return classes.Select(x => ToResponse(x.Class, x.StudentCount, x.SubjectCount)).ToList();
    }

    public async Task<ClassResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schoolClass = await FindClassOrThrowAsync(id, cancellationToken);
        var studentCount = await _context.Users.CountAsync(u => u.ClassId == id, cancellationToken);
        var subjectCount = await _context.Subjects.CountAsync(s => s.ClassId == id, cancellationToken);

        return ToResponse(schoolClass, studentCount, subjectCount);
    }

    public async Task<ClassResponse> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken = default)
    {
        var schoolClass = await FindClassOrThrowAsync(id, cancellationToken);

        var name = request.Name.Trim();
        var section = string.IsNullOrWhiteSpace(request.Section) ? null : request.Section.Trim();

        var duplicate = await _context.Classes.AnyAsync(
            c => c.Id != id && c.Name.ToLower() == name.ToLower() && c.Section == section,
            cancellationToken);

        if (duplicate)
        {
            throw new ConflictException($"A class named '{name}'{(section is null ? "" : $" (section {section})")} already exists.");
        }

        schoolClass.Name = name;
        schoolClass.Section = section;

        await _context.SaveChangesAsync(cancellationToken);

        var studentCount = await _context.Users.CountAsync(u => u.ClassId == id, cancellationToken);
        var subjectCount = await _context.Subjects.CountAsync(s => s.ClassId == id, cancellationToken);

        return ToResponse(schoolClass, studentCount, subjectCount);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var schoolClass = await FindClassOrThrowAsync(id, cancellationToken);

        var hasStudents = await _context.Users.AnyAsync(u => u.ClassId == id, cancellationToken);
        var hasSubjects = await _context.Subjects.AnyAsync(s => s.ClassId == id, cancellationToken);

        if (hasStudents || hasSubjects)
        {
            throw new ConflictException("This class cannot be deleted because it still has students or subjects assigned to it.");
        }

        _context.Classes.Remove(schoolClass);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task<SchoolClass> FindClassOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var schoolClass = await _context.Classes.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        return schoolClass ?? throw new NotFoundException(nameof(SchoolClass), id);
    }

    private static ClassResponse ToResponse(SchoolClass schoolClass, int studentCount, int subjectCount) => new()
    {
        Id = schoolClass.Id,
        Name = schoolClass.Name,
        Section = schoolClass.Section,
        StudentCount = studentCount,
        SubjectCount = subjectCount,
        CreatedAtUtc = schoolClass.CreatedAtUtc
    };
}