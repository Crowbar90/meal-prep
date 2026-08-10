namespace MealPrepPlanner.Tests.Unit.MealPrep;

using MealPrepPlanner.Domain.MealPrep;
using MealPrepPlanner.Domain.MealPrep.Events;
using MealPrepPlanner.Domain.UserPreferences;

public class PrepScheduleTests
{
    [Fact]
    public void Create_EmitsGeneratedEvent()
    {
        var mealPlanId = Guid.NewGuid();
        var correlationId = Guid.NewGuid();

        var schedule = PrepSchedule.Create(mealPlanId, correlationId: correlationId);

        Assert.Equal(mealPlanId, schedule.MealPlanId);
        Assert.Empty(schedule.Tasks);
        Assert.Equal(0, schedule.TotalPrepTimeMinutes);
        var generated = Assert.Single(schedule.DomainEvents.OfType<PrepScheduleGenerated>());
        Assert.Equal(schedule.Id, generated.PrepScheduleId);
        Assert.Equal(mealPlanId, generated.MealPlanId);
        Assert.Equal(correlationId, generated.CorrelationId);
    }

    [Fact]
    public void Create_EmptyMealPlanId_Throws()
    {
        Assert.Throws<ArgumentException>(() => PrepSchedule.Create(Guid.Empty));
    }

    [Fact]
    public void TotalPrepTimeMinutes_SumsTaskDurations()
    {
        var schedule = PrepSchedule.Create(Guid.NewGuid());
        schedule.AddTask(PrepTask.Create(DayOfWeek.Sunday, "Batch cook chicken", 90));
        schedule.AddTask(PrepTask.Create(DayOfWeek.Wednesday, "Chop vegetables", 20));

        Assert.Equal(2, schedule.Tasks.Count);
        Assert.Equal(110, schedule.TotalPrepTimeMinutes);
    }

    [Fact]
    public void ClearDomainEvents_EmptiesEventList()
    {
        var schedule = PrepSchedule.Create(Guid.NewGuid());

        schedule.ClearDomainEvents();

        Assert.Empty(schedule.DomainEvents);
    }
}
