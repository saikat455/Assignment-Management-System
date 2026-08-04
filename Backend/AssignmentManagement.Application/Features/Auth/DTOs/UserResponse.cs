namespace AssignmentManagement.Application.Features.Admin.Users.DTOs;

public class UserResponse
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public Guid? ClassId { get; set; }

    public string? ClassName { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}