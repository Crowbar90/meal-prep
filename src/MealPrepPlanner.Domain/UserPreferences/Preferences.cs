namespace MealPrepPlanner.Domain.UserPreferences;

using MealPrepPlanner.Domain.Nutrition;
using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Consolidated, immutable household preferences. Replaced wholesale via
/// <see cref="Household.UpdatePreferences"/>.
/// </summary>
public sealed record Preferences
{
    public Preferences(
        IReadOnlyList<DietaryRestriction>? dietaryRestrictions = null,
        NutritionalGoals? nutritionGoals = null,
        IReadOnlyList<Equipment>? equipment = null,
        int maxCookingTimeMinutes = 60,
        Money? weeklyBudget = null,
        IReadOnlyList<string>? preferredSupermarkets = null,
        FoodPreferences? foodPreferences = null,
        int foodSafetyWindowHours = 72)
    {
        if (maxCookingTimeMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCookingTimeMinutes), "Max cooking time must be positive.");

        if (foodSafetyWindowHours <= 0)
            throw new ArgumentOutOfRangeException(nameof(foodSafetyWindowHours), "Food-safety window must be a positive number of hours.");

        DietaryRestrictions = dietaryRestrictions ?? [];
        NutritionGoals = nutritionGoals ?? NutritionalGoals.CreateDefault();
        Equipment = equipment ?? [];
        MaxCookingTimeMinutes = maxCookingTimeMinutes;
        WeeklyBudget = weeklyBudget ?? Money.Zero("EUR");
        PreferredSupermarkets = preferredSupermarkets ?? [];
        FoodPreferences = foodPreferences ?? FoodPreferences.Empty;
        FoodSafetyWindowHours = foodSafetyWindowHours;
    }

    public IReadOnlyList<DietaryRestriction> DietaryRestrictions { get; }

    public NutritionalGoals NutritionGoals { get; }

    public IReadOnlyList<Equipment> Equipment { get; }

    public int MaxCookingTimeMinutes { get; }

    public Money WeeklyBudget { get; }

    public IReadOnlyList<string> PreferredSupermarkets { get; }

    public FoodPreferences FoodPreferences { get; }

    public int FoodSafetyWindowHours { get; }
}
