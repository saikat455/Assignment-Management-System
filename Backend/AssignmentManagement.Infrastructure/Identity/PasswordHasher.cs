using AssignmentManagement.Application.Common.Interfaces;
using BCrypt.Net;

namespace AssignmentManagement.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string plainTextPassword) => BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

    public bool Verify(string plainTextPassword, string passwordHash) =>
        BCrypt.Net.BCrypt.Verify(plainTextPassword, passwordHash);
}