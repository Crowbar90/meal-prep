namespace MealPrepPlanner.Domain.Recipes.Services;

/// <summary>
/// Scales a recipe to a target number of servings. Deterministic linear scaling;
/// ingredient quantities and base servings change, everything else is preserved.
/// </summary>
public sealed class RecipeScaler
{
    private const int QuantityDecimals = 3;

    public Recipe Scale(Recipe recipe, int targetServings)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (targetServings <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetServings), "Target servings must be a positive number.");

        var factor = (decimal)targetServings / recipe.BaseServings;

        var scaledIngredients = recipe.Ingredients
            .Select(i => i.Scale(factor))
            .ToArray();

        return Recipe.Create(
            recipe.Name,
            recipe.Description,
            recipe.Instructions,
            recipe.PrepTimeMinutes,
            recipe.CookTimeMinutes,
            targetServings,
            recipe.Tags,
            recipe.EquipmentNeeded,
            recipe.Source,
            recipe.CreatedBy,
            scaledIngredients);
    }
}
