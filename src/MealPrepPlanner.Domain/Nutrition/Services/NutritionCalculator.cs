namespace MealPrepPlanner.Domain.Nutrition.Services;

using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Deterministic nutrition calculations. The domain does not trust AI for any math here.
///
/// Assumptions:
/// - Ingredient quantities are expressed in grams and nutrition is per 100g.
///   Non-gram units are rejected; this is documented in data-model.md (default unit "g").
/// - Optional ingredients are excluded from the calculation by default.
/// </summary>
public sealed class NutritionCalculator
{
    private const int RoundingDecimals = 1;
    private const decimal CalorieTolerancePercent = 0.05m;
    private const decimal MacroTolerancePercent = 0.10m;

    public NutritionProfile Calculate(Recipe recipe, int servings)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (servings <= 0)
            throw new ArgumentOutOfRangeException(nameof(servings), "Servings must be a positive number.");

        var total = new NutritionProfile(0m, 0m, 0m, 0m);

        foreach (var ingredient in recipe.Ingredients)
        {
            if (ingredient.IsOptional)
                continue;

            var grams = ToGrams(ingredient.Quantity);
            total += ingredient.NutritionPer100g.Scale(grams / 100m);
        }

        return total.Scale(1m / servings).Round(RoundingDecimals);
    }

    /// <summary>
    /// Checks a daily profile against goals. Tolerance: ±5% for calories,
    /// ±10% for macros (per bounded-contexts.md).
    /// </summary>
    public bool ValidateAgainstGoals(
        NutritionProfile daily,
        NutritionalGoals goals,
        out string[] warnings)
    {
        ArgumentNullException.ThrowIfNull(goals);

        var issues = new List<string>();

        CheckWithinTolerance("Calories", daily.Calories, goals.CaloriesPerDay, CalorieTolerancePercent, issues);
        CheckWithinTolerance("Protein", daily.Protein, goals.ProteinPerDay, MacroTolerancePercent, issues);
        CheckWithinTolerance("Carbs", daily.Carbs, goals.CarbsPerDay, MacroTolerancePercent, issues);
        CheckWithinTolerance("Fat", daily.Fat, goals.FatPerDay, MacroTolerancePercent, issues);

        if (goals.FiberPerDay is { } fiberGoal && daily.Fiber is { } fiber)
            CheckWithinTolerance("Fiber", fiber, fiberGoal, MacroTolerancePercent, issues);

        if (goals.SodiumMgPerDay is { } sodiumGoal && daily.SodiumMg is { } sodium)
            CheckWithinTolerance("Sodium", sodium, sodiumGoal, MacroTolerancePercent, issues);

        warnings = [.. issues];
        return issues.Count == 0;
    }

    private static decimal ToGrams(Quantity quantity)
    {
        if (!string.Equals(quantity.Unit, "g", StringComparison.OrdinalIgnoreCase))
        {
            throw new NotSupportedException(
                $"Nutrition calculation supports gram quantities only; got unit '{quantity.Unit}'.");
        }

        return quantity.Amount;
    }

    private static void CheckWithinTolerance(
        string nutrient,
        decimal actual,
        decimal target,
        decimal tolerance,
        List<string> issues)
    {
        if (target <= 0)
            return;

        var deviation = Math.Abs(actual - target) / target;
        if (deviation > tolerance)
        {
            var direction = actual > target ? "over" : "under";
            issues.Add(
                $"{nutrient} is {actual:0.#} vs. target {target:0.#} ({direction} by {deviation:P0}, limit {tolerance:P0}).");
        }
    }
}
