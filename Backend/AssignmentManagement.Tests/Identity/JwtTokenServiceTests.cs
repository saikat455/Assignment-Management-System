using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Infrastructure.Identity;
using Microsoft.Extensions.Options;

namespace AssignmentManagement.Tests.Identity;

public class JwtTokenServiceTests
{
    private static JwtTokenService CreateService(int expiryMinutes = 30)
    {
        var settings = new JwtSettings
        {
            Key = "unit-test-signing-key-at-least-32-characters-long",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpiryMinutes = expiryMinutes
        };

        return new JwtTokenService(Options.Create(settings));
    }

    [Fact]
    public void GenerateToken_IncludesTheUsersRoleAsAClaim()
    {
        var service = CreateService();
        var user = new User { Id = Guid.NewGuid(), FullName = "Ada Lovelace", Email = "ada@school.test", Role = Role.Teacher };

        var result = service.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        var roleClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Role);
        Assert.Equal("Teacher", roleClaim.Value);
    }

    [Fact]
    public void GenerateToken_IncludesTheUsersIdAsTheNameIdentifierClaim()
    {
        var service = CreateService();
        var user = new User { Id = Guid.NewGuid(), FullName = "Grace Hopper", Email = "grace@school.test", Role = Role.Admin };

        var result = service.GenerateToken(user);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(result.Token);
        var idClaim = jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier);
        Assert.Equal(user.Id.ToString(), idClaim.Value);
    }

    [Fact]
    public void GenerateToken_SetsExpiryAccordingToConfiguredMinutes()
    {
        var service = CreateService(expiryMinutes: 30);
        var user = new User { Id = Guid.NewGuid(), FullName = "Ada Lovelace", Email = "ada@school.test", Role = Role.Student };

        var before = DateTime.UtcNow;
        var result = service.GenerateToken(user);
        var after = DateTime.UtcNow;

        Assert.InRange(result.ExpiresAtUtc, before.AddMinutes(30), after.AddMinutes(30).AddSeconds(5));
    }
}