using AssignmentManagement.Domain.Common;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Domain.Entities;

/// <summary>
/// A login-capable account. Admin, Teacher, and Student all share this single
/// table, distinguished by <see cref="Role"/>.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ClassId { get; set; }

    public SchoolClass? Class { get; set; }
}