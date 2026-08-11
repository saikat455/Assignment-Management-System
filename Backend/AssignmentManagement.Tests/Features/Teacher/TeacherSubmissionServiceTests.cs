using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Features.Teacher.Submissions;
using AssignmentManagement.Application.Features.Teacher.Submissions.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Infrastructure.Persistence;
using AssignmentManagement.Tests.Common;

namespace AssignmentManagement.Tests.Features.Teacher;

public class TeacherSubmissionServiceTests
{
    private static (ApplicationDbContext Context, Assignment Assignment, Submission Submission, User Teacher, User OtherTeacher) SeedSubmission()
    {
        var context = TestDbContextFactory.Create();

        var schoolClass = new SchoolClass { Name = "Class 10", Section = "A" };
        var subject = new Subject { Name = "Math", Code = "MATH101", Class = schoolClass };
        var teacher = new User { FullName = "Teacher", Email = "t@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        var otherTeacher = new User { FullName = "Other Teacher", Email = "t2@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        var student = new User { FullName = "Student", Email = "s@school.test", PasswordHash = "x", Role = Role.Student, Class = schoolClass, IsActive = true };

        context.Classes.Add(schoolClass);
        context.Subjects.Add(subject);
        context.Users.AddRange(teacher, otherTeacher, student);
        context.SaveChanges();

        var assignment = new Assignment
        {
            Title = "Worksheet",
            Description = "desc",
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(1),
            MaxMarks = 100,
            Status = AssignmentStatus.Published
        };
        context.Assignments.Add(assignment);
        context.SaveChanges();

        var submission = new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = student.Id,
            AnswerText = "answer",
            SubmittedAtUtc = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        };
        context.Submissions.Add(submission);
        context.SaveChanges();

        return (context, assignment, submission, teacher, otherTeacher);
    }

    [Fact]
    public async Task GetSubmissionsForAssignmentAsync_Throws_WhenAnotherTeacherOwnsTheAssignment()
    {
        var (context, assignment, _, _, otherTeacher) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = otherTeacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetSubmissionsForAssignmentAsync(assignment.Id));
    }

    [Fact]
    public async Task GetSubmissionsForAssignmentAsync_Throws_WhenAssignmentDoesNotExist()
    {
        var (context, _, _, teacher, _) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetSubmissionsForAssignmentAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetSubmissionsForAssignmentAsync_ReturnsSubmissions_WhenTeacherOwnsTheAssignment()
    {
        var (context, assignment, _, teacher, _) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        var result = await service.GetSubmissionsForAssignmentAsync(assignment.Id);

        Assert.Single(result);
    }

    [Fact]
    public async Task GradeAsync_Throws_WhenMarksExceedMaxMarks()
    {
        var (context, _, submission, teacher, _) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.GradeAsync(submission.Id, new GradeSubmissionRequest { MarksObtained = 150 }));
    }

    [Fact]
    public async Task GradeAsync_SetsStatusToGraded_OnSuccess()
    {
        var (context, _, submission, teacher, _) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        var result = await service.GradeAsync(submission.Id, new GradeSubmissionRequest { MarksObtained = 85, Feedback = "Good work" });

        Assert.Equal("Graded", result.Status);
        Assert.Equal(85, result.MarksObtained);
        Assert.Equal("Good work", result.Feedback);
    }

    [Fact]
    public async Task GradeAsync_Throws_WhenAnotherTeacherOwnsTheAssignment()
    {
        var (context, _, submission, _, otherTeacher) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = otherTeacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.GradeAsync(submission.Id, new GradeSubmissionRequest { MarksObtained = 50 }));
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_WhenSettingStatusToGradedDirectly()
    {
        var (context, _, submission, teacher, _) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateStatusAsync(submission.Id, new UpdateSubmissionStatusRequest { Status = SubmissionStatus.Graded }));
    }

    [Fact]
    public async Task UpdateStatusAsync_Throws_WhenAnotherTeacherOwnsTheAssignment()
    {
        var (context, _, submission, _, otherTeacher) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = otherTeacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() =>
            service.UpdateStatusAsync(submission.Id, new UpdateSubmissionStatusRequest { Status = SubmissionStatus.Returned }));
    }

    [Fact]
    public async Task UpdateStatusAsync_ClearsMarksAndFeedback_WhenMovingAwayFromGraded()
    {
        var (context, _, submission, teacher, _) = SeedSubmission();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new TeacherSubmissionService(context, currentUser);

        await service.GradeAsync(submission.Id, new GradeSubmissionRequest { MarksObtained = 70, Feedback = "Needs revision" });

        var result = await service.UpdateStatusAsync(submission.Id, new UpdateSubmissionStatusRequest { Status = SubmissionStatus.Returned });

        Assert.Equal("Returned", result.Status);
        Assert.Null(result.MarksObtained);
        Assert.Null(result.Feedback);
    }
}