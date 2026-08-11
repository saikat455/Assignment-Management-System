using AssignmentManagement.Infrastructure.Identity;

namespace AssignmentManagement.Tests.Identity;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesADifferentStringThanThePlainTextPassword()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.NotEqual("Secret123!", hash);
    }

    [Fact]
    public void Verify_ReturnsTrue_ForTheCorrectPassword()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.True(_hasher.Verify("Secret123!", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForTheWrongPassword()
    {
        var hash = _hasher.Hash("Secret123!");

        Assert.False(_hasher.Verify("WrongPassword!", hash));
    }

    [Fact]
    public void Hash_ProducesADifferentHashEachTime_DueToSalting()
    {
        var hash1 = _hasher.Hash("Secret123!");
        var hash2 = _hasher.Hash("Secret123!");

        Assert.NotEqual(hash1, hash2);
    }
}