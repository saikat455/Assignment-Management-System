namespace AssignmentManagement.Application.Features.Admin.Classes.DTOs;

public class ClassResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Section { get; set; }

    public int StudentCount { get; set; }

    public int SubjectCount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}