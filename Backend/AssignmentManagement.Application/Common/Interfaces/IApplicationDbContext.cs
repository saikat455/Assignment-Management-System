using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<SchoolClass> Classes { get; }

    DbSet<Subject> Subjects { get; }

    DbSet<TeacherSubjectAssignment> TeacherAssignments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}