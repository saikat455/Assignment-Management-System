using AssignmentManagement.Application.Features.Teacher.Subjects;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/teacher/subjects")]
[Authorize(Roles = nameof(Role.Teacher))]
public class TeacherSubjectsController : ControllerBase
{
    private readonly ITeacherSubjectService _teacherSubjectService;

    public TeacherSubjectsController(ITeacherSubjectService teacherSubjectService)
    {
        _teacherSubjectService = teacherSubjectService;
    }

    //Subjects the current Teacher is assigned to - used to populate "create assignment" pickers.
    [HttpGet]
    public async Task<ActionResult<List<TeacherSubjectOption>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _teacherSubjectService.GetMySubjectsAsync(cancellationToken);
        return Ok(result);
    }
}