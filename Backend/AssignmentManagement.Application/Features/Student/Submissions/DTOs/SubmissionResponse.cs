
namespace AssignmentManagement.Application.Features.Student.Submissions.DTOs;

public class SubmissionResponse
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public string AnswerText { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public int? MarksObtained { get; set; }

    public int MaxMarks { get; set; }

    public string? Feedback { get; set; }
}