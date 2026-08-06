using AssignmentManagement.Application.Features.Teacher.Submissions.DTOs;

namespace AssignmentManagement.Application.Features.Teacher.Submissions;

public interface ITeacherSubmissionService
{
    Task<List<TeacherSubmissionResponse>> GetSubmissionsForAssignmentAsync(Guid assignmentId, CancellationToken cancellationToken = default);

    Task<TeacherSubmissionResponse> GetByIdAsync(Guid submissionId, CancellationToken cancellationToken = default);

    Task<TeacherSubmissionResponse> GradeAsync(Guid submissionId, GradeSubmissionRequest request, CancellationToken cancellationToken = default);

    Task<TeacherSubmissionResponse> UpdateStatusAsync(Guid submissionId, UpdateSubmissionStatusRequest request, CancellationToken cancellationToken = default);
}