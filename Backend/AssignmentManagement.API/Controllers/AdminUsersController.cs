using AssignmentManagement.Application.Features.Admin.Users;
using AssignmentManagement.Application.Features.Admin.Users.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = nameof(Role.Admin))]
public class AdminUsersController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public AdminUsersController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll([FromQuery] Role? role, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetAllAsync(role, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<UserResponse>> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await _userManagementService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Soft-deletes (deactivates) a user. Historical data is preserved.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _userManagementService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}