namespace MealPrepPlanner.Tests.Unit.Shared;

using MealPrepPlanner.Domain.Shared;

public class QuantityTests
{
    [Fact]
    public void Zero_CreatesZeroAmountInGivenUnit()
    {
        var quantity = Quantity.Zero("g");

        Assert.Equal(0m, quantity.Amount);
        Assert.Equal("g", quantity.Unit);
    }

    [Fact]
    public void Add_SameUnit_SumsAmounts()
    {
        var sum = new Quantity(150m, "g") + new Quantity(250m, "g");

        Assert.Equal(new Quantity(400m, "g"), sum);
    }

    [Fact]
    public void Add_DifferentUnits_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => new Quantity(1m, "g") + new Quantity(1m, "kg"));

        Assert.Contains("different units", ex.Message);
        Assert.Contains("g", ex.Message);
        Assert.Contains("kg", ex.Message);
    }

    [Fact]
    public void Round_RoundsAmountAwayFromZeroAndKeepsUnit()
    {
        Assert.Equal(new Quantity(1.005m, "g"), new Quantity(1.0045m, "g").Round(3));
        Assert.Equal(new Quantity(12.3m, "g"), new Quantity(12.34m, "g").Round(1));
    }

    [Fact]
    public void Equality_ComparesAmountAndUnit()
    {
        Assert.Equal(new Quantity(5m, "g"), new Quantity(5m, "g"));
        Assert.NotEqual(new Quantity(5m, "kg"), new Quantity(5m, "g"));
        Assert.NotEqual(new Quantity(6m, "g"), new Quantity(5m, "g"));
    }
}
