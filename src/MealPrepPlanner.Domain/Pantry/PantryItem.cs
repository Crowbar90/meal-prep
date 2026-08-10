namespace MealPrepPlanner.Domain.Pantry;

using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Aggregate root tracking a single ingredient in a household's pantry inventory.
/// </summary>
public class PantryItem : Entity
{
    private readonly List<DomainEvent> _domainEvents = [];

    private PantryItem()
    {
    }

    private PantryItem(
        Guid id,
        Guid householdId,
        Guid ingredientId,
        Quantity quantity,
        DateOnly dateAdded,
        DateOnly? expiresAt)
        : base(id)
    {
        HouseholdId = householdId;
        IngredientId = ingredientId;
        Quantity = quantity;
        DateAdded = dateAdded;
        ExpiresAt = expiresAt;
        Status = PantryItemStatus.Available;
    }

    public Guid HouseholdId { get; }

    public Guid IngredientId { get; }

    public Quantity Quantity { get; }

    public DateOnly DateAdded { get; }

    public DateOnly? ExpiresAt { get; }

    public PantryItemStatus Status { get; private set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    public static PantryItem Add(
        Guid householdId,
        Guid ingredientId,
        Quantity quantity,
        DateOnly? expiresAt = null,
        Guid correlationId = default)
    {
        if (householdId == Guid.Empty)
            throw new ArgumentException("Household id must not be empty.", nameof(householdId));

        if (ingredientId == Guid.Empty)
            throw new ArgumentException("Ingredient id must not be empty.", nameof(ingredientId));

        if (quantity.Amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Pantry quantity must be positive.");

        if (string.IsNullOrWhiteSpace(quantity.Unit))
            throw new ArgumentException("Pantry quantity unit must not be empty.", nameof(quantity));

        var item = new PantryItem(
            Guid.NewGuid(),
            householdId,
            ingredientId,
            quantity,
            DateOnly.FromDateTime(DateTime.UtcNow),
            expiresAt);

        item._domainEvents.Add(new Events.PantryItemAdded(item.Id, householdId, ingredientId, quantity, expiresAt, correlationId));
        return item;
    }

    public void Reserve(Guid correlationId = default)
    {
        EnsureStatus(PantryItemStatus.Available);
        Status = PantryItemStatus.Reserved;
        _domainEvents.Add(new Events.PantryItemReserved(Id, correlationId));
    }

    public void Consume(Guid correlationId = default)
    {
        EnsureStatus(PantryItemStatus.Available, PantryItemStatus.Reserved);
        Status = PantryItemStatus.Consumed;
        _domainEvents.Add(new Events.PantryItemConsumed(Id, correlationId));
    }

    public void MarkExpired(Guid correlationId = default)
    {
        EnsureStatus(PantryItemStatus.Available, PantryItemStatus.Reserved);
        Status = PantryItemStatus.Expired;
        _domainEvents.Add(new Events.PantryItemExpired(Id, correlationId));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatus(params PantryItemStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new InvalidOperationException(
                $"Invalid status transition. Current status is '{Status}'; allowed statuses: {string.Join(", ", allowed)}.");
        }
    }
}
