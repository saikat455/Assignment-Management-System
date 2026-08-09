namespace AssignmentManagement.Application.Features.Teacher.Subjects;

public interface ITeacherSubjectService
{
    Task<List<TeacherSubjectOption>> GetMySubjectsAsync(CancellationToken cancellationToken = default);
}