using System.ComponentModel.DataAnnotations;

namespace AssignmentManagement.Application.Features.Student.Submissions.DTOs;

public class CreateSubmissionRequest
{
    [Required, MaxLength(4000)]
    public string AnswerText { get; set; } = string.Empty;
}