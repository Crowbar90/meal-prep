namespace MealPrepPlanner.Domain.Pantry.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PantryItemReserved : DomainEvent
{
    public PantryItemReserved(Guid pantryItemId, Guid correlationId)
        : base(correlationId)
    {
        PantryItemId = pantryItemId;
    }

    public Guid PantryItemId { get; }
}
