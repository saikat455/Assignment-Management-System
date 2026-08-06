using System.ComponentModel.DataAnnotations;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Features.Teacher.Submissions.DTOs;

public class UpdateSubmissionStatusRequest
{
    [Required]
    public SubmissionStatus Status { get; set; }
}