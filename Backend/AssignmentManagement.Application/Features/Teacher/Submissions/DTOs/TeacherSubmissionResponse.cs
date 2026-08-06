namespace AssignmentManagement.Application.Features.Teacher.Submissions.DTOs;

public class TeacherSubmissionResponse
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    public string AssignmentTitle { get; set; } = string.Empty;

    public int MaxMarks { get; set; }

    public Guid StudentId { get; set; }

    public string StudentName { get; set; } = string.Empty;

    public string StudentEmail { get; set; } = string.Empty;

    public string AnswerText { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    public int? MarksObtained { get; set; }

    public string? Feedback { get; set; }
}