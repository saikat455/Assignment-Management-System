using AssignmentManagement.Application.Features.Student.Assignments;
using AssignmentManagement.Application.Features.Student.Assignments.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/student/assignments")]
[Authorize(Roles = nameof(Role.Student))]
public class StudentAssignmentsController : ControllerBase
{
    private readonly IStudentAssignmentService _studentAssignmentService;

    public StudentAssignmentsController(IStudentAssignmentService studentAssignmentService)
    {
        _studentAssignmentService = studentAssignmentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StudentAssignmentResponse>>> GetAll([FromQuery] Guid? subjectId, CancellationToken cancellationToken)
    {
        var result = await _studentAssignmentService.GetAssignmentsAsync(subjectId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StudentAssignmentResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _studentAssignmentService.GetAssignmentByIdAsync(id, cancellationToken);
        return Ok(result);
    }
}