namespace MealPrepPlanner.Tests.Unit.Shopping;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;
using MealPrepPlanner.Domain.Shopping;
using MealPrepPlanner.Domain.Shopping.Services;

public class ShoppingListTests
{
    [Fact]
    public void Create_EmptyMealPlanId_Throws()
    {
        Assert.Throws<ArgumentException>(() => ShoppingList.Create(Guid.Empty));
    }

    [Fact]
    public void Create_SetsMealPlanIdAndSupermarketId()
    {
        var mealPlanId = Guid.NewGuid();
        var supermarketId = Guid.NewGuid();

        var list = ShoppingList.Create(mealPlanId, supermarketId);

        Assert.Equal(mealPlanId, list.MealPlanId);
        Assert.Equal(supermarketId, list.SupermarketId);
        Assert.Empty(list.Items);
    }

    [Fact]
    public void TotalCost_WithUnpricedGeneratorItems_IsZeroEuros()
    {
        var recipe = Recipe.Create("Dish", null, ["Step 1"], 10, 20, 4);
        recipe.AddIngredient(
            Ingredient.Create("Chicken", new NutritionProfile(100m, 10m, 5m, 5m)),
            new Quantity(200m, "g"));
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), new DateOnly(2026, 8, 10));
        var slot = plan.AddSlot(DayOfWeek.Monday, MealType.Dinner, 4);
        plan.AssignRecipe(slot, recipe, 4);

        var list = new ShoppingListGenerator().Generate(plan, []);

        Assert.Equal(Money.Zero("EUR"), list.TotalCost);
    }
}
