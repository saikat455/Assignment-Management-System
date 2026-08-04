using AssignmentManagement.Domain.Common;

namespace AssignmentManagement.Domain.Entities;

public class SchoolClass : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Section { get; set; }

    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    public ICollection<User> Students { get; set; } = new List<User>();
}