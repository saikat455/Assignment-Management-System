namespace AssignmentManagement.Domain.Common;

/// <summary>
/// Base class for all domain entities. Provides a strongly typed primary key
/// and audit fields shared across the domain model.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
