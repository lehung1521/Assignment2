using B2BWholesale.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data.Services;

/// <summary>
/// Task 1.4 - queries for the dashboard: best-selling items, low-stock warning and revenue by agent.
/// Cancelled orders are never counted as sales.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    public const string CancelledStatus = "Cancelled";

    private readonly AppDbContext _dbContext;

    public DashboardService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<BestSellerRow> GetBestSellingItems(DateTime? from, DateTime? to, int top)
    {
        ValidateRange(from, to);
        if (top < 1 || top > 100)
        {
            throw new ServiceValidationException("Top N must be between 1 and 100.");
        }

        var rows = SalesLines(from, to)
            .GroupBy(d => new { d.ItemID, ItemName = d.Item!.ItemName, Size = d.Item!.Size })
            .Select(g => new
            {
                g.Key.ItemID,
                g.Key.ItemName,
                g.Key.Size,
                Units = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.Quantity * x.UnitAmount)
            })
            .OrderByDescending(x => x.Units)
            .ThenBy(x => x.ItemName)
            .Take(top)
            .ToList();

        return rows
            .Select((r, index) => new BestSellerRow(index + 1, r.ItemID, r.ItemName, r.Size, r.Units, r.Revenue))
            .ToList();
    }

    public IReadOnlyList<LowStockRow> GetLowStockItems(int threshold)
    {
        if (threshold < 0 || threshold > 1_000_000)
        {
            throw new ServiceValidationException("The low-stock threshold must be a non-negative number.");
        }

        return _dbContext.Items
            .AsNoTracking()
            .Where(i => i.StockQuantity < threshold)
            .OrderBy(i => i.StockQuantity)
            .ThenBy(i => i.ItemName)
            .Select(i => new LowStockRow(i.ItemID, i.ItemName, i.Size, i.StockQuantity, threshold))
            .ToList();
    }

    public IReadOnlyList<AgentRevenueRow> GetRevenueByAgent(int? agentId, DateTime? from, DateTime? to)
    {
        ValidateRange(from, to);

        var lines = SalesLines(from, to);
        if (agentId.HasValue)
        {
            var id = agentId.Value;
            lines = lines.Where(d => d.Order!.AgentID == id);
        }

        var rows = lines
            .GroupBy(d => new { AgentId = d.Order!.AgentID, AgentName = d.Order!.Agent!.AgentName })
            .Select(g => new
            {
                g.Key.AgentId,
                g.Key.AgentName,
                Orders = g.Select(x => x.OrderID).Distinct().Count(),
                Revenue = g.Sum(x => x.Quantity * x.UnitAmount)
            })
            .OrderByDescending(x => x.Revenue)
            .ThenBy(x => x.AgentName)
            .ToList();

        var result = rows
            .Select(r => new AgentRevenueRow(r.AgentId, r.AgentName, r.Orders, r.Revenue))
            .ToList();

        // A chosen agent with no sales in the period still gets a row (0 orders, 0 revenue).
        if (agentId.HasValue && result.Count == 0)
        {
            var agent = _dbContext.Agents.AsNoTracking().FirstOrDefault(a => a.AgentID == agentId.Value)
                ?? throw new KeyNotFoundException($"Agent {agentId.Value} was not found.");
            result.Add(new AgentRevenueRow(agent.AgentID, agent.AgentName, 0, 0m));
        }

        return result;
    }

    /// <summary>Order lines of non-cancelled orders inside the (inclusive) date range.</summary>
    private IQueryable<OrderDetail> SalesLines(DateTime? from, DateTime? to)
    {
        var query = _dbContext.OrderDetails
            .AsNoTracking()
            .Where(d => d.Order!.OrderStatus != CancelledStatus);

        if (from.HasValue)
        {
            var start = from.Value.Date;
            query = query.Where(d => d.Order!.OrderDate >= start);
        }

        if (to.HasValue)
        {
            var endExclusive = to.Value.Date.AddDays(1);
            query = query.Where(d => d.Order!.OrderDate < endExclusive);
        }

        return query;
    }

    private static void ValidateRange(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
        {
            throw new ServiceValidationException("The 'From' date must not be after the 'To' date.");
        }
    }
}
