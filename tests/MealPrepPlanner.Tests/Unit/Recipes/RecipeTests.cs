namespace MealPrepPlanner.Tests.Unit.Recipes;

using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;

public class RecipeTests
{
    [Fact]
    public void Create_EmptyName_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => Recipe.Create("  ", null, ["Step 1"], 5, 10, 2));

        Assert.Contains("must not be empty", ex.Message);
    }

    [Fact]
    public void Create_NullInstructions_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => Recipe.Create("Soup", null, null!, 5, 10, 2));
    }

    [Fact]
    public void Create_EmptyInstructions_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(
            () => Recipe.Create("Soup", null, [], 5, 10, 2));

        Assert.Contains("at least one instruction", ex.Message);
    }

    [Fact]
    public void Create_NegativePrepTime_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Recipe.Create("Soup", null, ["Step 1"], -1, 10, 2));
    }

    [Fact]
    public void Create_NonPositiveBaseServings_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => Recipe.Create("Soup", null, ["Step 1"], 5, 10, 0));
    }

    [Fact]
    public void TotalTime_SumsPrepAndCook()
    {
        var recipe = Recipe.Create("Soup", null, ["Step 1"], 15, 45, 2);

        Assert.Equal(TimeSpan.FromMinutes(60), recipe.TotalTime);
    }

    [Fact]
    public void AddIngredient_CapturesSnapshotOfIngredient()
    {
        var recipe = Recipe.Create("Soup", null, ["Step 1"], 5, 10, 2);
        var ingredient = Ingredient.Create(
            "Chicken",
            new NutritionProfile(200m, 25m, 0m, 10m),
            allergens: ["nuts"]);

        recipe.AddIngredient(ingredient, new Quantity(300m, "g"), isOptional: true, preparation: "diced");

        var snapshot = Assert.Single(recipe.Ingredients);
        Assert.Equal(ingredient.Id, snapshot.IngredientId);
        Assert.Equal(ingredient.Name, snapshot.Name);
        Assert.Equal(new Quantity(300m, "g"), snapshot.Quantity);
        Assert.Equal(ingredient.NutritionPer100g, snapshot.NutritionPer100g);
        Assert.Equal(["nuts"], snapshot.Allergens);
        Assert.True(snapshot.IsOptional);
        Assert.Equal("diced", snapshot.Preparation);
    }

    [Fact]
    public void AddIngredient_NonPositiveQuantity_Throws()
    {
        var recipe = Recipe.Create("Soup", null, ["Step 1"], 5, 10, 2);
        var ingredient = Ingredient.Create("Chicken", new NutritionProfile(200m, 25m, 0m, 10m));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => recipe.AddIngredient(ingredient, new Quantity(0m, "g")));
    }

    [Fact]
    public void AddIngredient_EmptyUnit_Throws()
    {
        var recipe = Recipe.Create("Soup", null, ["Step 1"], 5, 10, 2);
        var ingredient = Ingredient.Create("Chicken", new NutritionProfile(200m, 25m, 0m, 10m));

        Assert.Throws<ArgumentException>(
            () => recipe.AddIngredient(ingredient, new Quantity(100m, " ")));
    }
}
