namespace MealPrepPlanner.Domain.MealPrep.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PrepScheduleGenerated : DomainEvent
{
    public PrepScheduleGenerated(Guid prepScheduleId, Guid mealPlanId, Guid correlationId)
        : base(correlationId)
    {
        PrepScheduleId = prepScheduleId;
        MealPlanId = mealPlanId;
    }

    public Guid PrepScheduleId { get; }

    public Guid MealPlanId { get; }
}
