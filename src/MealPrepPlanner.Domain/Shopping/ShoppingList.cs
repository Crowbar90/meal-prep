namespace MealPrepPlanner.Domain.Shopping;

using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Aggregate root for a shopping list generated from a finalized meal plan.
/// </summary>
public class ShoppingList : Entity
{
    private readonly List<ShoppingListItem> _items = [];

    private ShoppingList()
    {
    }

    private ShoppingList(Guid id, Guid mealPlanId, Guid? supermarketId)
        : base(id)
    {
        MealPlanId = mealPlanId;
        SupermarketId = supermarketId;
    }

    public Guid MealPlanId { get; }

    public Guid? SupermarketId { get; }

    public IReadOnlyList<ShoppingListItem> Items => _items;

    public Money TotalCost
    {
        get
        {
            var priced = _items
                .Select(i => i.EstimatedPrice)
                .Where(p => p is not null)
                .Cast<Money>()
                .ToArray();

            if (priced.Length == 0)
                return Money.Zero("EUR");

            return priced.Aggregate((sum, next) => sum + next).Round();
        }
    }

    public static ShoppingList Create(
        Guid mealPlanId,
        Guid? supermarketId = null,
        IReadOnlyList<ShoppingListItem>? items = null)
    {
        if (mealPlanId == Guid.Empty)
            throw new ArgumentException("Meal plan id must not be empty.", nameof(mealPlanId));

        var list = new ShoppingList(Guid.NewGuid(), mealPlanId, supermarketId);
        list._items.AddRange(items ?? []);
        return list;
    }
}
