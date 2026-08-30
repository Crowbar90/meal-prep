namespace MealPrepPlanner.Tests.Unit.MealPrep;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.MealPrep;
using MealPrepPlanner.Domain.Shared;

public class PrepTaskTests
{
    private static readonly Guid RecipeId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void Create_SetsAllFields()
    {
        var slotA = Guid.NewGuid();
        var slotB = Guid.NewGuid();
        var steps = new List<PrepStepDocument>
        {
            new(1, "Dice onions", DurationMinutes: 5),
            new(2, "Bake at 180C", DurationMinutes: 25, EquipmentId: "oven")
        };

        var task = PrepTask.Create(
            RecipeId,
            DayOfWeek.Sunday,
            MealType.Dinner,
            batchSizeServings: 4,
            earliestStartOffsetMinutes: 60,
            latestFinishOffsetMinutes: 180,
            equipmentIds: ["oven", "knife"],
            steps: steps,
            assignedToSlotIds: [slotA, slotB]);

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal(RecipeId, task.RecipeId);
        Assert.Equal(DayOfWeek.Sunday, task.DayOfWeek);
        Assert.Equal(MealType.Dinner, task.MealType);
        Assert.Equal(4, task.BatchSizeServings);
        Assert.Equal(60, task.EarliestStartOffsetMinutes);
        Assert.Equal(180, task.LatestFinishOffsetMinutes);
        Assert.Equal(["oven", "knife"], task.EquipmentIds);
        Assert.Equal(2, task.Steps.Count);
        Assert.Equal([slotA, slotB], task.AssignedToSlotIds);
        Assert.Equal(new TimeWindow(60, 180).EarliestStartOffsetMinutes, task.Window.EarliestStartOffsetMinutes);
    }

    [Fact]
    public void Create_DefaultsCollectionsToEmpty()
    {
        var task = PrepTask.Create(
            RecipeId,
            DayOfWeek.Monday,
            MealType.Lunch,
            batchSizeServings: 2,
            earliestStartOffsetMinutes: 0,
            latestFinishOffsetMinutes: 30);

        Assert.Empty(task.EquipmentIds);
        Assert.Empty(task.Steps);
        Assert.Empty(task.AssignedToSlotIds);
    }

    [Fact]
    public void Create_EmptyRecipeId_Throws()
    {
        Assert.Throws<ArgumentException>(() => PrepTask.Create(
            Guid.Empty,
            DayOfWeek.Monday,
            MealType.Lunch,
            batchSizeServings: 2,
            earliestStartOffsetMinutes: 0,
            latestFinishOffsetMinutes: 30));
    }

    [Fact]
    public void Create_NonPositiveBatchSize_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrepTask.Create(
            RecipeId,
            DayOfWeek.Monday,
            MealType.Lunch,
            batchSizeServings: 0,
            earliestStartOffsetMinutes: 0,
            latestFinishOffsetMinutes: 30));
    }

    [Fact]
    public void Create_NegativeEarliestStart_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrepTask.Create(
            RecipeId,
            DayOfWeek.Monday,
            MealType.Lunch,
            batchSizeServings: 2,
            earliestStartOffsetMinutes: -1,
            latestFinishOffsetMinutes: 30));
    }

    [Fact]
    public void Create_LatestFinishBeforeEarliestStart_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PrepTask.Create(
            RecipeId,
            DayOfWeek.Monday,
            MealType.Lunch,
            batchSizeServings: 2,
            earliestStartOffsetMinutes: 60,
            latestFinishOffsetMinutes: 30));
    }

    [Fact]
    public void Create_BlankEquipmentId_IsDropped()
    {
        var task = PrepTask.Create(
            RecipeId,
            DayOfWeek.Monday,
            MealType.Lunch,
            batchSizeServings: 2,
            earliestStartOffsetMinutes: 0,
            latestFinishOffsetMinutes: 30,
            equipmentIds: ["oven", "", "   ", "knife"]);

        Assert.Equal(["oven", "knife"], task.EquipmentIds);
    }
}
