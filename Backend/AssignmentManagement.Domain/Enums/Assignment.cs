using AssignmentManagement.Domain.Common;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Domain.Entities;

public class Assignment : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime DeadlineUtc { get; set; }

    public int MaxMarks { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    public Guid SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;

    public Guid TeacherId { get; set; }

    public User Teacher { get; set; } = null!;
}