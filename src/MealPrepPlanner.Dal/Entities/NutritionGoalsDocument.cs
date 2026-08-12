namespace MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence-layer projection of a household's preferences JSONB document.
/// Maps to a column with type <c>jsonb</c>.
/// </summary>
public sealed class NutritionGoalsDocument
{
    public decimal CaloriesPerDay { get; set; }

    public decimal ProteinPerDay { get; set; }

    public decimal CarbsPerDay { get; set; }

    public decimal FatPerDay { get; set; }

    public decimal? FiberPerDay { get; set; }

    public decimal? SodiumMgPerDay { get; set; }
}
