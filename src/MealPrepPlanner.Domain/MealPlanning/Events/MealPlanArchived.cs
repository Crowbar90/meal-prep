namespace MealPrepPlanner.Domain.MealPlanning.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class MealPlanArchived : DomainEvent
{
    public MealPlanArchived(Guid mealPlanId, Guid correlationId)
        : base(correlationId)
    {
        MealPlanId = mealPlanId;
    }

    public Guid MealPlanId { get; }
}
