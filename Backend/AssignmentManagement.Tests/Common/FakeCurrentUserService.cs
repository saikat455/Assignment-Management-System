using AssignmentManagement.Application.Common.Interfaces;

namespace AssignmentManagement.Tests.Common;

/// A settable stand-in for ICurrentUserService, used to simulate "who is logged in" in tests.
public class FakeCurrentUserService : ICurrentUserService
{
    public Guid? UserId { get; set; }

    public string? Role { get; set; }
}