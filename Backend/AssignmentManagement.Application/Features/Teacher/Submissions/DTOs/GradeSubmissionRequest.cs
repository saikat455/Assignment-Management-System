using System.ComponentModel.DataAnnotations;

namespace AssignmentManagement.Application.Features.Teacher.Submissions.DTOs;

public class GradeSubmissionRequest
{
    [Required, Range(0, int.MaxValue)]
    public int MarksObtained { get; set; }

    [MaxLength(2000)]
    public string? Feedback { get; set; }
}