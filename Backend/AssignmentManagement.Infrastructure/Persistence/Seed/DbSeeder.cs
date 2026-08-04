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

        await context.SaveChangesAsync();
    }
}