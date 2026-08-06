using AssignmentManagement.Application.Features.Student.Submissions;
using AssignmentManagement.Application.Features.Student.Submissions.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/student/submissions")]
[Authorize(Roles = nameof(Role.Student))]
public class StudentSubmissionsController : ControllerBase
{
    private readonly IStudentSubmissionService _studentSubmissionService;

    public StudentSubmissionsController(IStudentSubmissionService studentSubmissionService)
    {
        _studentSubmissionService = studentSubmissionService;
    }

    [HttpGet]
    public async Task<ActionResult<List<SubmissionResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _studentSubmissionService.GetMySubmissionsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{assignmentId:guid}")]
    public async Task<ActionResult<SubmissionResponse>> GetForAssignment(Guid assignmentId, CancellationToken cancellationToken)
    {
        var result = await _studentSubmissionService.GetMySubmissionForAssignmentAsync(assignmentId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{assignmentId:guid}")]
    public async Task<ActionResult<SubmissionResponse>> Submit(
        Guid assignmentId, [FromBody] CreateSubmissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentSubmissionService.SubmitAsync(assignmentId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPut("{assignmentId:guid}")]
    public async Task<ActionResult<SubmissionResponse>> Update(
        Guid assignmentId, [FromBody] UpdateSubmissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _studentSubmissionService.UpdateAsync(assignmentId, request, cancellationToken);
        return Ok(result);
    }
}