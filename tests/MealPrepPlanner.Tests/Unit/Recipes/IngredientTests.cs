namespace MealPrepPlanner.Tests.Unit.Recipes;

using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;

public class IngredientTests
{
    private static readonly NutritionProfile DefaultNutrition = new(100m, 10m, 20m, 5m);

    [Fact]
    public void Create_EmptyName_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => Ingredient.Create(" ", DefaultNutrition));

        Assert.Contains("must not be empty", ex.Message);
    }

    [Fact]
    public void Create_EmptyDefaultUnit_Throws()
    {
        Assert.Throws<ArgumentException>(
            () => Ingredient.Create("Chicken", DefaultNutrition, defaultUnit: " "));
    }

    [Fact]
    public void Create_CopiesAllergensAndAssignsId()
    {
        var ingredient = Ingredient.Create("Chicken", DefaultNutrition, allergens: ["nuts", "peanuts"]);

        Assert.Equal(["nuts", "peanuts"], ingredient.Allergens);
        Assert.NotEqual(Guid.Empty, ingredient.Id);
    }
}
