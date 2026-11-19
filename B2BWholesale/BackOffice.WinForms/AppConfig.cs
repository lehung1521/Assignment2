using System.Configuration;
using B2BWholesale.Data;
using Microsoft.EntityFrameworkCore;

namespace BackOffice.WinForms;

internal static class AppConfig
{
    private const string ConnectionName = "DefaultConnection";

    public static string ConnectionString
    {
        get
        {
            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionName]?.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Connection string '{ConnectionName}' was not found in App.config. " +
                    "Check the <connectionStrings> section of BackOffice.WinForms\\App.config.");
            }

            return connectionString;
        }
    }

    public static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }
}