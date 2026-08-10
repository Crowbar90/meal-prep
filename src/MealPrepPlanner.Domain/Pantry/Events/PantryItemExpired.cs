namespace MealPrepPlanner.Domain.Pantry.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PantryItemExpired : DomainEvent
{
    public PantryItemExpired(Guid pantryItemId, Guid correlationId)
        : base(correlationId)
    {
        PantryItemId = pantryItemId;
    }

    public Guid PantryItemId { get; }
}
