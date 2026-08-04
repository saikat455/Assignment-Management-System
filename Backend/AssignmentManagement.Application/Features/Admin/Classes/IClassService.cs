using AssignmentManagement.Application.Features.Admin.Classes.DTOs;

namespace AssignmentManagement.Application.Features.Admin.Classes;

public interface IClassService
{
    Task<ClassResponse> CreateAsync(CreateClassRequest request, CancellationToken cancellationToken = default);

    Task<List<ClassResponse>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ClassResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ClassResponse> UpdateAsync(Guid id, UpdateClassRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}