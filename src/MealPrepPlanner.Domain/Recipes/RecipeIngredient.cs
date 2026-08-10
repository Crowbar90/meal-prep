namespace MealPrepPlanner.Domain.Recipes;

using MealPrepPlanner.Domain.Shared;

/// <summary>
/// A line item within a <see cref="Recipe"/>. Modeled as a value object carrying a
/// snapshot of the referenced ingredient so the recipe stays self-contained for
/// domain services (nutrition, conflict detection, shopping generation).
/// </summary>
public sealed record RecipeIngredient(
    Guid IngredientId,
    string Name,
    Quantity Quantity,
    NutritionProfile NutritionPer100g,
    IReadOnlyList<string> Allergens,
    bool IsOptional,
    string? Preparation)
{
    public RecipeIngredient Scale(decimal factor)
    {
        var scaledAmount = Math.Round(Quantity.Amount * factor, 3, MidpointRounding.AwayFromZero);
        return this with { Quantity = new Quantity(scaledAmount, Quantity.Unit) };
    }
}
