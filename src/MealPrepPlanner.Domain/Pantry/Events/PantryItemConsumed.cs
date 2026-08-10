namespace MealPrepPlanner.Domain.Pantry.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PantryItemConsumed : DomainEvent
{
    public PantryItemConsumed(Guid pantryItemId, Guid correlationId)
        : base(correlationId)
    {
        PantryItemId = pantryItemId;
    }

    public Guid PantryItemId { get; }
}
