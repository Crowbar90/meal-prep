namespace MealPrepPlanner.Domain.Recipes;

using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Canonical ingredient reference with base nutritional data per 100g.
/// Standalone aggregate root.
/// </summary>
public class Ingredient : Entity
{
    private readonly List<string> _allergens = [];

    private Ingredient()
    {
        Name = string.Empty;
        DefaultUnit = string.Empty;
    }

    private Ingredient(
        Guid id,
        string name,
        string? category,
        string defaultUnit,
        NutritionProfile nutritionPer100g)
        : base(id)
    {
        Name = name;
        Category = category;
        DefaultUnit = defaultUnit;
        NutritionPer100g = nutritionPer100g;
    }

    public string Name { get; }

    public string? Category { get; }

    public string DefaultUnit { get; }

    public NutritionProfile NutritionPer100g { get; }

    public IReadOnlyList<string> Allergens => _allergens;

    public static Ingredient Create(
        string name,
        NutritionProfile nutritionPer100g,
        string? category = null,
        string defaultUnit = "g",
        IReadOnlyList<string>? allergens = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Ingredient name must not be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(defaultUnit))
            throw new ArgumentException("Default unit must not be empty.", nameof(defaultUnit));

        var ingredient = new Ingredient(Guid.NewGuid(), name, category, defaultUnit, nutritionPer100g);
        if (allergens is not null)
            ingredient._allergens.AddRange(allergens);

        return ingredient;
    }
}
