namespace B2BWholesale.Data.Services;

/// <summary>Catalog of items shown in the Agent Portal (Task 3.2).</summary>
public interface ICatalogService
{
    Task<CatalogPage> GetCatalogAsync(string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ItemRecord>> GetAllItemsAsync(CancellationToken cancellationToken = default);
}

/// <summary>Places a master-detail order inside ONE database transaction (Task 3.2).</summary>
public interface IOrderPlacementService
{
    Task<PlaceOrderResult> PlaceOrderAsync(int agentId, IEnumerable<OrderLineRequest> lines, CancellationToken cancellationToken = default);
}

/// <summary>Queries behind the back-office dashboard (Task 1.4).</summary>
public interface IDashboardService
{
    IReadOnlyList<BestSellerRow> GetBestSellingItems(DateTime? from, DateTime? to, int top);
    IReadOnlyList<LowStockRow> GetLowStockItems(int threshold);
    IReadOnlyList<AgentRevenueRow> GetRevenueByAgent(int? agentId, DateTime? from, DateTime? to);
}
