using B2BWholesale.Data;
using B2BWholesale.Data.Models;
using B2BWholesale.Data.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace B2BWholesale.Tests;

public sealed class BackOfficeServiceTests
{
    [Fact]
    public void InventoryService_RejectsNegativeStock()
    {
        using var db = CreateDbContext();
        var service = new InventoryService(db);

        var exception = Assert.Throws<ServiceValidationException>(() =>
            service.CreateItem(new ItemInput("Rice", "25 kg", "500000", "-1")));

        Assert.Contains("non-negative", exception.Message);
    }

    [Fact]
    public void InventoryService_RejectsDuplicateNameAndSize()
    {
        using var db = CreateDbContext();
        db.Items.Add(new Item { ItemName = "Rice", Size = "25 kg", Price = 500000m, StockQuantity = 10 });
        db.SaveChanges();
        var service = new InventoryService(db);

        Assert.Throws<ServiceValidationException>(() =>
            service.CreateItem(new ItemInput(" Rice ", "25 kg", "500000", "2")));
    }

    [Fact]
    public void InventoryService_CreatesUpdatesAndListsItems()
    {
        using var db = CreateDbContext();
        var service = new InventoryService(db);

        var created = service.CreateItem(new ItemInput(" Rice ", "25 kg", "500000", "10"));
        var updated = service.UpdateItem(created.ItemId, new ItemInput("Rice", "25 kg", "510000", "0"));

        Assert.Equal("Rice", updated.ItemName);
        Assert.Equal(510000m, updated.Price);
        Assert.Equal(0, Assert.Single(service.GetItems()).StockQuantity);
    }

    [Fact]
    public void AgentService_RejectsPhoneOutsideSqlConstraint()
    {
        using var db = CreateDbContext();
        var service = new AgentManagementService(db);

        var exception = Assert.Throws<ServiceValidationException>(() =>
            service.CreateAgent(new AgentInput("Agent", "Address", "123-456")));

        Assert.Contains("9 to 11 digits", exception.Message);
    }

    [Fact]
    public void AuthenticationService_LocksAccountOnThirdFailedAttempt()
    {
        using var db = CreateDbContext();
        var user = new User
        {
            UserName = "admin",
            Email = "admin@example.test",
            Password = PasswordHashing.ComputeSha256("Correct1!"),
            Role = "Admin",
            FailedLoginCount = 0
        };
        db.Users.Add(user);
        db.SaveChanges();
        var service = new AdminAuthenticationService(db);

        Assert.Equal(AdminLoginStatus.InvalidCredentials, service.AuthenticateAdmin("admin", "wrong").Status);
        Assert.Equal(AdminLoginStatus.InvalidCredentials, service.AuthenticateAdmin("admin", "wrong").Status);
        Assert.Equal(AdminLoginStatus.AccountLocked, service.AuthenticateAdmin("admin", "wrong").Status);
        Assert.Equal(AdminLoginStatus.AccountLocked, service.AuthenticateAdmin("admin", "Correct1!").Status);
        Assert.True(user.Lock);
        Assert.Equal(3, user.FailedLoginCount);
    }

    [Fact]
    public void AuthenticationService_DeniesAgentAndAcceptsValidAdmin()
    {
        using var db = CreateDbContext();
        db.Users.AddRange(
            new User
            {
                UserName = "admin",
                Email = "admin@example.test",
                Password = PasswordHashing.ComputeSha256("Correct1!"),
                Role = "Admin"
            },
            new User
            {
                UserName = "agent",
                Email = "agent@example.test",
                Password = PasswordHashing.ComputeSha256("Correct1!"),
                Role = "Agent",
                AgentID = 1
            });
        db.SaveChanges();
        var service = new AdminAuthenticationService(db);

        Assert.Equal(AdminLoginStatus.Success, service.AuthenticateAdmin("admin@example.test", "Correct1!").Status);
        Assert.Equal(AdminLoginStatus.AgentAccessDenied, service.AuthenticateAdmin("agent", "Correct1!").Status);
        Assert.Equal(AdminLoginStatus.MissingCredentials, service.AuthenticateAdmin(" ", "").Status);
    }

    [Fact]
    public void OrderService_RejectsStatusNotAllowedBySqlSchema()
    {
        using var db = CreateDbContext();
        var service = new OrderManagementService(db);

        Assert.Throws<ServiceValidationException>(() => service.UpdateOrderStatus(10, "Lost"));
    }

    [Fact]
    public void OrderService_ListsCalculatedTotalsAndUpdatesValidStatus()
    {
        using var db = CreateDbContext();
        var agent = new Agent { AgentID = 1, AgentName = "Agent Co.", Address = "123 Main Street", Phone = "0901234567" };
        var item = new Item { ItemID = 1, ItemName = "Rice", Size = "25 kg", Price = 100m, StockQuantity = 4 };
        var order = new Order { OrderID = 1, AgentID = 1, Agent = agent, OrderStatus = "Pending" };
        db.AddRange(agent, item, order);
        db.OrderDetails.Add(new OrderDetail { OrderID = 1, ItemID = 1, Quantity = 3, UnitAmount = 100m });
        db.SaveChanges();
        var service = new OrderManagementService(db);

        service.UpdateOrderStatus(1, "Shipped");
        var summary = Assert.Single(service.GetOrders());

        Assert.Equal("Shipped", summary.OrderStatus);
        Assert.Equal(300m, summary.Total);
    }

    [Fact]
    public void ShippingManifestService_ReturnsLineAndGrandTotals()
    {
        using var db = CreateDbContext();
        var agent = new Agent
        {
            AgentID = 1,
            AgentName = "Agent Co.",
            Address = "123 Main Street",
            Phone = "0901234567"
        };
        var item = new Item
        {
            ItemID = 1,
            ItemName = "Rice",
            Size = "25 kg",
            Price = 400000m,
            StockQuantity = 10
        };
        var secondItem = new Item
        {
            ItemID = 2,
            ItemName = "Oil",
            Size = "12 bottles",
            Price = 350000m,
            StockQuantity = 10
        };
        var order = new Order
        {
            OrderID = 1,
            OrderDate = new DateTime(2026, 1, 2),
            AgentID = agent.AgentID,
            Agent = agent,
            OrderStatus = "Pending"
        };
        db.AddRange(agent, item, secondItem, order);
        db.OrderDetails.AddRange(
            new OrderDetail { OrderID = 1, ItemID = 1, Quantity = 2, UnitAmount = 400000m },
            new OrderDetail { OrderID = 1, ItemID = 2, Quantity = 3, UnitAmount = 350000m });
        db.SaveChanges();
        var service = new ShippingManifestService(db);

        var manifest = service.GetManifest(1);

        Assert.Equal("Agent Co.", manifest.AgentName);
        Assert.Equal(2, manifest.Lines.Count);
        Assert.Equal(800000m, manifest.Lines[0].LineTotal);
        Assert.Equal(1850000m, manifest.GrandTotal);
    }

    [Fact]
    public void ShippingManifestPdfGenerator_ProducesPdfBytes()
    {
        var generator = new ShippingManifestPdfGenerator();
        var data = new ShippingManifestData(
            15,
            new DateTime(2026, 1, 2),
            "Pending",
            "Agent Co.",
            "123 Main Street",
            "0901234567",
            [new ShippingManifestLine("Rice", "25 kg", 2, 400000m, 800000m)],
            800000m);

        var bytes = generator.GeneratePdf(data);

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(bytes, 0, 5));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
