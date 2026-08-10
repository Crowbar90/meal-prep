namespace MealPrepPlanner.Tests.Unit.Pantry;

using MealPrepPlanner.Domain.Pantry;
using MealPrepPlanner.Domain.Pantry.Events;
using MealPrepPlanner.Domain.Shared;

public class PantryItemTests
{
    private static readonly Guid HouseholdId = Guid.NewGuid();
    private static readonly Guid IngredientId = Guid.NewGuid();
    private static readonly DateOnly ExpiresAt = new(2026, 9, 1);

    [Fact]
    public void Add_SetsPropertiesAndEmitsEvent()
    {
        var correlationId = Guid.NewGuid();

        var item = PantryItem.Add(HouseholdId, IngredientId, new Quantity(500m, "g"), ExpiresAt, correlationId);

        Assert.Equal(HouseholdId, item.HouseholdId);
        Assert.Equal(IngredientId, item.IngredientId);
        Assert.Equal(new Quantity(500m, "g"), item.Quantity);
        Assert.Equal(ExpiresAt, item.ExpiresAt);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), item.DateAdded);
        Assert.Equal(PantryItemStatus.Available, item.Status);
        var added = Assert.Single(item.DomainEvents.OfType<PantryItemAdded>());
        Assert.Equal(item.Id, added.PantryItemId);
        Assert.Equal(HouseholdId, added.HouseholdId);
        Assert.Equal(IngredientId, added.IngredientId);
        Assert.Equal(new Quantity(500m, "g"), added.Quantity);
        Assert.Equal(ExpiresAt, added.ExpiresAt);
        Assert.Equal(correlationId, added.CorrelationId);
    }

    [Fact]
    public void Add_EmptyHouseholdId_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => PantryItem.Add(Guid.Empty, IngredientId, new Quantity(100m, "g")));
    }

    [Fact]
    public void Add_EmptyIngredientId_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => PantryItem.Add(HouseholdId, Guid.Empty, new Quantity(100m, "g")));
    }

    [Fact]
    public void Add_NonPositiveQuantity_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => PantryItem.Add(HouseholdId, IngredientId, new Quantity(0m, "g")));
    }

    [Fact]
    public void Add_EmptyUnit_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, " ")));
    }

    [Fact]
    public void Reserve_TransitionsAndEmitsEvent()
    {
        var item = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));
        var correlationId = Guid.NewGuid();

        item.Reserve(correlationId);

        Assert.Equal(PantryItemStatus.Reserved, item.Status);
        var reserved = Assert.Single(item.DomainEvents.OfType<PantryItemReserved>());
        Assert.Equal(item.Id, reserved.PantryItemId);
        Assert.Equal(correlationId, reserved.CorrelationId);
    }

    [Fact]
    public void Reserve_NonAvailableItem_Throws()
    {
        var item = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));
        item.Reserve();

        Assert.Throws<InvalidOperationException>(() => item.Reserve());
    }

    [Fact]
    public void Consume_FromAvailable_TransitionsToConsumed()
    {
        var item = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));

        item.Consume();

        Assert.Equal(PantryItemStatus.Consumed, item.Status);
        Assert.Single(item.DomainEvents.OfType<PantryItemConsumed>());
    }

    [Fact]
    public void Consume_FromReserved_TransitionsToConsumed()
    {
        var item = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));
        item.Reserve();

        item.Consume();

        Assert.Equal(PantryItemStatus.Consumed, item.Status);
    }

    [Fact]
    public void Consume_ConsumedItem_Throws()
    {
        var item = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));
        item.Consume();

        Assert.Throws<InvalidOperationException>(() => item.Consume());
    }

    [Fact]
    public void MarkExpired_FromAvailableAndReserved_TransitionsToExpired()
    {
        var available = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));
        available.MarkExpired();
        Assert.Equal(PantryItemStatus.Expired, available.Status);
        Assert.Single(available.DomainEvents.OfType<PantryItemExpired>());

        var reserved = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));
        reserved.Reserve();
        reserved.MarkExpired();
        Assert.Equal(PantryItemStatus.Expired, reserved.Status);
    }

    [Fact]
    public void MarkExpired_ExpiredItem_Throws()
    {
        var item = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));
        item.MarkExpired();

        Assert.Throws<InvalidOperationException>(() => item.MarkExpired());
    }

    [Fact]
    public void ClearDomainEvents_EmptiesEventList()
    {
        var item = PantryItem.Add(HouseholdId, IngredientId, new Quantity(100m, "g"));

        item.ClearDomainEvents();

        Assert.Empty(item.DomainEvents);
    }
}
