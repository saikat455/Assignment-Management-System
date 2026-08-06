using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AssignmentManagement.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var demoClass = new SchoolClass { Name = "Class 10", Section = "A" };
        context.Classes.Add(demoClass);

        var admin = new User
        {
            FullName = "System Administrator",
            Email = "admin@school.test",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            Role = Role.Admin,
            IsActive = true
        };

        var teacher = new User
        {
            FullName = "Demo Teacher",
            Email = "teacher@school.test",
            PasswordHash = passwordHasher.Hash("Teacher@123"),
            Role = Role.Teacher,
            IsActive = true
        };

        var student = new User
        {
            FullName = "Demo Student",
            Email = "student@school.test",
            PasswordHash = passwordHasher.Hash("Student@123"),
            Role = Role.Student,
            IsActive = true,
            Class = demoClass
        };

        context.Users.AddRange(admin, teacher, student);

        var demoSubject = new Subject
        {
            Name = "Mathematics",
            Code = "MATH101",
            Class = demoClass
        };
        context.Subjects.Add(demoSubject);

        await context.SaveChangesAsync();

        context.TeacherAssignments.Add(new TeacherSubjectAssignment
        {
            TeacherId = teacher.Id,
            SubjectId = demoSubject.Id
        });

        var demoAssignments = new[]
        {
            new Assignment
            {
                Title = "Algebra Basics - Worksheet 1",
                Description = "Complete questions 1-20 on linear equations.",
                DeadlineUtc = DateTime.UtcNow.AddDays(7),
                MaxMarks = 100,
                Status = AssignmentStatus.Published,
                SubjectId = demoSubject.Id,
                TeacherId = teacher.Id
            },
            new Assignment
            {
                Title = "Geometry - Draft Homework",
                Description = "Not yet ready to publish - triangle congruence problems.",
                DeadlineUtc = DateTime.UtcNow.AddDays(14),
                MaxMarks = 50,
                Status = AssignmentStatus.Draft,
                SubjectId = demoSubject.Id,
                TeacherId = teacher.Id
            }
        };

        context.Assignments.AddRange(demoAssignments);
        await context.SaveChangesAsync();

        context.Submissions.Add(new Submission
        {
            AssignmentId = demoAssignments[0].Id,
            StudentId = student.Id,
            AnswerText = "1) x = 5, 2) x = -2, ... (demo answer)",
            SubmittedAtUtc = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        });

        await context.SaveChangesAsync();
    }
}