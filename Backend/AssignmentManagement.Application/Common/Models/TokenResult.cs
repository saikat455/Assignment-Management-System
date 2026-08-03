namespace AssignmentManagement.Application.Common.Models;

public class TokenResult
{
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }
}