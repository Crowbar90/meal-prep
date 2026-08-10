namespace MealPrepPlanner.Domain.Shopping.Services;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.Pantry;
using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Builds a shopping list from a meal plan by aggregating ingredient quantities
/// across all slots and subtracting what is already available in the pantry.
///
/// Assumptions:
/// - Only slots with an assigned recipe contribute.
/// - Quantities are aggregated in a single unit per ingredient; mixed units are rejected.
/// - To-buy quantities are rounded up to a realistic package step (100g for gram units, else 1 unit).
/// - The currency defaults to EUR when no prices are known.
/// </summary>
public sealed class ShoppingListGenerator
{
    public ShoppingList Generate(MealPlan plan, IReadOnlyList<PantryItem> pantryItems)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(pantryItems);

        var neededByIngredient = AggregateNeeded(plan);
        var pantryByIngredient = AggregatePantry(pantryItems);

        var items = new List<ShoppingListItem>();
        foreach (var (ingredientId, (name, needed)) in neededByIngredient.OrderBy(k => k.Value.Name, StringComparer.OrdinalIgnoreCase))
        {
            var inPantry = pantryByIngredient.GetValueOrDefault(ingredientId);
            if (!string.IsNullOrEmpty(inPantry.Unit)
                && !string.Equals(inPantry.Unit, needed.Unit, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(
                $"Pantry unit '{inPantry.Unit}' does not match recipe unit '{needed.Unit}' for ingredient '{name}'.");

            var shortfall = needed.Amount - inPantry.Amount;
            var toBuy = shortfall > 0 ? RoundUpToPackage(new Quantity(shortfall, needed.Unit)) : Quantity.Zero(needed.Unit);

            items.Add(new ShoppingListItem(
                Guid.NewGuid(),
                ingredientId,
                name,
                needed,
                toBuy,
                null,
                inPantry.Amount > 0,
                inPantry.Amount > 0 ? "Already in pantry" : null));
        }

        return ShoppingList.Create(plan.Id, items: items);
    }

    private static SortedDictionary<Guid, (string Name, Quantity Quantity)> AggregateNeeded(MealPlan plan)
    {
        var result = new SortedDictionary<Guid, (string Name, Quantity Quantity)>();

        foreach (var slot in plan.Slots)
        {
            var recipe = slot.Recipe ?? throw new InvalidOperationException(
                $"Slot '{slot.MealType} {slot.DayOfWeek}' has no recipe assigned; cannot generate a shopping list from an incomplete plan.");

            foreach (var ingredient in recipe.Ingredients)
            {
                if (ingredient.IsOptional)
                    continue;

                if (result.TryGetValue(ingredient.IngredientId, out var existing))
                {
                    if (!string.Equals(existing.Quantity.Unit, ingredient.Quantity.Unit, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException(
                        $"Ingredient '{ingredient.Name}' appears in multiple units across the plan.");

                    result[ingredient.IngredientId] = (existing.Name, existing.Quantity + ingredient.Quantity);
                }
                else
                    result[ingredient.IngredientId] = (ingredient.Name, ingredient.Quantity);
            }
        }

        return result;
    }

    private static Dictionary<Guid, Quantity> AggregatePantry(IReadOnlyList<PantryItem> pantryItems)
    {
        var result = new Dictionary<Guid, Quantity>();

        foreach (var item in pantryItems)
        {
            if (item.Status != PantryItemStatus.Available)
                continue;

            if (result.TryGetValue(item.IngredientId, out var existing))
            {
                if (!string.Equals(existing.Unit, item.Quantity.Unit, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(
                    $"Pantry contains ingredient '{item.IngredientId}' in multiple units.");

                result[item.IngredientId] = existing + item.Quantity;
            }
            else
                result[item.IngredientId] = item.Quantity;
        }

        return result;
    }

    private static Quantity RoundUpToPackage(Quantity quantity)
    {
        if (quantity.Amount <= 0)
            return Quantity.Zero(quantity.Unit);

        var step = string.Equals(quantity.Unit, "g", StringComparison.OrdinalIgnoreCase) ? 100m : 1m;
        var rounded = Math.Ceiling(quantity.Amount / step) * step;
        return new Quantity(rounded, quantity.Unit);
    }
}
