using System.ComponentModel.DataAnnotations;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Features.Auth.DTOs;

public class RegisterRequest
{
    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public Role Role { get; set; }
}