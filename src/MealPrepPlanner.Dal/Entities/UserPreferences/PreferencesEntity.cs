namespace MealPrepPlanner.Dal.Entities.UserPreferences;

using MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.UserPreferences.Preferences</c>.
/// One row per household (UNIQUE on HouseholdId). JSONB columns hold the
/// structured documents; scalar lists are stored as <c>text[]</c>.
/// </summary>
public class PreferencesEntity
{
    public Guid Id { get; set; }

    public Guid HouseholdId { get; set; }

    /// <summary>Scalar list, persisted as <c>text[]</c>.</summary>
    public string[] DietaryRestrictions { get; set; } = [];

    public NutritionGoalsDocument NutritionGoals { get; set; } = new();

    /// <summary>Scalar list, persisted as <c>text[]</c>.</summary>
    public string[] Equipment { get; set; } = [];

    public int MaxCookingTimeMinutes { get; set; }

    public decimal WeeklyBudgetAmount { get; set; }

    public string WeeklyBudgetCurrency { get; set; } = "EUR";

    /// <summary>Scalar list, persisted as <c>text[]</c>.</summary>
    public string[] PreferredSupermarkets { get; set; } = [];

    public FoodPreferencesDocument FoodPreferences { get; set; } = new();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
