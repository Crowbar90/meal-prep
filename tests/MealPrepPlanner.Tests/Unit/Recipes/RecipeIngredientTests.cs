namespace MealPrepPlanner.Tests.Unit.Recipes;

using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;

public class RecipeIngredientTests
{
    private static RecipeIngredient Create(int amountGrams = 100) =>
        new(
            Guid.NewGuid(),
            "Chicken",
            new Quantity(amountGrams, "g"),
            new NutritionProfile(200m, 25m, 0m, 10m),
            ["nuts"],
            false,
            "cubed");

    [Fact]
    public void Scale_MultipliesQuantityAndRoundsToThreeDecimals()
    {
        var scaled = Create(100).Scale(1m / 3m);

        Assert.Equal(new Quantity(33.333m, "g"), scaled.Quantity);
    }

    [Fact]
    public void Scale_PreservesSnapshotFields()
    {
        var original = Create(200);

        var scaled = original.Scale(2m);

        Assert.Equal(original.IngredientId, scaled.IngredientId);
        Assert.Equal(original.Name, scaled.Name);
        Assert.Equal(original.NutritionPer100g, scaled.NutritionPer100g);
        Assert.Equal(original.Allergens, scaled.Allergens);
        Assert.Equal(original.IsOptional, scaled.IsOptional);
        Assert.Equal(original.Preparation, scaled.Preparation);
    }
}
