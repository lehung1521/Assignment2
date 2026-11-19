using B2BWholesale.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data.Services;

public sealed class ShippingManifestService : IShippingManifestService
{
    private readonly AppDbContext _dbContext;

    public ShippingManifestService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ShippingManifestData GetManifest(int orderId)
    {
        var order = _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.Agent)
            .Include(x => x.OrderDetails)
            .ThenInclude(detail => detail.Item)
            .FirstOrDefault(x => x.OrderID == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} was not found.");

        var lines = order.OrderDetails
            .Select(detail => new ShippingManifestLine(
                detail.Item?.ItemName ?? string.Empty,
                detail.Item?.Size ?? string.Empty,
                detail.Quantity,
                detail.UnitAmount,
                CalculateLineTotal(detail.Quantity, detail.UnitAmount)))
            .ToList();

        return new ShippingManifestData(
            order.OrderID,
            order.OrderDate,
            order.OrderStatus,
            order.Agent?.AgentName ?? string.Empty,
            order.Agent?.Address ?? string.Empty,
            order.Agent?.Phone ?? string.Empty,
            lines,
            lines.Sum(line => line.LineTotal));
    }

    public static decimal CalculateLineTotal(int quantity, decimal unitPrice)
    {
        if (quantity <= 0)
        {
            throw new ServiceValidationException("Order quantity must be greater than zero.");
        }

        if (unitPrice < 0)
        {
            throw new ServiceValidationException("Unit price cannot be negative.");
        }

        return checked(quantity * unitPrice);
    }
}
