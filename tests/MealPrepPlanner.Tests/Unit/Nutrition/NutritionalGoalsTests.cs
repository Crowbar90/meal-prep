namespace MealPrepPlanner.Tests.Unit.Nutrition;

using MealPrepPlanner.Domain.Nutrition;

public class NutritionalGoalsTests
{
    [Fact]
    public void CreateDefault_ReturnsStandardDailyTargets()
    {
        var goals = NutritionalGoals.CreateDefault();

        Assert.Equal(2000m, goals.CaloriesPerDay);
        Assert.Equal(100m, goals.ProteinPerDay);
        Assert.Equal(250m, goals.CarbsPerDay);
        Assert.Equal(70m, goals.FatPerDay);
    }

    [Fact]
    public void Constructor_NegativeTarget_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new NutritionalGoals(-1m, 100m, 250m, 70m));
    }
}
