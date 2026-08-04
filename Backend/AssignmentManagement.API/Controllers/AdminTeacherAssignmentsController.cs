using AssignmentManagement.Application.Features.Admin.TeacherAssignments;
using AssignmentManagement.Application.Features.Admin.TeacherAssignments.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/admin/teacher-assignments")]
[Authorize(Roles = nameof(Role.Admin))]
public class AdminTeacherAssignmentsController : ControllerBase
{
    private readonly ITeacherAssignmentService _teacherAssignmentService;

    public AdminTeacherAssignmentsController(ITeacherAssignmentService teacherAssignmentService)
    {
        _teacherAssignmentService = teacherAssignmentService;
    }

    [HttpPost]
    public async Task<ActionResult<TeacherAssignmentResponse>> Assign(
        [FromBody] CreateTeacherAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _teacherAssignmentService.AssignAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<TeacherAssignmentResponse>>> GetAll(
        [FromQuery] Guid? teacherId, [FromQuery] Guid? subjectId, [FromQuery] Guid? classId, CancellationToken cancellationToken)
    {
        var result = await _teacherAssignmentService.GetAllAsync(teacherId, subjectId, classId, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Unassign(Guid id, CancellationToken cancellationToken)
    {
        await _teacherAssignmentService.UnassignAsync(id, cancellationToken);
        return NoContent();
    }
}