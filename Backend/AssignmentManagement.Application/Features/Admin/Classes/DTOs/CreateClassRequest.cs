using System.ComponentModel.DataAnnotations;

namespace AssignmentManagement.Application.Features.Admin.Classes.DTOs;

public class CreateClassRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Section { get; set; }
}