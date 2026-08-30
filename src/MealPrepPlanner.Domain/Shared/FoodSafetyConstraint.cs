namespace MealPrepPlanner.Domain.Shared;

/// <summary>
/// Food-safety window for cooked prep food stored at refrigerator temperature
/// (2–4 °C). Default is 72 hours per USDA guidance for cooked food held in
/// the fridge; households override via <c>Preferences.FoodSafetyWindowHours</c>.
/// </summary>
public sealed class FoodSafetyConstraint : ValueObject
{
    public FoodSafetyConstraint(int maxStorageHours)
    {
        if (maxStorageHours <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxStorageHours), "Food-safety window must be a positive number of hours.");

        MaxStorageHours = maxStorageHours;
    }

    public static FoodSafetyConstraint Default => new(72);

    public int MaxStorageHours { get; }

    public TimeSpan MaxStorageDuration => TimeSpan.FromHours(MaxStorageHours);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MaxStorageHours;
    }
}
