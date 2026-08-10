namespace MealPrepPlanner.Tests.Unit.Recipes;

using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Recipes.Services;
using MealPrepPlanner.Domain.Shared;
using MealPrepPlanner.Domain.UserPreferences;

public class RecipeScalerTests
{
    [Fact]
    public void Scale_MultipliesQuantitiesByTargetToBaseFactor()
    {
        var recipe = CreateRecipe(4, new Quantity(200m, "g"), new Quantity(100m, "g"));

        var scaled = new RecipeScaler().Scale(recipe, 6);

        Assert.Equal(new Quantity(300m, "g"), scaled.Ingredients[0].Quantity);
        Assert.Equal(new Quantity(150m, "g"), scaled.Ingredients[1].Quantity);
    }

    [Fact]
    public void Scale_FromSingleServing_TriplesQuantities()
    {
        var recipe = CreateRecipe(1, new Quantity(150m, "g"), new Quantity(50m, "g"));

        var scaled = new RecipeScaler().Scale(recipe, 3);

        Assert.Equal(new Quantity(450m, "g"), scaled.Ingredients[0].Quantity);
        Assert.Equal(new Quantity(150m, "g"), scaled.Ingredients[1].Quantity);
    }

    [Fact]
    public void Scale_RoundsQuantitiesToThreeDecimals()
    {
        var recipe = CreateRecipe(3, new Quantity(100m, "g"), new Quantity(200m, "g"));

        var scaled = new RecipeScaler().Scale(recipe, 4);

        Assert.Equal(new Quantity(133.333m, "g"), scaled.Ingredients[0].Quantity);
        Assert.Equal(new Quantity(266.667m, "g"), scaled.Ingredients[1].Quantity);
    }

    [Fact]
    public void Scale_UpdatesBaseServings()
    {
        var scaled = new RecipeScaler().Scale(CreateRecipe(4, new Quantity(100m, "g"), new Quantity(100m, "g")), 2);

        Assert.Equal(2, scaled.BaseServings);
    }

    [Fact]
    public void Scale_PreservesRecipeMetadata()
    {
        var recipe = CreateRecipe(4, new Quantity(100m, "g"), new Quantity(100m, "g"));

        var scaled = new RecipeScaler().Scale(recipe, 8);

        Assert.Equal(recipe.Name, scaled.Name);
        Assert.Equal(recipe.Description, scaled.Description);
        Assert.Equal(recipe.Instructions, scaled.Instructions);
        Assert.Equal(recipe.PrepTimeMinutes, scaled.PrepTimeMinutes);
        Assert.Equal(recipe.CookTimeMinutes, scaled.CookTimeMinutes);
        Assert.Equal(recipe.Tags, scaled.Tags);
        Assert.Equal(recipe.EquipmentNeeded, scaled.EquipmentNeeded);
        Assert.Equal(recipe.Source, scaled.Source);
        Assert.Equal(recipe.CreatedBy, scaled.CreatedBy);
    }

    [Fact]
    public void Scale_PreservesIngredientSnapshotFields()
    {
        var recipe = CreateRecipe(2, new Quantity(100m, "g"), new Quantity(50m, "g"));

        var scaled = new RecipeScaler().Scale(recipe, 4);
        var scaledIngredient = scaled.Ingredients[0];

        Assert.Equal(recipe.Ingredients[0].IngredientId, scaledIngredient.IngredientId);
        Assert.Equal(recipe.Ingredients[0].Name, scaledIngredient.Name);
        Assert.Equal(recipe.Ingredients[0].NutritionPer100g, scaledIngredient.NutritionPer100g);
        Assert.Equal(recipe.Ingredients[0].Allergens, scaledIngredient.Allergens);
        Assert.Equal(recipe.Ingredients[0].IsOptional, scaledIngredient.IsOptional);
        Assert.Equal(recipe.Ingredients[0].Preparation, scaledIngredient.Preparation);
    }

    [Fact]
    public void Scale_NonPositiveTargetServings_Throws()
    {
        var recipe = CreateRecipe(4, new Quantity(100m, "g"), new Quantity(100m, "g"));

        Assert.Throws<ArgumentOutOfRangeException>(() => new RecipeScaler().Scale(recipe, 0));
    }

    [Fact]
    public void Scale_NullRecipe_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new RecipeScaler().Scale(null!, 2));
    }

    private static Recipe CreateRecipe(int baseServings, params Quantity[] quantities)
    {
        var recipe = Recipe.Create(
            "Chicken Rice",
            "A simple dish",
            ["Season", "Cook"],
            10,
            20,
            baseServings,
            ["quick"],
            [new Equipment("oven")],
            "Test source",
            Guid.NewGuid());

        for (var index = 0; index < quantities.Length; index++)
        {
            var ingredient = Ingredient.Create(
                $"Ingredient {index}",
                new NutritionProfile(100m, 10m, 20m, 5m),
                allergens: ["nuts"]);
            recipe.AddIngredient(ingredient, quantities[index]);
        }

        return recipe;
    }
}
