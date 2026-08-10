namespace MealPrepPlanner.Tests.Unit.UserPreferences;

using MealPrepPlanner.Domain.Shared;
using MealPrepPlanner.Domain.UserPreferences;

public class PreferencesTests
{
    [Fact]
    public void Constructor_DefaultsAreEmptyOrStandard()
    {
        var preferences = new Preferences();

        Assert.Empty(preferences.DietaryRestrictions);
        Assert.Empty(preferences.Equipment);
        Assert.Empty(preferences.PreferredSupermarkets);
        Assert.Same(FoodPreferences.Empty, preferences.FoodPreferences);
        Assert.Equal(60, preferences.MaxCookingTimeMinutes);
        Assert.Equal(0m, preferences.WeeklyBudget.Amount);
        Assert.Equal("EUR", preferences.WeeklyBudget.Currency);
    }

    [Fact]
    public void Constructor_NonPositiveMaxCookingTime_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Preferences(maxCookingTimeMinutes: 0));
    }

    [Fact]
    public void Constructor_PreservesSuppliedValues()
    {
        var restrictions = new[] { new DietaryRestriction("vegan") };
        var supermarkets = new[] { "Tesco" };
        var equipment = new[] { new Equipment("oven") };

        var preferences = new Preferences(
            dietaryRestrictions: restrictions,
            maxCookingTimeMinutes: 30,
            weeklyBudget: Money.Zero("GBP"),
            preferredSupermarkets: supermarkets,
            equipment: equipment);

        Assert.Equal(restrictions, preferences.DietaryRestrictions);
        Assert.Equal(equipment, preferences.Equipment);
        Assert.Equal(30, preferences.MaxCookingTimeMinutes);
        Assert.Equal("GBP", preferences.WeeklyBudget.Currency);
        Assert.Equal(supermarkets, preferences.PreferredSupermarkets);
    }
}
