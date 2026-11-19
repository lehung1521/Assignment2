using B2BWholesale.Data.Models;

namespace B2BWholesale.Data.Services;

public sealed record ItemInput(string ItemName, string Size, string Price, string StockQuantity);

public sealed record ItemRecord(int ItemId, string ItemName, string Size, decimal Price, int StockQuantity);

public sealed record AgentInput(string AgentName, string Address, string Phone);

public sealed record AgentRecord(int AgentId, string AgentName, string Address, string Phone);

public enum AdminLoginStatus
{
    MissingCredentials,
    Success,
    InvalidCredentials,
    AccountLocked,
    AgentAccessDenied
}

public sealed record AdminLoginResult(AdminLoginStatus Status, User? User = null, int AttemptsRemaining = 0);

public sealed record OrderSummary(
    int OrderId,
    DateTime OrderDate,
    string AgentName,
    string OrderStatus,
    decimal Total);

public sealed record OrderDetailLine(
    string ItemName,
    string Size,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record ShippingManifestLine(
    string ItemName,
    string Size,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record ShippingManifestData(
    int OrderId,
    DateTime OrderDate,
    string OrderStatus,
    string AgentName,
    string AgentAddress,
    string AgentPhone,
    IReadOnlyList<ShippingManifestLine> Lines,
    decimal GrandTotal);

public sealed class ServiceValidationException : Exception
{
    public ServiceValidationException(string message) : base(message) { }
}
