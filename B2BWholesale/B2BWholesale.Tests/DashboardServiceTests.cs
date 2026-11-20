using B2BWholesale.Data;
using B2BWholesale.Data.Models;
using B2BWholesale.Data.Services;

namespace B2BWholesale.Tests;

/// <summary>Task 1.4 - dashboard queries: best-sellers, low stock, revenue by agent.</summary>
public sealed class DashboardServiceTests
{
    [Fact]
    public void BestSellers_RankByUnitsSold_AndIgnoreCancelledOrders()
    {
        using var db = CreateSales();
        var service = new DashboardService(db);

        var rows = service.GetBestSellingItems(null, null, 10);

        Assert.Equal(2, rows.Count);
        Assert.Equal("Rice", rows[0].ItemName);
        Assert.Equal(7, rows[0].UnitsSold);          // 5 + 2
        Assert.Equal(700m, rows[0].Revenue);
        Assert.Equal(1, rows[0].Rank);
        Assert.Equal("Oil", rows[1].ItemName);
        Assert.Equal(1, rows[1].UnitsSold);          // the 50 Oil in the cancelled order are NOT counted
    }

    [Fact]
    public void BestSellers_HonoursTopNAndDateRange()
    {
        using var db = CreateSales();
        var service = new DashboardService(db);

        var topOne = service.GetBestSellingItems(null, null, 1);
        var lateOctober = service.GetBestSellingItems(new DateTime(2026, 10, 5), null, 10);

        Assert.Single(topOne);
        var only = Assert.Single(lateOctober);       // only order 2 (10 Oct) is left
        Assert.Equal("Rice", only.ItemName);
        Assert.Equal(2, only.UnitsSold);
    }

    [Fact]
    public void LowStock_ReturnsOnlyItemsBelowTheThreshold_LowestFirst()
    {
        using var db = CreateSales();
        var service = new DashboardService(db);

        var rows = service.GetLowStockItems(10);

        var low = Assert.Single(rows);
        Assert.Equal("Oil", low.ItemName);
        Assert.Equal(3, low.StockQuantity);
        Assert.Equal(2, service.GetLowStockItems(100).Count);
        Assert.Empty(service.GetLowStockItems(3));   // "below" means strictly less than
    }

    [Fact]
    public void RevenueByAgent_SumsQuantityTimesUnitAmount_ForNonCancelledOrders()
    {
        using var db = CreateSales();
        var service = new DashboardService(db);

        var rows = service.GetRevenueByAgent(null, null, null);

        Assert.Equal(2, rows.Count);
        Assert.Equal("Agent One", rows[0].AgentName);
        Assert.Equal(750m, rows[0].Revenue);         // 5 x 100 + 1 x 250 (cancelled order ignored)
        Assert.Equal(1, rows[0].OrderCount);
        Assert.Equal("Agent Two", rows[1].AgentName);
        Assert.Equal(200m, rows[1].Revenue);
    }

    [Fact]
    public void RevenueByAgent_FiltersOneAgent_AndReturnsAZeroRowWhenThereAreNoSales()
    {
        using var db = CreateSales();
        db.Agents.Add(new Agent { AgentID = 3, AgentName = "Agent Three", Address = "3 Main Street", Phone = "0903333333" });
        db.SaveChanges();
        var service = new DashboardService(db);

        var agentTwo = Assert.Single(service.GetRevenueByAgent(2, null, null));
        var agentThree = Assert.Single(service.GetRevenueByAgent(3, null, null));
        var agentOneNovember = Assert.Single(service.GetRevenueByAgent(1, new DateTime(2026, 11, 1), null));

        Assert.Equal(200m, agentTwo.Revenue);
        Assert.Equal(0m, agentThree.Revenue);
        Assert.Equal(0, agentThree.OrderCount);
        Assert.Equal(0m, agentOneNovember.Revenue);
    }

    [Fact]
    public void Dashboard_RejectsInvalidFilters()
    {
        using var db = CreateSales();
        var service = new DashboardService(db);

        Assert.Throws<ServiceValidationException>(() =>
            service.GetRevenueByAgent(null, new DateTime(2026, 11, 1), new DateTime(2026, 10, 1)));
        Assert.Throws<ServiceValidationException>(() => service.GetBestSellingItems(null, null, 0));
        Assert.Throws<ServiceValidationException>(() => service.GetLowStockItems(-1));
    }

    /// <summary>
    /// Items: Rice (stock 50, 100), Oil (stock 3, 250). Agents One and Two.
    /// Order 1 (One, 1 Oct, Delivered): 5 Rice + 1 Oil.  Order 2 (Two, 10 Oct, Pending): 2 Rice.
    /// Order 3 (One, 20 Oct, Cancelled): 50 Oil - must never be counted.
    /// </summary>
    private static AppDbContext CreateSales()
    {
        var db = TestDb.Create();
        db.Agents.AddRange(
            new Agent { AgentID = 1, AgentName = "Agent One", Address = "1 Main Street", Phone = "0901111111" },
            new Agent { AgentID = 2, AgentName = "Agent Two", Address = "2 Main Street", Phone = "0902222222" });
        db.Items.AddRange(
            new Item { ItemID = 1, ItemName = "Rice", Size = "25 kg", Price = 100m, StockQuantity = 50 },
            new Item { ItemID = 2, ItemName = "Oil", Size = "12 bottles", Price = 250m, StockQuantity = 3 });
        db.Orders.AddRange(
            new Order { OrderID = 1, AgentID = 1, OrderDate = new DateTime(2026, 10, 1), OrderStatus = "Delivered" },
            new Order { OrderID = 2, AgentID = 2, OrderDate = new DateTime(2026, 10, 10), OrderStatus = "Pending" },
            new Order { OrderID = 3, AgentID = 1, OrderDate = new DateTime(2026, 10, 20), OrderStatus = "Cancelled" });
        db.OrderDetails.AddRange(
            new OrderDetail { OrderID = 1, ItemID = 1, Quantity = 5, UnitAmount = 100m },
            new OrderDetail { OrderID = 1, ItemID = 2, Quantity = 1, UnitAmount = 250m },
            new OrderDetail { OrderID = 2, ItemID = 1, Quantity = 2, UnitAmount = 100m },
            new OrderDetail { OrderID = 3, ItemID = 2, Quantity = 50, UnitAmount = 250m });
        db.SaveChanges();
        return db;
    }
}
