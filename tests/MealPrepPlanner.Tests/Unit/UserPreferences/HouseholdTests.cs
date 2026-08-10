namespace MealPrepPlanner.Tests.Unit.UserPreferences;

using MealPrepPlanner.Domain.Nutrition;
using MealPrepPlanner.Domain.UserPreferences;

public class HouseholdTests
{
    [Fact]
    public void Create_EmptyName_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => Household.Create("  "));

        Assert.Contains("must not be empty", ex.Message);
    }

    [Fact]
    public void AddMember_AddsMemberWithDetails()
    {
        var household = Household.Create("The Smiths");

        var member = household.AddMember("Alice", 32, Sex.Female, 60m, 165, ActivityLevel.Moderate);

        Assert.Equal("Alice", member.Name);
        Assert.Equal(32, member.Age);
        Assert.Equal(Sex.Female, member.Sex);
        Assert.Equal(60m, member.WeightKg);
        Assert.Equal(165, member.HeightCm);
        Assert.Equal(ActivityLevel.Moderate, member.ActivityLevel);
        Assert.Single(household.Members);
    }

    [Fact]
    public void AddMember_DuplicateName_Throws()
    {
        var household = Household.Create("The Smiths");
        household.AddMember("Alice", 32, Sex.Female, 60m, 165, ActivityLevel.Moderate);

        var ex = Assert.Throws<InvalidOperationException>(
            () => household.AddMember("alice", 40, Sex.Male, 80m, 180, ActivityLevel.Active));

        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public void AddMember_NonPositiveAge_Throws()
    {
        var household = Household.Create("The Smiths");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => household.AddMember("Alice", 0, Sex.Female, 60m, 165, ActivityLevel.Moderate));
    }

    [Fact]
    public void AddMember_NonPositiveWeight_Throws()
    {
        var household = Household.Create("The Smiths");

        Assert.Throws<ArgumentOutOfRangeException>(
            () => household.AddMember("Alice", 32, Sex.Female, 0m, 165, ActivityLevel.Moderate));
    }

    [Fact]
    public void UpdatePreferences_ReplacesPreferences()
    {
        var household = Household.Create("The Smiths");
        var first = new Preferences(maxCookingTimeMinutes: 30);
        var second = new Preferences(
            nutritionGoals: new NutritionalGoals(2500m, 120m, 300m, 80m),
            maxCookingTimeMinutes: 45);

        household.UpdatePreferences(first);
        household.UpdatePreferences(second);

        Assert.Same(second, household.Preferences);
        Assert.Equal(45, household.Preferences!.MaxCookingTimeMinutes);
    }

    [Fact]
    public void UpdatePreferences_Null_Throws()
    {
        var household = Household.Create("The Smiths");

        Assert.Throws<ArgumentNullException>(() => household.UpdatePreferences(null!));
    }
}
