using System.ComponentModel.DataAnnotations;

namespace AssignmentManagement.Application.Features.Admin.TeacherAssignments.DTOs;

public class CreateTeacherAssignmentRequest
{
    [Required]
    public Guid TeacherId { get; set; }

    [Required]
    public Guid SubjectId { get; set; }
}