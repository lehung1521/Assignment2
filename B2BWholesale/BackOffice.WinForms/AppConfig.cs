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

    /// <summary>Dashboard low-stock threshold, read from the appSettings section of App.config.</summary>
    public static int LowStockThreshold
    {
        get
        {
            var raw = ConfigurationManager.AppSettings["LowStockThreshold"];
            return int.TryParse(raw, out var value) && value >= 0 ? value : 20;
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