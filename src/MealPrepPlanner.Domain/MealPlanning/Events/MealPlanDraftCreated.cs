namespace MealPrepPlanner.Domain.MealPlanning.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class MealPlanDraftCreated : DomainEvent
{
    public MealPlanDraftCreated(
        Guid mealPlanId,
        Guid householdId,
        DateOnly weekStartDate,
        Guid correlationId)
        : base(correlationId)
    {
        MealPlanId = mealPlanId;
        HouseholdId = householdId;
        WeekStartDate = weekStartDate;
    }

    public Guid MealPlanId { get; }

    public Guid HouseholdId { get; }

    public DateOnly WeekStartDate { get; }
}
