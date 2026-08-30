namespace MealPrepPlanner.Domain.MealPrep.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PrepScheduleDraftCreated : DomainEvent
{
    public PrepScheduleDraftCreated(
        Guid prepScheduleId,
        Guid mealPlanId,
        Guid householdId,
        DateOnly weekStartDate,
        Guid correlationId)
        : base(correlationId)
    {
        PrepScheduleId = prepScheduleId;
        MealPlanId = mealPlanId;
        HouseholdId = householdId;
        WeekStartDate = weekStartDate;
    }

    public Guid PrepScheduleId { get; }

    public Guid MealPlanId { get; }

    public Guid HouseholdId { get; }

    public DateOnly WeekStartDate { get; }
}
