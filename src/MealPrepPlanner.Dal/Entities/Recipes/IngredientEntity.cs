namespace MealPrepPlanner.Dal.Entities.Recipes;

using MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.Recipes.Ingredient</c>.
/// <c>Allergens</c> is a <c>text[]</c> column; <c>NutritionPer100g</c> is JSONB.
/// </summary>
public class IngredientEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string DefaultUnit { get; set; } = "g";

    public NutritionProfileDocument NutritionPer100g { get; set; } = new();

    public string[] Allergens { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
}
