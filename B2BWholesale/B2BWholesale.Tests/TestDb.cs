using B2BWholesale.Data;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Tests;

internal static class TestDb
{
    /// <summary>A fresh, empty in-memory database (seed data is NOT loaded).</summary>
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
