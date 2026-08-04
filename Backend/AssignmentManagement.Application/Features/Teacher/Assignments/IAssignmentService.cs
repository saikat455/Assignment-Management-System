using AssignmentManagement.Application.Features.Teacher.Assignments.DTOs;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Features.Teacher.Assignments;

public interface IAssignmentService
{
    Task<AssignmentResponse> CreateAsync(CreateAssignmentRequest request, CancellationToken cancellationToken = default);

    Task<List<AssignmentResponse>> GetMyAssignmentsAsync(Guid? subjectId, AssignmentStatus? status, CancellationToken cancellationToken = default);

    Task<AssignmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssignmentResponse> UpdateAsync(Guid id, UpdateAssignmentRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssignmentResponse> PublishAsync(Guid id, CancellationToken cancellationToken = default);

    Task<AssignmentResponse> UnpublishAsync(Guid id, CancellationToken cancellationToken = default);
}