using AssignmentManagement.Application.Features.Admin.Assignments;
using AssignmentManagement.Application.Features.Teacher.Assignments.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/admin/assignments")]
[Authorize(Roles = nameof(Role.Admin))]
public class AdminAssignmentsController : ControllerBase
{
    private readonly IAdminAssignmentService _adminAssignmentService;

    public AdminAssignmentsController(IAdminAssignmentService adminAssignmentService)
    {
        _adminAssignmentService = adminAssignmentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AssignmentResponse>>> GetAll(
        [FromQuery] Guid? teacherId,
        [FromQuery] Guid? subjectId,
        [FromQuery] Guid? classId,
        [FromQuery] AssignmentStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await _adminAssignmentService.GetAllAsync(teacherId, subjectId, classId, status, cancellationToken);
        return Ok(result);
    }
}