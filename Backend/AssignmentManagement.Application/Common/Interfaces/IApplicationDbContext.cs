namespace AssignmentManagement.Application.Common.Interfaces;

/// <summary>
/// Abstraction over the persistence context so the Application layer never
/// depends directly on EF Core or Npgsql. Infrastructure provides the concrete
/// implementation (ApplicationDbContext). DbSet properties for each aggregate
/// will be added here as entities are introduced in later phases.
/// </summary>
public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
