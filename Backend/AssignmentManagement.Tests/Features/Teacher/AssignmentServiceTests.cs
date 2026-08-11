using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Features.Teacher.Assignments;
using AssignmentManagement.Application.Features.Teacher.Assignments.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Infrastructure.Persistence;
using AssignmentManagement.Tests.Common;

namespace AssignmentManagement.Tests.Features.Teacher;

public class AssignmentServiceTests
{
    private static (ApplicationDbContext Context, SchoolClass Class, Subject Subject, User Teacher, User OtherTeacher) SeedClassSubjectAndTeacher()
    {
        var context = TestDbContextFactory.Create();

        var schoolClass = new SchoolClass { Name = "Class 10", Section = "A" };
        var subject = new Subject { Name = "Math", Code = "MATH101", Class = schoolClass };
        var teacher = new User { FullName = "Teacher One", Email = "t1@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        var otherTeacher = new User { FullName = "Teacher Two", Email = "t2@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };

        context.Classes.Add(schoolClass);
        context.Subjects.Add(subject);
        context.Users.AddRange(teacher, otherTeacher);
        context.SaveChanges();

        context.TeacherAssignments.Add(new TeacherSubjectAssignment { TeacherId = teacher.Id, SubjectId = subject.Id });
        context.SaveChanges();

        return (context, schoolClass, subject, teacher, otherTeacher);
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenDeadlineIsInThePast()
    {
        var (context, _, subject, teacher, _) = SeedClassSubjectAndTeacher();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new AssignmentService(context, currentUser);

        var request = new CreateAssignmentRequest
        {
            Title = "Late",
            Description = "desc",
            SubjectId = subject.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(-1),
            MaxMarks = 100
        };

        await Assert.ThrowsAsync<BadRequestException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenTeacherIsNotAssignedToTheSubject()
    {
        var (context, _, subject, _, otherTeacher) = SeedClassSubjectAndTeacher();
        var currentUser = new FakeCurrentUserService { UserId = otherTeacher.Id };
        var service = new AssignmentService(context, currentUser);

        var request = new CreateAssignmentRequest
        {
            Title = "Not mine",
            Description = "desc",
            SubjectId = subject.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100
        };

        await Assert.ThrowsAsync<ForbiddenException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_Throws_WhenSubjectDoesNotExist()
    {
        var (context, _, _, teacher, _) = SeedClassSubjectAndTeacher();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new AssignmentService(context, currentUser);

        var request = new CreateAssignmentRequest
        {
            Title = "Ghost subject",
            Description = "desc",
            SubjectId = Guid.NewGuid(),
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100
        };

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task CreateAsync_DefaultsToDraft_WhenPublishImmediatelyIsFalse()
    {
        var (context, _, subject, teacher, _) = SeedClassSubjectAndTeacher();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new AssignmentService(context, currentUser);

        var request = new CreateAssignmentRequest
        {
            Title = "Worksheet",
            Description = "desc",
            SubjectId = subject.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            PublishImmediately = false
        };

        var result = await service.CreateAsync(request);

        Assert.Equal("Draft", result.Status);
    }

    [Fact]
    public async Task CreateAsync_PublishesImmediately_WhenRequested()
    {
        var (context, _, subject, teacher, _) = SeedClassSubjectAndTeacher();
        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new AssignmentService(context, currentUser);

        var request = new CreateAssignmentRequest
        {
            Title = "Worksheet",
            Description = "desc",
            SubjectId = subject.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            PublishImmediately = true
        };

        var result = await service.CreateAsync(request);

        Assert.Equal("Published", result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_Throws_WhenAnotherTeacherOwnsTheAssignment()
    {
        var (context, _, subject, teacher, otherTeacher) = SeedClassSubjectAndTeacher();

        var assignment = new Assignment
        {
            Title = "Mine",
            Description = "desc",
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100
        };
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = otherTeacher.Id };
        var service = new AssignmentService(context, currentUser);

        await Assert.ThrowsAsync<ForbiddenException>(() => service.GetByIdAsync(assignment.Id));
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenSubmissionsAlreadyExist()
    {
        var (context, schoolClass, subject, teacher, _) = SeedClassSubjectAndTeacher();

        var student = new User { FullName = "Student", Email = "s1@school.test", PasswordHash = "x", Role = Role.Student, Class = schoolClass, IsActive = true };
        context.Users.Add(student);

        var assignment = new Assignment
        {
            Title = "Mine",
            Description = "desc",
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            Status = AssignmentStatus.Published
        };
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();

        context.Submissions.Add(new Submission
        {
            AssignmentId = assignment.Id,
            StudentId = student.Id,
            AnswerText = "answer",
            SubmittedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new AssignmentService(context, currentUser);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(assignment.Id));
    }

    [Fact]
    public async Task DeleteAsync_Succeeds_WhenNoSubmissionsExist()
    {
        var (context, _, subject, teacher, _) = SeedClassSubjectAndTeacher();

        var assignment = new Assignment
        {
            Title = "Mine",
            Description = "desc",
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100
        };
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new AssignmentService(context, currentUser);

        await service.DeleteAsync(assignment.Id);

        Assert.Empty(context.Assignments);
    }

    [Fact]
    public async Task PublishAsync_Throws_WhenAlreadyPublished()
    {
        var (context, _, subject, teacher, _) = SeedClassSubjectAndTeacher();

        var assignment = new Assignment
        {
            Title = "Mine",
            Description = "desc",
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            Status = AssignmentStatus.Published
        };
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new AssignmentService(context, currentUser);

        await Assert.ThrowsAsync<ConflictException>(() => service.PublishAsync(assignment.Id));
    }

    [Fact]
    public async Task UnpublishAsync_Throws_WhenAlreadyDraft()
    {
        var (context, _, subject, teacher, _) = SeedClassSubjectAndTeacher();

        var assignment = new Assignment
        {
            Title = "Mine",
            Description = "desc",
            SubjectId = subject.Id,
            TeacherId = teacher.Id,
            DeadlineUtc = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            Status = AssignmentStatus.Draft
        };
        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = teacher.Id };
        var service = new AssignmentService(context, currentUser);

        await Assert.ThrowsAsync<ConflictException>(() => service.UnpublishAsync(assignment.Id));
    }
}