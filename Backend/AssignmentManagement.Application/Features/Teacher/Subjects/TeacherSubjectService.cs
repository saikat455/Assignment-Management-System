using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Features.Teacher.Subjects;

public class TeacherSubjectService : ITeacherSubjectService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public TeacherSubjectService(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<TeacherSubjectOption>> GetMySubjectsAsync(CancellationToken cancellationToken = default)
    {
        var teacherId = _currentUserService.UserId
            ?? throw new UnauthorizedException("Unable to determine the current user.");

        return await _context.TeacherAssignments
            .Include(t => t.Subject).ThenInclude(s => s.Class)
            .Where(t => t.TeacherId == teacherId)
            .OrderBy(t => t.Subject.Name)
            .Select(t => new TeacherSubjectOption
            {
                SubjectId = t.Subject.Id,
                SubjectName = t.Subject.Name,
                ClassId = t.Subject.ClassId,
                ClassName = string.IsNullOrWhiteSpace(t.Subject.Class.Section)
                    ? t.Subject.Class.Name
                    : t.Subject.Class.Name + " - " + t.Subject.Class.Section
            })
            .ToListAsync(cancellationToken);
    }
}