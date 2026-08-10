namespace MealPrepPlanner.Domain.Shared;

/// <summary>
/// A measured amount expressed in a unit of measurement.
/// </summary>
public readonly record struct Quantity(decimal Amount, string Unit)
{
    public static Quantity Zero(string unit) => new(0m, unit);

    public static Quantity operator +(Quantity left, Quantity right) =>
        left.Unit == right.Unit
            ? new Quantity(left.Amount + right.Amount, left.Unit)
            : throw new InvalidOperationException(
                $"Cannot add quantities in different units: '{left.Unit}' and '{right.Unit}'.");

    public Quantity Round(int decimals) =>
        new(Math.Round(Amount, decimals, MidpointRounding.AwayFromZero), Unit);

    public override string ToString() => $"{Amount:0.###} {Unit}";
}
