namespace B2BWholesale.Data.Services;

/// <summary>One requested line of a master-detail order (what the agent typed in).</summary>
public sealed record OrderLineRequest(int ItemId, int Quantity);

public sealed record PlaceOrderResult(int OrderId, decimal Total, int LineCount);

public sealed record StockShortage(int ItemId, string ItemName, string Size, int Requested, int Available);

/// <summary>
/// Thrown when at least one requested line is larger than the stock on hand.
/// The whole order is blocked: nothing is saved and no stock is changed.
/// </summary>
public sealed class InsufficientStockException : Exception
{
    public IReadOnlyList<StockShortage> Shortages { get; }

    public InsufficientStockException(IReadOnlyList<StockShortage> shortages)
        : base(BuildMessage(shortages))
    {
        Shortages = shortages;
    }

    private static string BuildMessage(IReadOnlyList<StockShortage> shortages)
    {
        var parts = shortages.Select(s =>
            $"{s.ItemName} ({s.Size}): requested {s.Requested}, only {s.Available} in stock");
        return "Not enough stock. The order was NOT placed. " + string.Join("; ", parts) + ".";
    }
}

public sealed record CatalogPage(IReadOnlyList<ItemRecord> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}

// ---------- Dashboard (Task 1.4) ----------

public sealed record BestSellerRow(int Rank, int ItemId, string ItemName, string Size, int UnitsSold, decimal Revenue);

public sealed record LowStockRow(int ItemId, string ItemName, string Size, int StockQuantity, int Threshold);

public sealed record AgentRevenueRow(int AgentId, string AgentName, int OrderCount, decimal Revenue);
