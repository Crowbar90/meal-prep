namespace MealPrepPlanner.Domain.Nutrition;

/// <summary>
/// Per-day nutritional targets for a household.
/// </summary>
public sealed record NutritionalGoals
{
    public NutritionalGoals(
        decimal caloriesPerDay,
        decimal proteinPerDay,
        decimal carbsPerDay,
        decimal fatPerDay,
        decimal? fiberPerDay = null,
        decimal? sodiumMgPerDay = null)
    {
        if (caloriesPerDay < 0 || proteinPerDay < 0 || carbsPerDay < 0 || fatPerDay < 0)
            throw new ArgumentOutOfRangeException(nameof(caloriesPerDay), "Nutritional goals must be non-negative.");

        CaloriesPerDay = caloriesPerDay;
        ProteinPerDay = proteinPerDay;
        CarbsPerDay = carbsPerDay;
        FatPerDay = fatPerDay;
        FiberPerDay = fiberPerDay;
        SodiumMgPerDay = sodiumMgPerDay;
    }

    public decimal CaloriesPerDay { get; }

    public decimal ProteinPerDay { get; }

    public decimal CarbsPerDay { get; }

    public decimal FatPerDay { get; }

    public decimal? FiberPerDay { get; }

    public decimal? SodiumMgPerDay { get; }

    public static NutritionalGoals CreateDefault() => new(2000m, 100m, 250m, 70m);
}
