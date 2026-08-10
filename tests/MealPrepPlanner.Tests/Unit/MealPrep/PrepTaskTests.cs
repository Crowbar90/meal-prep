namespace MealPrepPlanner.Tests.Unit.MealPrep;

using MealPrepPlanner.Domain.MealPrep;
using MealPrepPlanner.Domain.UserPreferences;

public class PrepTaskTests
{
    [Fact]
    public void Create_SetsDetailsAndCollections()
    {
        var task = PrepTask.Create(
            DayOfWeek.Sunday,
            "Batch cook chicken",
            90,
            ["Chicken Curry", "Chicken Rice"],
            [new Equipment("oven")],
            "Reheat at 180C for 10 minutes");

        Assert.Equal(DayOfWeek.Sunday, task.Day);
        Assert.Equal("Batch cook chicken", task.Description);
        Assert.Equal(90, task.DurationMinutes);
        Assert.Equal(["Chicken Curry", "Chicken Rice"], task.RecipesUsing);
        Assert.Equal([new Equipment("oven")], task.EquipmentNeeded);
        Assert.Equal("Reheat at 180C for 10 minutes", task.ReheatingInstructions);
    }

    [Fact]
    public void Create_DefaultsCollectionsToEmpty()
    {
        var task = PrepTask.Create(DayOfWeek.Monday, "Chop vegetables", 20);

        Assert.Empty(task.RecipesUsing);
        Assert.Empty(task.EquipmentNeeded);
        Assert.Null(task.ReheatingInstructions);
    }

    [Fact]
    public void Create_EmptyDescription_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => PrepTask.Create(DayOfWeek.Monday, " ", 20));

        Assert.Contains("must not be empty", ex.Message);
    }

    [Fact]
    public void Create_NonPositiveDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PrepTask.Create(DayOfWeek.Monday, "Chop vegetables", 0));
    }
}
