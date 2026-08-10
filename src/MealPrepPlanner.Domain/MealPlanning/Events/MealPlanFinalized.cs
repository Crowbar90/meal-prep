namespace MealPrepPlanner.Domain.MealPlanning.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class MealPlanFinalized : DomainEvent
{
    public MealPlanFinalized(
        Guid mealPlanId,
        Guid householdId,
        DateTimeOffset finalizedAt,
        Guid correlationId)
        : base(correlationId)
    {
        MealPlanId = mealPlanId;
        HouseholdId = householdId;
        FinalizedAt = finalizedAt;
    }

    public Guid MealPlanId { get; }

    public Guid HouseholdId { get; }

    public DateTimeOffset FinalizedAt { get; }
}
