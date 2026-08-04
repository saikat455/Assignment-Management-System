using AssignmentManagement.Application.Features.Admin.Classes;
using AssignmentManagement.Application.Features.Admin.Classes.DTOs;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/admin/classes")]
[Authorize(Roles = nameof(Role.Admin))]
public class AdminClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public AdminClassesController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpPost]
    public async Task<ActionResult<ClassResponse>> Create([FromBody] CreateClassRequest request, CancellationToken cancellationToken)
    {
        var result = await _classService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    public async Task<ActionResult<List<ClassResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _classService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClassResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _classService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClassResponse>> Update(Guid id, [FromBody] UpdateClassRequest request, CancellationToken cancellationToken)
    {
        var result = await _classService.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _classService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}