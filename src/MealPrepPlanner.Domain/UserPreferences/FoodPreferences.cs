namespace MealPrepPlanner.Domain.UserPreferences;

/// <summary>
/// Taste preferences: liked/disliked cuisines and ingredients, preferred proteins,
/// and a spice ceiling.
/// </summary>
public sealed record FoodPreferences
{
    public FoodPreferences(
        IReadOnlyList<string>? likedCuisines = null,
        IReadOnlyList<string>? dislikedIngredients = null,
        IReadOnlyList<string>? likedIngredients = null,
        IReadOnlyList<string>? preferredProteins = null,
        int maxSpiceLevel = 0)
    {
        LikedCuisines = likedCuisines ?? [];
        DislikedIngredients = dislikedIngredients ?? [];
        LikedIngredients = likedIngredients ?? [];
        PreferredProteins = preferredProteins ?? [];
        MaxSpiceLevel = maxSpiceLevel;
    }

    public static FoodPreferences Empty { get; } = new();

    public IReadOnlyList<string> LikedCuisines { get; }

    public IReadOnlyList<string> DislikedIngredients { get; }

    public IReadOnlyList<string> LikedIngredients { get; }

    public IReadOnlyList<string> PreferredProteins { get; }

    public int MaxSpiceLevel { get; }
}
