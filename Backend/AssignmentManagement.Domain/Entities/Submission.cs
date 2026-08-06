using AssignmentManagement.Domain.Common;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Domain.Entities;

public class Submission : BaseEntity
{
    public Guid AssignmentId { get; set; }

    public Assignment Assignment { get; set; } = null!;

    public Guid StudentId { get; set; }

    public User Student { get; set; } = null!;

    public string AnswerText { get; set; } = string.Empty;

    public DateTime SubmittedAtUtc { get; set; }

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    /// <summary>Set once a Teacher grades the submission (Phase 6).</summary>
    public int? MarksObtained { get; set; }

    /// <summary>Set once a Teacher leaves feedback (Phase 6).</summary>
    public string? Feedback { get; set; }
}