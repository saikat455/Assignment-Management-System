using AssignmentManagement.Application.Features.Admin.Subjects.DTOs;

namespace AssignmentManagement.Application.Features.Admin.Subjects;

public interface ISubjectService
{
    Task<SubjectResponse> CreateAsync(CreateSubjectRequest request, CancellationToken cancellationToken = default);

    Task<List<SubjectResponse>> GetAllAsync(Guid? classId, CancellationToken cancellationToken = default);

    Task<SubjectResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SubjectResponse> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}