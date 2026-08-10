namespace MealPrepPlanner.Tests.Unit.Shared;

using MealPrepPlanner.Domain.Shared;

public class MoneyTests
{
    [Fact]
    public void Zero_CreatesZeroAmountInGivenCurrency()
    {
        var money = Money.Zero("EUR");

        Assert.Equal(0m, money.Amount);
        Assert.Equal("EUR", money.Currency);
    }

    [Fact]
    public void Add_SameCurrency_SumsAmounts()
    {
        var sum = new Money(10.50m, "EUR") + new Money(2.25m, "EUR");

        Assert.Equal(new Money(12.75m, "EUR"), sum);
    }

    [Fact]
    public void Add_DifferentCurrencies_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => new Money(1m, "EUR") + new Money(1m, "USD"));

        Assert.Contains("different currencies", ex.Message);
        Assert.Contains("EUR", ex.Message);
        Assert.Contains("USD", ex.Message);
    }

    [Fact]
    public void Round_RoundsToTwoDecimalsAwayFromZero()
    {
        Assert.Equal(1.01m, new Money(1.005m, "EUR").Round().Amount);
        Assert.Equal(-1.01m, new Money(-1.005m, "EUR").Round().Amount);
    }

    [Fact]
    public void Round_PreservesCurrency()
    {
        Assert.Equal("USD", new Money(9.999m, "USD").Round().Currency);
    }

    [Fact]
    public void ToString_FormatsAmountWithTwoDecimalsAndCurrency()
    {
        Assert.Equal("12.50 EUR", new Money(12.5m, "EUR").ToString());
    }
}
