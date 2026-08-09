namespace AssignmentManagement.Application.Features.Teacher.Subjects;

/// A subject the current Teacher is assigned to teach - used to populate "create assignment" pickers.
public class TeacherSubjectOption
{
    public Guid SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public Guid ClassId { get; set; }

    public string ClassName { get; set; } = string.Empty;
}