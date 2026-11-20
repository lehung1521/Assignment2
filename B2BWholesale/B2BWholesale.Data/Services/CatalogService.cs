using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data.Services;

public sealed class CatalogService : ICatalogService
{
    private readonly AppDbContext _dbContext;

    public CatalogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CatalogPage> GetCatalogAsync(
        string? search, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _dbContext.Items.AsNoTracking();
        var term = search?.Trim();
        if (!string.IsNullOrEmpty(term))
        {
            query = query.Where(i => i.ItemName.Contains(term) || i.Size.Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)pageSize));
        page = Math.Clamp(page, 1, totalPages);

        var items = await query
            .OrderBy(i => i.ItemName)
            .ThenBy(i => i.Size)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new ItemRecord(i.ItemID, i.ItemName, i.Size, i.Price, i.StockQuantity))
            .ToListAsync(cancellationToken);

        return new CatalogPage(items, page, pageSize, total);
    }

    public async Task<IReadOnlyList<ItemRecord>> GetAllItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Items
            .AsNoTracking()
            .OrderBy(i => i.ItemName)
            .ThenBy(i => i.Size)
            .Select(i => new ItemRecord(i.ItemID, i.ItemName, i.Size, i.Price, i.StockQuantity))
            .ToListAsync(cancellationToken);
    }
}
