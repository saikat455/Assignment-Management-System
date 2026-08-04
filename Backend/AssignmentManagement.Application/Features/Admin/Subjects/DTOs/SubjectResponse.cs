namespace AssignmentManagement.Application.Features.Admin.Subjects.DTOs;

public class SubjectResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public Guid ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;

    public int TeacherCount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}