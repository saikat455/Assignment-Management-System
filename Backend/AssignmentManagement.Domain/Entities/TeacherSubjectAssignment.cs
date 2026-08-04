using AssignmentManagement.Domain.Common;

namespace AssignmentManagement.Domain.Entities;

public class TeacherSubjectAssignment : BaseEntity
{
    public Guid TeacherId { get; set; }

    public User Teacher { get; set; } = null!;

    public Guid SubjectId { get; set; }

    public Subject Subject { get; set; } = null!;
}