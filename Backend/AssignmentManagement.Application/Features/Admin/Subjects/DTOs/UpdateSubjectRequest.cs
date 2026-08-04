using System.ComponentModel.DataAnnotations;

namespace AssignmentManagement.Application.Features.Admin.Subjects.DTOs;

public class UpdateSubjectRequest
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty;
}