namespace B2BWholesale.Data.Services;

public interface IAdminAuthenticationService
{
    AdminLoginResult AuthenticateAdmin(string? usernameOrEmail, string? password);
}

public interface IInventoryService
{
    IReadOnlyList<ItemRecord> GetItems();
    ItemRecord? GetItem(int itemId);
    ItemRecord CreateItem(ItemInput input);
    ItemRecord UpdateItem(int itemId, ItemInput input);
    void DeleteItem(int itemId);
}

public interface IAgentManagementService
{
    IReadOnlyList<AgentRecord> GetAgents();
    AgentRecord? GetAgent(int agentId);
    AgentRecord CreateAgent(AgentInput input);
    AgentRecord UpdateAgent(int agentId, AgentInput input);
    void DeleteAgent(int agentId);
}

public interface IOrderManagementService
{
    IReadOnlyList<string> GetAllowedStatuses();
    IReadOnlyList<OrderSummary> GetOrders();
    IReadOnlyList<OrderDetailLine> GetOrderDetails(int orderId);
    void UpdateOrderStatus(int orderId, string? status);
}

public interface IShippingManifestService
{
    ShippingManifestData GetManifest(int orderId);
}

public interface IShippingManifestPdfGenerator
{
    byte[] GeneratePdf(ShippingManifestData manifest);
}
