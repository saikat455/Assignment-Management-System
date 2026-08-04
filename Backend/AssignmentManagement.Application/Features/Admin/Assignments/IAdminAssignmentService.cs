using AssignmentManagement.Application.Features.Teacher.Assignments.DTOs;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Features.Admin.Assignments;

public interface IAdminAssignmentService
{
    Task<List<AssignmentResponse>> GetAllAsync(
        Guid? teacherId, Guid? subjectId, Guid? classId, AssignmentStatus? status,
        CancellationToken cancellationToken = default);
}