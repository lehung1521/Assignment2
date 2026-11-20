using B2BWholesale.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data.Services;

/// <summary>
/// Task 3.2 - the core business rule of the system.
/// In ONE database transaction it (1) creates the Order, (2) inserts every OrderDetail and
/// (3) deducts the ordered quantity from Item.StockQuantity.
/// If any item has insufficient stock the transaction is rolled back and nothing is saved.
/// </summary>
public sealed class OrderPlacementService : IOrderPlacementService
{
    public const string PendingStatus = "Pending";

    private readonly AppDbContext _dbContext;

    public OrderPlacementService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PlaceOrderResult> PlaceOrderAsync(
        int agentId, IEnumerable<OrderLineRequest> lines, CancellationToken cancellationToken = default)
    {
        if (agentId <= 0)
        {
            throw new ServiceValidationException("A valid agent account is required to place an order.");
        }

        // Validation + merging duplicate items (stock is checked against the combined quantity).
        var merged = OrderCalculator.MergeLines(lines);

        var agentExists = await _dbContext.Agents.AsNoTracking()
            .AnyAsync(a => a.AgentID == agentId, cancellationToken);
        if (!agentExists)
        {
            throw new ServiceValidationException("The agent account was not found.");
        }

        // The EF Core InMemory provider (used by unit tests) has no transactions.
        // Every check runs before the first write there, so an order is still all-or-nothing.
        if (!_dbContext.Database.IsRelational())
        {
            return await PlaceCoreAsync(agentId, merged, useAtomicStockUpdate: false, cancellationToken);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await PlaceCoreAsync(agentId, merged, useAtomicStockUpdate: true, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _dbContext.ChangeTracker.Clear();
            throw;
        }
    }

    private async Task<PlaceOrderResult> PlaceCoreAsync(
        int agentId,
        IReadOnlyList<OrderLineRequest> lines,
        bool useAtomicStockUpdate,
        CancellationToken cancellationToken)
    {
        var itemIds = lines.Select(l => l.ItemId).ToList();
        var items = await _dbContext.Items
            .Where(i => itemIds.Contains(i.ItemID))
            .ToDictionaryAsync(i => i.ItemID, cancellationToken);

        var missing = itemIds.Where(id => !items.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            throw new ServiceValidationException(
                $"Item {string.Join(", ", missing)} does not exist in the catalog.");
        }

        // Check ALL lines first so the agent sees every shortage at once.
        var shortages = lines
            .Where(l => items[l.ItemId].StockQuantity < l.Quantity)
            .Select(l => ToShortage(items[l.ItemId], l.Quantity, items[l.ItemId].StockQuantity))
            .ToList();
        if (shortages.Count > 0)
        {
            throw new InsufficientStockException(shortages);
        }

        var order = new Order
        {
            AgentID = agentId,                 // always taken from the login, never from the form
            OrderDate = DateTime.Now,
            OrderStatus = PendingStatus
        };

        foreach (var line in lines)
        {
            var item = items[line.ItemId];
            order.OrderDetails.Add(new OrderDetail
            {
                ItemID = item.ItemID,
                Quantity = line.Quantity,
                UnitAmount = item.Price        // price at the time of the order
            });
        }

        _dbContext.Orders.Add(order);

        if (useAtomicStockUpdate)
        {
            // UPDATE ... WHERE StockQuantity >= @qty: if another agent took the stock between
            // our check and this statement, 0 rows are changed and the whole order is rolled back.
            foreach (var line in lines)
            {
                var itemId = line.ItemId;
                var quantity = line.Quantity;
                var affected = await _dbContext.Items
                    .Where(i => i.ItemID == itemId && i.StockQuantity >= quantity)
                    .ExecuteUpdateAsync(
                        s => s.SetProperty(i => i.StockQuantity, i => i.StockQuantity - quantity),
                        cancellationToken);

                if (affected != 1)
                {
                    var available = await _dbContext.Items.AsNoTracking()
                        .Where(i => i.ItemID == itemId)
                        .Select(i => i.StockQuantity)
                        .FirstOrDefaultAsync(cancellationToken);
                    throw new InsufficientStockException(new[] { ToShortage(items[itemId], quantity, available) });
                }
            }
        }
        else
        {
            foreach (var line in lines)
            {
                items[line.ItemId].StockQuantity -= line.Quantity;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var total = OrderCalculator.CalculateTotal(order.OrderDetails.Select(d => (d.Quantity, d.UnitAmount)));
        return new PlaceOrderResult(order.OrderID, total, order.OrderDetails.Count);
    }

    private static StockShortage ToShortage(Item item, int requested, int available) =>
        new(item.ItemID, item.ItemName, item.Size, requested, available);
}
