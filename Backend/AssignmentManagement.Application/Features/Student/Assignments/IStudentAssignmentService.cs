using AssignmentManagement.Application.Features.Student.Assignments.DTOs;

namespace AssignmentManagement.Application.Features.Student.Assignments;

public interface IStudentAssignmentService
{
    Task<List<StudentAssignmentResponse>> GetAssignmentsAsync(Guid? subjectId, CancellationToken cancellationToken = default);

    Task<StudentAssignmentResponse> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}