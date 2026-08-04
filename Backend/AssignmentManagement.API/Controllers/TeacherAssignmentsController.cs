using AssignmentManagement.Application.Features.Teacher.Assignments;
using AssignmentManagement.Application.Features.Teacher.Assignments.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/teacher/assignments")]
[Authorize(Roles = nameof(Role.Teacher))]
public class TeacherAssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;

    public TeacherAssignmentsController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    [HttpPost]
    public async Task<ActionResult<AssignmentResponse>> Create([FromBody] CreateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<AssignmentResponse>>> GetMine(
        [FromQuery] Guid? subjectId, [FromQuery] AssignmentStatus? status, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GetMyAssignmentsAsync(subjectId, status, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssignmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<AssignmentResponse>> Update(Guid id, [FromBody] UpdateAssignmentRequest request, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _assignmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/publish")]
    public async Task<ActionResult<AssignmentResponse>> Publish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.PublishAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/unpublish")]
    public async Task<ActionResult<AssignmentResponse>> Unpublish(Guid id, CancellationToken cancellationToken)
    {
        var result = await _assignmentService.UnpublishAsync(id, cancellationToken);
        return Ok(result);
    }
}