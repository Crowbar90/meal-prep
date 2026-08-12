namespace MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence-layer projection of a household's food preferences JSONB document.
/// </summary>
public sealed class FoodPreferencesDocument
{
    public List<string> LikedCuisines { get; set; } = [];

    public List<string> DislikedIngredients { get; set; } = [];

    public List<string> LikedIngredients { get; set; } = [];

    public List<string> PreferredProteins { get; set; } = [];

    public int MaxSpiceLevel { get; set; }
}
