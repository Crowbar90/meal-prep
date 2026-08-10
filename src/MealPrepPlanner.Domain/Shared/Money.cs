namespace MealPrepPlanner.Domain.Shared;

/// <summary>
/// Monetary amount with an ISO 4217 currency code.
/// </summary>
public readonly record struct Money(decimal Amount, string Currency)
{
    public static Money Zero(string currency) => new(0m, currency);

    public static Money operator +(Money left, Money right) =>
        left.Currency == right.Currency
            ? new Money(left.Amount + right.Amount, left.Currency)
            : throw new InvalidOperationException(
                $"Cannot add amounts in different currencies: '{left.Currency}' and '{right.Currency}'.");

    public Money Round() => new(Math.Round(Amount, 2, MidpointRounding.AwayFromZero), Currency);

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
