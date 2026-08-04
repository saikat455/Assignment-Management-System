using System.ComponentModel.DataAnnotations;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Features.Admin.Users.DTOs;

public class UpdateUserRequest
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Role Role { get; set; }

    [Required]
    public bool IsActive { get; set; }

    /// <summary>Only meaningful when Role == Student.</summary>
    public Guid? ClassId { get; set; }
}