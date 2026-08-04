namespace AssignmentManagement.Application.Features.Admin.TeacherAssignments.DTOs;

public class TeacherAssignmentResponse
{
    public Guid Id { get; set; }

    public Guid TeacherId { get; set; }

    public string TeacherName { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public Guid ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}