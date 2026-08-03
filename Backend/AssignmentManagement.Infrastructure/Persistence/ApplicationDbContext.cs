using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Persistence;

/// <summary>
/// EF Core database context targeting PostgreSQL (via Npgsql).
/// DbSet properties for domain entities (Users, Classes, Subjects, Assignments,
/// Submissions, etc.) will be added incrementally as each module is built out
/// in later phases, together with their EntityTypeConfiguration classes under
/// Persistence/Configurations.
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply all IEntityTypeConfiguration<T> classes found in this assembly.
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
