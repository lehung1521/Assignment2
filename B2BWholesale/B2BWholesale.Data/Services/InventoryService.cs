using System.Globalization;
using B2BWholesale.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace B2BWholesale.Data.Services;

public sealed class InventoryService : IInventoryService
{
    private readonly AppDbContext _dbContext;

    public InventoryService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IReadOnlyList<ItemRecord> GetItems() => _dbContext.Items
        .AsNoTracking()
        .OrderBy(item => item.ItemName)
        .Select(item => ToRecord(item))
        .ToList();

    public ItemRecord? GetItem(int itemId)
    {
        var item = _dbContext.Items.AsNoTracking().FirstOrDefault(x => x.ItemID == itemId);
        return item is null ? null : ToRecord(item);
    }

    public ItemRecord CreateItem(ItemInput input)
    {
        var values = Validate(input, null);
        var item = new Item
        {
            ItemName = values.ItemName,
            Size = values.Size,
            Price = values.Price,
            StockQuantity = values.StockQuantity
        };
        _dbContext.Items.Add(item);
        SaveItem("An item with this name and size already exists or the item violates a database constraint.");
        return ToRecord(item);
    }

    public ItemRecord UpdateItem(int itemId, ItemInput input)
    {
        var item = _dbContext.Items.FirstOrDefault(x => x.ItemID == itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} was not found.");
        var values = Validate(input, itemId);
        item.ItemName = values.ItemName;
        item.Size = values.Size;
        item.Price = values.Price;
        item.StockQuantity = values.StockQuantity;
        SaveItem("The item could not be saved because its name/size is already in use or a database constraint was violated.");
        return ToRecord(item);
    }

    public void DeleteItem(int itemId)
    {
        var item = _dbContext.Items.FirstOrDefault(x => x.ItemID == itemId)
            ?? throw new KeyNotFoundException($"Item {itemId} was not found.");
        _dbContext.Items.Remove(item);
        try
        {
            _dbContext.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ServiceValidationException("This item cannot be deleted because it appears in order history.");
        }
    }

    private (string ItemName, string Size, decimal Price, int StockQuantity) Validate(ItemInput input, int? currentItemId)
    {
        ArgumentNullException.ThrowIfNull(input);
        var name = input.ItemName?.Trim() ?? string.Empty;
        var size = input.Size?.Trim() ?? string.Empty;

        if (name.Length == 0 || name.Length > 100)
        {
            throw new ServiceValidationException("Item name is required and must be at most 100 characters.");
        }

        if (size.Length == 0 || size.Length > 50)
        {
            throw new ServiceValidationException("Item size is required and must be at most 50 characters.");
        }

        if (!decimal.TryParse(input.Price, NumberStyles.Number, CultureInfo.CurrentCulture, out var price) || price <= 0)
        {
            throw new ServiceValidationException("Price must be a positive number.");
        }

        if (!int.TryParse(input.StockQuantity, NumberStyles.None, CultureInfo.CurrentCulture, out var stock) || stock < 0)
        {
            throw new ServiceValidationException("Stock quantity must be a non-negative whole number.");
        }

        if (_dbContext.Items.Any(x =>
            x.ItemName == name && x.Size == size && (!currentItemId.HasValue || x.ItemID != currentItemId.Value)))
        {
            throw new ServiceValidationException("An item with this name and size already exists.");
        }

        return (name, size, price, stock);
    }

    private void SaveItem(string constraintMessage)
    {
        try
        {
            _dbContext.SaveChanges();
        }
        catch (DbUpdateException)
        {
            throw new ServiceValidationException(constraintMessage);
        }
    }

    private static ItemRecord ToRecord(Item item) =>
        new(item.ItemID, item.ItemName, item.Size, item.Price, item.StockQuantity);
}
