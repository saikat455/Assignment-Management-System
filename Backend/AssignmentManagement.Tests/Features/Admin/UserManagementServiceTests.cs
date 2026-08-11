using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Features.Admin.Users;
using AssignmentManagement.Application.Features.Admin.Users.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Tests.Common;

namespace AssignmentManagement.Tests.Features.Admin;

public class UserManagementServiceTests
{
    [Fact]
    public async Task DeactivateAsync_Throws_WhenAdminTriesToDeactivateThemselves()
    {
        using var context = TestDbContextFactory.Create();
        var admin = new User { FullName = "Admin", Email = "a@school.test", PasswordHash = "x", Role = Role.Admin, IsActive = true };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = admin.Id };
        var service = new UserManagementService(context, currentUser);

        await Assert.ThrowsAsync<BadRequestException>(() => service.DeactivateAsync(admin.Id));
    }

    [Fact]
    public async Task DeactivateAsync_SetsIsActiveFalse_ForAnotherUser()
    {
        using var context = TestDbContextFactory.Create();
        var admin = new User { FullName = "Admin", Email = "a@school.test", PasswordHash = "x", Role = Role.Admin, IsActive = true };
        var teacher = new User { FullName = "Teacher", Email = "t@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        context.Users.AddRange(admin, teacher);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = admin.Id };
        var service = new UserManagementService(context, currentUser);

        await service.DeactivateAsync(teacher.Id);

        var updated = await context.Users.FindAsync(teacher.Id);
        Assert.False(updated!.IsActive);
    }

    [Fact]
    public async Task DeactivateAsync_Throws_WhenUserDoesNotExist()
    {
        using var context = TestDbContextFactory.Create();
        var admin = new User { FullName = "Admin", Email = "a@school.test", PasswordHash = "x", Role = Role.Admin, IsActive = true };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = admin.Id };
        var service = new UserManagementService(context, currentUser);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeactivateAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenAdminTriesToDeactivateThemselvesViaUpdate()
    {
        using var context = TestDbContextFactory.Create();
        var admin = new User { FullName = "Admin", Email = "a@school.test", PasswordHash = "x", Role = Role.Admin, IsActive = true };
        context.Users.Add(admin);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = admin.Id };
        var service = new UserManagementService(context, currentUser);

        var request = new UpdateUserRequest { FullName = "Admin", Email = "a@school.test", Role = Role.Admin, IsActive = false };

        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAsync(admin.Id, request));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenEmailIsTakenByAnotherUser()
    {
        using var context = TestDbContextFactory.Create();
        var admin = new User { FullName = "Admin", Email = "a@school.test", PasswordHash = "x", Role = Role.Admin, IsActive = true };
        var teacher1 = new User { FullName = "Teacher1", Email = "t1@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        var teacher2 = new User { FullName = "Teacher2", Email = "t2@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        context.Users.AddRange(admin, teacher1, teacher2);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = admin.Id };
        var service = new UserManagementService(context, currentUser);

        var request = new UpdateUserRequest { FullName = "Teacher2", Email = "t1@school.test", Role = Role.Teacher, IsActive = true };

        await Assert.ThrowsAsync<ConflictException>(() => service.UpdateAsync(teacher2.Id, request));
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenChangingRoleToStudentWithoutClassId()
    {
        using var context = TestDbContextFactory.Create();
        var admin = new User { FullName = "Admin", Email = "a@school.test", PasswordHash = "x", Role = Role.Admin, IsActive = true };
        var teacher = new User { FullName = "Teacher", Email = "t@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        context.Users.AddRange(admin, teacher);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = admin.Id };
        var service = new UserManagementService(context, currentUser);

        var request = new UpdateUserRequest { FullName = "Teacher", Email = "t@school.test", Role = Role.Student, IsActive = true, ClassId = null };

        await Assert.ThrowsAsync<BadRequestException>(() => service.UpdateAsync(teacher.Id, request));
    }

    [Fact]
    public async Task UpdateAsync_Succeeds_WhenChangingRoleToStudentWithAValidClassId()
    {
        using var context = TestDbContextFactory.Create();
        var admin = new User { FullName = "Admin", Email = "a@school.test", PasswordHash = "x", Role = Role.Admin, IsActive = true };
        var teacher = new User { FullName = "Teacher", Email = "t@school.test", PasswordHash = "x", Role = Role.Teacher, IsActive = true };
        var schoolClass = new SchoolClass { Name = "Class 10", Section = "A" };
        context.Users.AddRange(admin, teacher);
        context.Classes.Add(schoolClass);
        await context.SaveChangesAsync();

        var currentUser = new FakeCurrentUserService { UserId = admin.Id };
        var service = new UserManagementService(context, currentUser);

        var request = new UpdateUserRequest { FullName = "Teacher", Email = "t@school.test", Role = Role.Student, IsActive = true, ClassId = schoolClass.Id };

        var result = await service.UpdateAsync(teacher.Id, request);

        Assert.Equal(schoolClass.Id, result.ClassId);
    }
}