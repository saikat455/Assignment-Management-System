using AssignmentManagement.Application.Features.Admin.TeacherAssignments.DTOs;

namespace AssignmentManagement.Application.Features.Admin.TeacherAssignments;

public interface ITeacherAssignmentService
{
    Task<TeacherAssignmentResponse> AssignAsync(CreateTeacherAssignmentRequest request, CancellationToken cancellationToken = default);

    Task<List<TeacherAssignmentResponse>> GetAllAsync(Guid? teacherId, Guid? subjectId, Guid? classId, CancellationToken cancellationToken = default);

    Task UnassignAsync(Guid id, CancellationToken cancellationToken = default);
}