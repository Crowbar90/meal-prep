namespace MealPrepPlanner.Domain.Pantry.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PantryItemAdded : DomainEvent
{
    public PantryItemAdded(
        Guid pantryItemId,
        Guid householdId,
        Guid ingredientId,
        Quantity quantity,
        DateOnly? expiresAt,
        Guid correlationId)
        : base(correlationId)
    {
        PantryItemId = pantryItemId;
        HouseholdId = householdId;
        IngredientId = ingredientId;
        Quantity = quantity;
        ExpiresAt = expiresAt;
    }

    public Guid PantryItemId { get; }

    public Guid HouseholdId { get; }

    public Guid IngredientId { get; }

    public Quantity Quantity { get; }

    public DateOnly? ExpiresAt { get; }
}
