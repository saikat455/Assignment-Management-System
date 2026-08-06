using AssignmentManagement.Application.Features.Student.Submissions.DTOs;

namespace AssignmentManagement.Application.Features.Student.Submissions;

public interface IStudentSubmissionService
{
    Task<SubmissionResponse> SubmitAsync(Guid assignmentId, CreateSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<SubmissionResponse> UpdateAsync(Guid assignmentId, UpdateSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<List<SubmissionResponse>> GetMySubmissionsAsync(CancellationToken cancellationToken = default);

    Task<SubmissionResponse> GetMySubmissionForAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);
}