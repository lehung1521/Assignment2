using B2BWholesale.Data.Services;

namespace B2BWholesale.Tests;

/// <summary>Task 3.2 business rules that need no database.</summary>
public sealed class OrderCalculatorTests
{
    [Fact]
    public void MergeLines_CombinesDuplicateItemsAndSortsByItemId()
    {
        var merged = OrderCalculator.MergeLines(new[]
        {
            new OrderLineRequest(7, 2),
            new OrderLineRequest(3, 5),
            new OrderLineRequest(7, 4)
        });

        Assert.Equal(2, merged.Count);
        Assert.Equal(new OrderLineRequest(3, 5), merged[0]);
        Assert.Equal(new OrderLineRequest(7, 6), merged[1]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void MergeLines_RejectsZeroOrNegativeQuantity(int quantity)
    {
        var ex = Assert.Throws<ServiceValidationException>(() =>
            OrderCalculator.MergeLines(new[] { new OrderLineRequest(1, quantity) }));

        Assert.Contains("greater than zero", ex.Message);
    }

    [Fact]
    public void MergeLines_RejectsEmptyOrder()
    {
        Assert.Throws<ServiceValidationException>(() => OrderCalculator.MergeLines(Array.Empty<OrderLineRequest>()));
        Assert.Throws<ServiceValidationException>(() => OrderCalculator.MergeLines(null));
    }

    [Fact]
    public void CalculateTotal_SumsQuantityTimesUnitPrice()
    {
        var total = OrderCalculator.CalculateTotal(new[] { (3, 100m), (2, 250m), (1, 99.5m) });

        Assert.Equal(899.5m, total);
    }
}
