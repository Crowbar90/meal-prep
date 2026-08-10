namespace MealPrepPlanner.Domain.Recipes;

using MealPrepPlanner.Domain.Shared;
using MealPrepPlanner.Domain.UserPreferences;

/// <summary>
/// Aggregate root describing a dish: instructions, timing, equipment, and ingredients.
/// </summary>
public class Recipe : Entity
{
    private readonly List<string> _instructions = [];
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<string> _tags = [];
    private readonly List<Equipment> _equipmentNeeded = [];

    private Recipe()
    {
        Name = string.Empty;
    }

    private Recipe(
        Guid id,
        string name,
        string? description,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int baseServings,
        string? source,
        Guid? createdBy)
        : base(id)
    {
        Name = name;
        Description = description;
        PrepTimeMinutes = prepTimeMinutes;
        CookTimeMinutes = cookTimeMinutes;
        BaseServings = baseServings;
        Source = source;
        CreatedBy = createdBy;
    }

    public string Name { get; }

    public string? Description { get; }

    public IReadOnlyList<string> Instructions => _instructions;

    public int PrepTimeMinutes { get; }

    public int CookTimeMinutes { get; }

    public int BaseServings { get; }

    public IReadOnlyList<RecipeIngredient> Ingredients => _ingredients;

    public IReadOnlyList<string> Tags => _tags;

    public IReadOnlyList<Equipment> EquipmentNeeded => _equipmentNeeded;

    public string? Source { get; }

    public Guid? CreatedBy { get; }

    public TimeSpan TotalTime => TimeSpan.FromMinutes(PrepTimeMinutes + CookTimeMinutes);

    public static Recipe Create(
        string name,
        string? description,
        IReadOnlyList<string> instructions,
        int prepTimeMinutes,
        int cookTimeMinutes,
        int baseServings,
        IReadOnlyList<string>? tags = null,
        IReadOnlyList<Equipment>? equipmentNeeded = null,
        string? source = null,
        Guid? createdBy = null,
        IReadOnlyList<RecipeIngredient>? ingredients = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Recipe name must not be empty.", nameof(name));

        ArgumentNullException.ThrowIfNull(instructions);

        if (instructions.Count == 0)
            throw new ArgumentException("A recipe must have at least one instruction step.", nameof(instructions));

        if (prepTimeMinutes < 0 || cookTimeMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(prepTimeMinutes), "Times must be non-negative.");

        if (baseServings <= 0)
            throw new ArgumentOutOfRangeException(nameof(baseServings), "Base servings must be a positive number.");

        var recipe = new Recipe(
            Guid.NewGuid(),
            name,
            description,
            prepTimeMinutes,
            cookTimeMinutes,
            baseServings,
            source,
            createdBy);

        recipe._instructions.AddRange(instructions);
        recipe._tags.AddRange(tags ?? []);
        recipe._equipmentNeeded.AddRange(equipmentNeeded ?? []);
        recipe._ingredients.AddRange(ingredients ?? []);
        return recipe;
    }

    public void AddIngredient(
        Ingredient ingredient,
        Quantity quantity,
        bool isOptional = false,
        string? preparation = null)
    {
        ArgumentNullException.ThrowIfNull(ingredient);

        if (quantity.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Ingredient quantity must be positive.");

        if (string.IsNullOrWhiteSpace(quantity.Unit))
            throw new ArgumentException("Ingredient quantity unit must not be empty.", nameof(quantity));

        _ingredients.Add(new RecipeIngredient(
            ingredient.Id,
            ingredient.Name,
            quantity,
            ingredient.NutritionPer100g,
            [.. ingredient.Allergens],
            isOptional,
            preparation));
    }
}
