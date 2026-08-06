using System.ComponentModel.DataAnnotations;

namespace AssignmentManagement.Application.Features.Student.Submissions.DTOs;

public class UpdateSubmissionRequest
{
    [Required, MaxLength(4000)]
    public string AnswerText { get; set; } = string.Empty;
}