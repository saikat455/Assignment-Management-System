using AssignmentManagement.Application.Features.Teacher.Submissions;
using AssignmentManagement.Application.Features.Teacher.Submissions.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Authorize(Roles = nameof(Role.Teacher))]
public class TeacherSubmissionsController : ControllerBase
{
    private readonly ITeacherSubmissionService _teacherSubmissionService;

    public TeacherSubmissionsController(ITeacherSubmissionService teacherSubmissionService)
    {
        _teacherSubmissionService = teacherSubmissionService;
    }

    [HttpGet("api/teacher/assignments/{assignmentId:guid}/submissions")]
    public async Task<ActionResult<List<TeacherSubmissionResponse>>> GetForAssignment(Guid assignmentId, CancellationToken cancellationToken)
    {
        var result = await _teacherSubmissionService.GetSubmissionsForAssignmentAsync(assignmentId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("api/teacher/submissions/{id:guid}")]
    public async Task<ActionResult<TeacherSubmissionResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _teacherSubmissionService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("api/teacher/submissions/{id:guid}/grade")]
    public async Task<ActionResult<TeacherSubmissionResponse>> Grade(
        Guid id, [FromBody] GradeSubmissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _teacherSubmissionService.GradeAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPatch("api/teacher/submissions/{id:guid}/status")]
    public async Task<ActionResult<TeacherSubmissionResponse>> UpdateStatus(
        Guid id, [FromBody] UpdateSubmissionStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _teacherSubmissionService.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(result);
    }
}