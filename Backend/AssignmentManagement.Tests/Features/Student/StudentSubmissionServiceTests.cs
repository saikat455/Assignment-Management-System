using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Features.Student.Submissions;
using AssignmentManagement.Application.Features.Student.Submissions.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Infrastructure.Persistence;
using AssignmentManagement.Tests.Common;

namespace AssignmentManagement.Tests.Features.Student;

public class StudentSubmissionServiceTests
{
    private static (ApplicationDbContext Context, Assignment Assignment, User Student) SeedPublishedAssignmentForStudent(DateTime deadlineUtc)
    {
        var context = TestDbContextFactory.Create();

        var schoolClass = new SchoolClass { Name = "Class 10", Section = "A" };
        var subject = new Subject { Name = "Math", Code = "MATH101", Class = schoolClass };
        var teacher = new User { FullName = "Teacher", Email = "t@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        var student = new User { FullName = "Student", Email = "s@school.test", PasswordHash = "x", Role = Role.Student, Class = schoolClass, IsActive = true };

        context.Classes.Add(schoolClass);
        context.Subjects.Add(subject);
        context.Users.AddRange(teacher, student);
        context.SaveChanges();

        var assignment = new Assignment
        {
            Title = "Worksheet",
            Description = "desc",
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            DeadlineUtc = deadlineUtc,
            MaxMarks = 100,
            Status = AssignmentStatus.Published
        };
        context.Assignments.Add(assignment);
        context.SaveChanges();

        return (context, assignment, student);
    }

    [Fact]
    public async Task SubmitAsync_Throws_WhenAssignmentDoesNotExistForTheStudentsClass()
    {
        var (context, _, student) = SeedPublishedAssignmentForStudent(DateTime.UtcNow.AddDays(1));
        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SubmitAsync(Guid.NewGuid(), new CreateSubmissionRequest { AnswerText = "answer" }));
    }

    [Fact]
    public async Task SubmitAsync_Throws_WhenAssignmentIsStillADraft()
    {
        var context = TestDbContextFactory.Create();
        var schoolClass = new SchoolClass { Name = "Class 10", Section = "A" };
        var subject = new Subject { Name = "Math", Code = "MATH101", Class = schoolClass };
        var teacher = new User { FullName = "Teacher", Email = "t@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        var student = new User { FullName = "Student", Email = "s@school.test", PasswordHash = "x", Role = Role.Student, Class = schoolClass, IsActive = true };
        context.Classes.Add(schoolClass);
        context.Subjects.Add(subject);
        context.Users.AddRange(teacher, student);
        context.SaveChanges();

        var draftAssignment = new Assignment
        {
            Title = "Draft only",
            Description = "desc",
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            Status = AssignmentStatus.Draft
        };
        context.Assignments.Add(draftAssignment);
        context.SaveChanges();

        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.SubmitAsync(draftAssignment.Id, new CreateSubmissionRequest { AnswerText = "answer" }));
    }

    [Fact]
    public async Task SubmitAsync_Succeeds_AsSubmitted_WhenBeforeDeadline()
    {
        var (context, assignment, student) = SeedPublishedAssignmentForStudent(DateTime.UtcNow.AddDays(1));
        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        var result = await service.SubmitAsync(assignment.Id, new CreateSubmissionRequest { AnswerText = "my answer" });

        Assert.Equal("Submitted", result.Status);
    }

    [Fact]
    public async Task SubmitAsync_MarksAsLate_WhenAfterDeadline()
    {
        var (context, assignment, student) = SeedPublishedAssignmentForStudent(DateTime.UtcNow.AddMinutes(-5));
        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        var result = await service.SubmitAsync(assignment.Id, new CreateSubmissionRequest { AnswerText = "my late answer" });

        Assert.Equal("Late", result.Status);
    }

    [Fact]
    public async Task SubmitAsync_Throws_WhenAlreadySubmitted()
    {
        var (context, assignment, student) = SeedPublishedAssignmentForStudent(DateTime.UtcNow.AddDays(1));
        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        await service.SubmitAsync(assignment.Id, new CreateSubmissionRequest { AnswerText = "first" });

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.SubmitAsync(assignment.Id, new CreateSubmissionRequest { AnswerText = "second" }));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenDeadlineHasPassedAndStatusIsNotReturned()
    {
        var (context, assignment, student) = SeedPublishedAssignmentForStudent(DateTime.UtcNow.AddMinutes(-10));
        context.Submissions.Add(new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = student.Id,
            AnswerText = "on time answer",
            SubmittedAtUtc = DateTime.UtcNow.AddMinutes(-20),
            Status = SubmissionStatus.Submitted
        });
        context.SaveChanges();

        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<BadRequestException>(() =>
            service.UpdateAsync(assignment.Id, new UpdateSubmissionRequest { AnswerText = "too late" }));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenSubmissionIsAlreadyGraded()
    {
        var (context, assignment, student) = SeedPublishedAssignmentForStudent(DateTime.UtcNow.AddDays(1));
        context.Submissions.Add(new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = student.Id,
            AnswerText = "answer",
            SubmittedAtUtc = DateTime.UtcNow,
            Status = SubmissionStatus.Graded,
            MarksObtained = 90
        });
        context.SaveChanges();

        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.UpdateAsync(assignment.Id, new UpdateSubmissionRequest { AnswerText = "trying to sneak an edit" }));
    }

    [Fact]
    public async Task UpdateAsync_AllowsEdit_AndResetsToSubmitted_WhenTeacherReturnedIt_EvenPastDeadline()
    {
        var (context, assignment, student) = SeedPublishedAssignmentForStudent(DateTime.UtcNow.AddMinutes(-5));
        context.Submissions.Add(new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = student.Id,
            AnswerText = "first try",
            SubmittedAtUtc = DateTime.UtcNow.AddMinutes(-10),
            Status = SubmissionStatus.Returned
        });
        context.SaveChanges();

        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        var result = await service.UpdateAsync(assignment.Id, new UpdateSubmissionRequest { AnswerText = "fixed answer" });

        Assert.Equal("Submitted", result.Status);
        Assert.Equal("fixed answer", result.AnswerText);
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WhenStillBeforeDeadline()
    {
        var (context, assignment, student) = SeedPublishedAssignmentForStudent(DateTime.UtcNow.AddDays(1));
        context.Submissions.Add(new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = student.Id,
            AnswerText = "first draft",
            SubmittedAtUtc = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        });
        context.SaveChanges();

        var currentUser = new FakeCurrentUserService { UserId = student.Id };
        var service = new StudentSubmissionService(context, currentUser);

        var result = await service.UpdateAsync(assignment.Id, new UpdateSubmissionRequest { AnswerText = "revised draft" });

        Assert.Equal("revised draft", result.AnswerText);
        Assert.Equal("Submitted", result.Status);
    }
}