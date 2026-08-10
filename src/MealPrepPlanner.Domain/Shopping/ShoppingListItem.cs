namespace MealPrepPlanner.Domain.Shopping;

using MealPrepPlanner.Domain.Shared;

/// <summary>
/// A line item on a shopping list: what the plan needs, what to actually buy
/// (rounded to package sizes, pantry subtracted), and pantry hints.
/// </summary>
public class ShoppingListItem : Entity
{
    private ShoppingListItem()
    {
        IngredientName = string.Empty;
    }

    internal ShoppingListItem(
        Guid id,
        Guid ingredientId,
        string ingredientName,
        Quantity quantityNeeded,
        Quantity quantityToBuy,
        Money? estimatedPrice,
        bool pantryHas,
        string? notes)
        : base(id)
    {
        IngredientId = ingredientId;
        IngredientName = ingredientName;
        QuantityNeeded = quantityNeeded;
        QuantityToBuy = quantityToBuy;
        EstimatedPrice = estimatedPrice;
        PantryHas = pantryHas;
        Notes = notes;
    }

    public Guid IngredientId { get; }

    public string IngredientName { get; }

    public Quantity QuantityNeeded { get; }

    public Quantity QuantityToBuy { get; }

    public Money? EstimatedPrice { get; }

    public bool PantryHas { get; }

    public string? Notes { get; }
}
