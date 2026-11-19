using B2BWholesale.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data.Services;

public sealed class OrderManagementService : IOrderManagementService
{
    private static readonly IReadOnlyList<string> AllowedStatuses =
        Array.AsReadOnly(new[] { "Pending", "Shipped", "Delivered", "Cancelled" });
    private static readonly HashSet<string> ValidStatuses = new(AllowedStatuses, StringComparer.Ordinal);

    private readonly AppDbContext _dbContext;

    public OrderManagementService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<string> GetAllowedStatuses() => AllowedStatuses;

    public IReadOnlyList<OrderSummary> GetOrders() => _dbContext.Orders
        .AsNoTracking()
        .Include(order => order.Agent)
        .Include(order => order.OrderDetails)
        .OrderByDescending(order => order.OrderDate)
        .ThenByDescending(order => order.OrderID)
        .AsEnumerable()
        .Select(order => new OrderSummary(
            order.OrderID,
            order.OrderDate,
            order.Agent?.AgentName ?? string.Empty,
            order.OrderStatus,
            order.OrderDetails.Sum(detail => ShippingManifestService.CalculateLineTotal(detail.Quantity, detail.UnitAmount))))
        .ToList();

    public IReadOnlyList<OrderDetailLine> GetOrderDetails(int orderId)
    {
        var order = _dbContext.Orders
            .AsNoTracking()
            .Include(x => x.OrderDetails)
            .ThenInclude(detail => detail.Item)
            .FirstOrDefault(x => x.OrderID == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} was not found.");

        return order.OrderDetails
            .Select(detail => new OrderDetailLine(
                detail.Item?.ItemName ?? string.Empty,
                detail.Item?.Size ?? string.Empty,
                detail.Quantity,
                detail.UnitAmount,
                ShippingManifestService.CalculateLineTotal(detail.Quantity, detail.UnitAmount)))
            .ToList();
    }

    public void UpdateOrderStatus(int orderId, string? status)
    {
        if (string.IsNullOrWhiteSpace(status) || !ValidStatuses.Contains(status))
        {
            throw new ServiceValidationException("Choose a valid order status.");
        }

        var order = _dbContext.Orders.FirstOrDefault(x => x.OrderID == orderId)
            ?? throw new KeyNotFoundException($"Order {orderId} was not found.");
        order.OrderStatus = status;
        _dbContext.SaveChanges();
    }
}
