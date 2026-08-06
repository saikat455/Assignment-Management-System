namespace AssignmentManagement.Application.Features.Student.Assignments.DTOs;

public class StudentAssignmentResponse
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime DeadlineUtc { get; set; }

    public bool IsOverdue { get; set; }

    public int MaxMarks { get; set; }

    public Guid SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public string TeacherName { get; set; } = string.Empty;

    public bool HasSubmitted { get; set; }

    public string? SubmissionStatus { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}