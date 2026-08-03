using AssignmentManagement.Domain.Common;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Domain.Entities;


public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; }
    public bool IsActive { get; set; } = true;
}