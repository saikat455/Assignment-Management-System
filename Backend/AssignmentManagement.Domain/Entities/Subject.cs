using AssignmentManagement.Domain.Common;

namespace AssignmentManagement.Domain.Entities;

public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public Guid ClassId { get; set; }

    public SchoolClass Class { get; set; } = null!;

    public ICollection<TeacherSubjectAssignment> TeacherAssignments { get; set; } = new List<TeacherSubjectAssignment>();
}