using System.ComponentModel.DataAnnotations;

namespace AssignmentManagement.Application.Features.Teacher.Assignments.DTOs;

public class CreateAssignmentRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(4000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime DeadlineUtc { get; set; }

    [Required, Range(1, 1000)]
    public int MaxMarks { get; set; }

    [Required]
    public Guid SubjectId { get; set; }
    public bool PublishImmediately { get; set; } = false;
}