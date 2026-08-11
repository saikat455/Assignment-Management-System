using AssignmentManagement.Application.Common.Exceptions;
using AssignmentManagement.Application.Common.Interfaces;
using AssignmentManagement.Application.Common.Models;
using AssignmentManagement.Application.Features.Auth;
using AssignmentManagement.Application.Features.Auth.DTOs;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Tests.Common;
using Moq;

namespace AssignmentManagement.Tests.Features.Auth;

public class AuthServiceTests
{
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenService> _jwtTokenService = new();

    private AuthService CreateService(AssignmentManagement.Infrastructure.Persistence.ApplicationDbContext context) =>
        new(context, _passwordHasher.Object, _jwtTokenService.Object);

    [Fact]
    public async Task RegisterAsync_Throws_WhenEmailAlreadyExists()
    {
        using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Email = "taken@school.test", FullName = "Existing", PasswordHash = "x", Role = Role.Teacher });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var request = new RegisterRequest { FullName = "New Teacher", Email = "taken@school.test", Password = "Password1", Role = Role.Teacher };

        await Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenStudentHasNoClassId()
    {
        using var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var request = new RegisterRequest { FullName = "New Student", Email = "student@school.test", Password = "Password1", Role = Role.Student, ClassId = null };

        await Assert.ThrowsAsync<BadRequestException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_Throws_WhenStudentClassDoesNotExist()
    {
        using var context = TestDbContextFactory.Create();
        var service = CreateService(context);
        var request = new RegisterRequest { FullName = "New Student", Email = "student@school.test", Password = "Password1", Role = Role.Student, ClassId = Guid.NewGuid() };

        await Assert.ThrowsAsync<NotFoundException>(() => service.RegisterAsync(request));
    }

    [Fact]
    public async Task RegisterAsync_CreatesUser_WhenRequestIsValid()
    {
        using var context = TestDbContextFactory.Create();
        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");

        var service = CreateService(context);
        var request = new RegisterRequest { FullName = "New Teacher", Email = "teacher2@school.test", Password = "Password1", Role = Role.Teacher };

        var result = await service.RegisterAsync(request);

        Assert.Equal("teacher2@school.test", result.Email);
        Assert.Equal("Teacher", result.Role);
        Assert.Single(context.Users);
    }

    [Fact]
    public async Task RegisterAsync_AssignsTheClass_WhenRegisteringAValidStudent()
    {
        using var context = TestDbContextFactory.Create();
        var schoolClass = new Domain.Entities.SchoolClass { Name = "Class 10", Section = "A" };
        context.Classes.Add(schoolClass);
        await context.SaveChangesAsync();

        _passwordHasher.Setup(p => p.Hash(It.IsAny<string>())).Returns("hashed-password");
        var service = CreateService(context);
        var request = new RegisterRequest
        {
            FullName = "New Student",
            Email = "student2@school.test",
            Password = "Password1",
            Role = Role.Student,
            ClassId = schoolClass.Id
        };

        var result = await service.RegisterAsync(request);

        Assert.Equal(schoolClass.Id, result.ClassId);
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenUserDoesNotExist()
    {
        using var context = TestDbContextFactory.Create();
        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest { Email = "nobody@school.test", Password = "whatever" }));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenAccountIsDeactivated()
    {
        using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Email = "inactive@school.test", FullName = "Inactive", PasswordHash = "hash", Role = Role.Teacher, IsActive = false });
        await context.SaveChangesAsync();

        _passwordHasher.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest { Email = "inactive@school.test", Password = "whatever" }));
    }

    [Fact]
    public async Task LoginAsync_Throws_WhenPasswordIsWrong()
    {
        using var context = TestDbContextFactory.Create();
        context.Users.Add(new User { Email = "teacher3@school.test", FullName = "T", PasswordHash = "hash", Role = Role.Teacher, IsActive = true });
        await context.SaveChangesAsync();

        _passwordHasher.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(false);
        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            service.LoginAsync(new LoginRequest { Email = "teacher3@school.test", Password = "wrong" }));
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        using var context = TestDbContextFactory.Create();
        var user = new User { Email = "teacher4@school.test", FullName = "T4", PasswordHash = "hash", Role = Role.Teacher, IsActive = true };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        _passwordHasher.Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _jwtTokenService.Setup(j => j.GenerateToken(It.IsAny<User>()))
            .Returns(new TokenResult { Token = "fake-jwt", ExpiresAtUtc = DateTime.UtcNow.AddHours(1) });

        var service = CreateService(context);
        var result = await service.LoginAsync(new LoginRequest { Email = "teacher4@school.test", Password = "correct" });

        Assert.Equal("fake-jwt", result.Token);
        Assert.Equal("Teacher", result.Role);
    }
}