using AssignmentManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Tests.Common;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}