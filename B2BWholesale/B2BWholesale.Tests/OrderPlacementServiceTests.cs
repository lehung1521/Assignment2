using B2BWholesale.Data;
using B2BWholesale.Data.Models;
using B2BWholesale.Data.Services;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Tests;

/// <summary>
/// Task 3.2 - master-detail order with stock deduction.
/// The EF Core InMemory provider has no real transactions, so these tests prove the business rules
/// (validate everything first, then write): an order is always all-or-nothing.
/// The SQL transaction + atomic "UPDATE ... WHERE StockQuantity >= qty" run on SQL Server only.
/// </summary>
public sealed class OrderPlacementServiceTests
{
    [Fact]
    public async Task PlaceOrder_CreatesOrderAndDetails_AndDeductsStock()
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        var result = await service.PlaceOrderAsync(1, new[]
        {
            new OrderLineRequest(1, 3),
            new OrderLineRequest(2, 2)
        });

        Assert.Equal(3 * 100m + 2 * 250m, result.Total);
        Assert.Equal(2, result.LineCount);
        Assert.Equal(7, db.Items.Single(i => i.ItemID == 1).StockQuantity);   // 10 - 3
        Assert.Equal(3, db.Items.Single(i => i.ItemID == 2).StockQuantity);   // 5 - 2

        var order = db.Orders.Include(o => o.OrderDetails).Single();
        Assert.Equal("Pending", order.OrderStatus);
        Assert.Equal(1, order.AgentID);
        Assert.Equal(2, order.OrderDetails.Count);
    }

    [Fact]
    public async Task PlaceOrder_BlocksWholeOrder_WhenOneLineHasInsufficientStock()
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        var ex = await Assert.ThrowsAsync<InsufficientStockException>(() => service.PlaceOrderAsync(1, new[]
        {
            new OrderLineRequest(1, 3),     // fine
            new OrderLineRequest(2, 99)     // only 5 in stock
        }));

        var shortage = Assert.Single(ex.Shortages);
        Assert.Equal(2, shortage.ItemId);
        Assert.Equal(99, shortage.Requested);
        Assert.Equal(5, shortage.Available);

        // nothing saved, nothing deducted - not even for the line that was fine
        Assert.Empty(db.Orders);
        Assert.Empty(db.OrderDetails);
        Assert.Equal(10, db.Items.Single(i => i.ItemID == 1).StockQuantity);
        Assert.Equal(5, db.Items.Single(i => i.ItemID == 2).StockQuantity);
    }

    [Fact]
    public async Task PlaceOrder_ReportsEveryShortageAtOnce()
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        var ex = await Assert.ThrowsAsync<InsufficientStockException>(() => service.PlaceOrderAsync(1, new[]
        {
            new OrderLineRequest(1, 11),
            new OrderLineRequest(2, 6)
        }));

        Assert.Equal(2, ex.Shortages.Count);
        Assert.Contains("Rice", ex.Message);
        Assert.Contains("Oil", ex.Message);
    }

    [Fact]
    public async Task PlaceOrder_AllowsQuantityExactlyEqualToStock_AndStockBecomesZero()
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        await service.PlaceOrderAsync(1, new[] { new OrderLineRequest(2, 5) });

        Assert.Equal(0, db.Items.Single(i => i.ItemID == 2).StockQuantity);
        Assert.Single(db.Orders);
    }

    [Fact]
    public async Task PlaceOrder_ChecksDuplicateLinesAgainstTheCombinedQuantity()
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        // 6 + 6 = 12 > 10 in stock: each line alone would pass, together they must not
        await Assert.ThrowsAsync<InsufficientStockException>(() => service.PlaceOrderAsync(1, new[]
        {
            new OrderLineRequest(1, 6),
            new OrderLineRequest(1, 6)
        }));
        Assert.Empty(db.Orders);
        Assert.Equal(10, db.Items.Single(i => i.ItemID == 1).StockQuantity);

        // 4 + 6 = 10: allowed, merged into ONE detail line
        await service.PlaceOrderAsync(1, new[]
        {
            new OrderLineRequest(1, 4),
            new OrderLineRequest(1, 6)
        });
        var detail = Assert.Single(db.OrderDetails);
        Assert.Equal(10, detail.Quantity);
        Assert.Equal(0, db.Items.Single(i => i.ItemID == 1).StockQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task PlaceOrder_RejectsZeroOrNegativeQuantity_AndSavesNothing(int quantity)
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        await Assert.ThrowsAsync<ServiceValidationException>(() =>
            service.PlaceOrderAsync(1, new[] { new OrderLineRequest(1, quantity) }));

        Assert.Empty(db.Orders);
        Assert.Equal(10, db.Items.Single(i => i.ItemID == 1).StockQuantity);
    }

    [Fact]
    public async Task PlaceOrder_RejectsUnknownItemAndUnknownAgent()
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        await Assert.ThrowsAsync<ServiceValidationException>(() =>
            service.PlaceOrderAsync(1, new[] { new OrderLineRequest(999, 1) }));
        await Assert.ThrowsAsync<ServiceValidationException>(() =>
            service.PlaceOrderAsync(404, new[] { new OrderLineRequest(1, 1) }));

        Assert.Empty(db.Orders);
    }

    [Fact]
    public async Task PlaceOrder_StoresThePriceAtOrderTime_SoLaterPriceChangesDoNotAlterTheOrder()
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        await service.PlaceOrderAsync(1, new[] { new OrderLineRequest(1, 2) });

        var item = db.Items.Single(i => i.ItemID == 1);
        item.Price = 999m;
        db.SaveChanges();

        Assert.Equal(100m, db.OrderDetails.Single().UnitAmount);
    }

    [Fact]
    public async Task PlaceOrder_SecondOrderCannotTakeStockAlreadySold()
    {
        using var db = CreateCatalog();
        var service = new OrderPlacementService(db);

        await service.PlaceOrderAsync(1, new[] { new OrderLineRequest(2, 4) });          // 5 -> 1 left

        await Assert.ThrowsAsync<InsufficientStockException>(() =>
            service.PlaceOrderAsync(1, new[] { new OrderLineRequest(2, 2) }));           // only 1 left

        Assert.Equal(1, db.Items.Single(i => i.ItemID == 2).StockQuantity);
        Assert.Single(db.Orders);
    }

    /// <summary>Agent 1, item 1 "Rice" (stock 10, 100) and item 2 "Oil" (stock 5, 250).</summary>
    private static AppDbContext CreateCatalog()
    {
        var db = TestDb.Create();
        db.Agents.Add(new Agent { AgentID = 1, AgentName = "Agent Co.", Address = "1 Main Street", Phone = "0901234567" });
        db.Items.AddRange(
            new Item { ItemID = 1, ItemName = "Rice", Size = "25 kg", Price = 100m, StockQuantity = 10 },
            new Item { ItemID = 2, ItemName = "Oil", Size = "12 bottles", Price = 250m, StockQuantity = 5 });
        db.SaveChanges();
        return db;
    }
}
