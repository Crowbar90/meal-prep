namespace MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence-layer projection of an aggregated weekly nutrition summary. JSONB.
/// </summary>
public sealed class MealPlanNutritionSummaryDocument
{
    public decimal DailyCaloriesAverage { get; set; }

    public decimal DailyProteinAverage { get; set; }

    public decimal DailyCarbsAverage { get; set; }

    public decimal DailyFatAverage { get; set; }

    public List<string> DeficientMicronutrients { get; set; } = [];

    public List<string> ExcessMicronutrients { get; set; } = [];
}
