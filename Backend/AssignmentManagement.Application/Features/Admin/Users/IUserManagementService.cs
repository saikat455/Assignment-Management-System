using AssignmentManagement.Application.Features.Admin.Users.DTOs;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Features.Admin.Users;

public interface IUserManagementService
{
    Task<List<UserResponse>> GetAllAsync(Role? role, CancellationToken cancellationToken = default);

    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserResponse> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default);

    Task DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}