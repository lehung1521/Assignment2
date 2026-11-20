namespace B2BWholesale.Data.Services;

/// <summary>Pure business rules for an order (no database), so they are easy to unit test.</summary>
public static class OrderCalculator
{
    public const int MaxLines = 50;
    public const int MaxQuantityPerLine = 100_000;

    /// <summary>
    /// Validates the requested lines and merges duplicate items into one line.
    /// The result is sorted by ItemId so stock rows are always locked in the same order
    /// (this avoids deadlocks when two agents order at the same time).
    /// </summary>
    public static IReadOnlyList<OrderLineRequest> MergeLines(IEnumerable<OrderLineRequest>? lines)
    {
        if (lines is null)
        {
            throw new ServiceValidationException("The order must contain at least one item.");
        }

        var totals = new SortedDictionary<int, long>();
        foreach (var line in lines)
        {
            if (line.ItemId <= 0)
            {
                throw new ServiceValidationException("Choose a valid item for every order line.");
            }

            if (line.Quantity <= 0)
            {
                throw new ServiceValidationException("Quantity must be greater than zero.");
            }

            totals[line.ItemId] = totals.TryGetValue(line.ItemId, out var existing)
                ? existing + line.Quantity
                : line.Quantity;
        }

        if (totals.Count == 0)
        {
            throw new ServiceValidationException("The order must contain at least one item.");
        }

        if (totals.Count > MaxLines)
        {
            throw new ServiceValidationException($"An order can contain at most {MaxLines} different items.");
        }

        if (totals.Values.Any(q => q > MaxQuantityPerLine))
        {
            throw new ServiceValidationException($"Quantity per item cannot exceed {MaxQuantityPerLine:N0}.");
        }

        return totals.Select(x => new OrderLineRequest(x.Key, (int)x.Value)).ToList();
    }

    /// <summary>Sum of Quantity x UnitPrice over all lines.</summary>
    public static decimal CalculateTotal(IEnumerable<(int Quantity, decimal UnitPrice)> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        return lines.Sum(line => line.Quantity * line.UnitPrice);
    }
}
