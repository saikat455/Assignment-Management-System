using AssignmentManagement.Application.Features.Admin.Subjects;
using AssignmentManagement.Application.Features.Admin.Subjects.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/admin/subjects")]
[Authorize(Roles = nameof(Role.Admin))]
public class AdminSubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public AdminSubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpPost]
    public async Task<ActionResult<SubjectResponse>> Create([FromBody] CreateSubjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _subjectService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<SubjectResponse>>> GetAll([FromQuery] Guid? classId, CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetAllAsync(classId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubjectResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _subjectService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SubjectResponse>> Update(Guid id, [FromBody] UpdateSubjectRequest request, CancellationToken cancellationToken)
    {
        var result = await _subjectService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _subjectService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}