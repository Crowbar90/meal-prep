namespace MealPrepPlanner.Tests.Unit.Nutrition;

using MealPrepPlanner.Domain.Nutrition;
using MealPrepPlanner.Domain.Nutrition.Services;
using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;

public class NutritionCalculatorTests
{
    [Fact]
    public void Calculate_ComputesPerServingNutrition()
    {
        var recipe = CreateRecipe(
            ("Chicken", new NutritionProfile(100m, 10m, 20m, 5m), 200m),
            ("Rice", new NutritionProfile(50m, 5m, 10m, 2m), 100m));

        var profile = new NutritionCalculator().Calculate(recipe, servings: 4);

        Assert.Equal(new NutritionProfile(62.5m, 6.3m, 12.5m, 3m), profile);
    }

    [Fact]
    public void Calculate_SmallerServings_DividesTotalAccordingly()
    {
        var recipe = CreateRecipe(("Chicken", new NutritionProfile(100m, 10m, 20m, 5m), 200m));

        var profile = new NutritionCalculator().Calculate(recipe, servings: 2);

        Assert.Equal(new NutritionProfile(100m, 10m, 20m, 5m), profile);
    }

    [Fact]
    public void Calculate_ExcludesOptionalIngredients()
    {
        var recipe = Recipe.Create("Dish", null, ["Step 1"], 5, 10, 4);
        recipe.AddIngredient(
            Ingredient.Create("Chicken", new NutritionProfile(100m, 10m, 20m, 5m)),
            new Quantity(200m, "g"));
        recipe.AddIngredient(
            Ingredient.Create("Bacon", new NutritionProfile(500m, 25m, 0m, 50m)),
            new Quantity(1000m, "g"),
            isOptional: true);

        var profile = new NutritionCalculator().Calculate(recipe, servings: 4);

        Assert.Equal(50m, profile.Calories);
    }

    [Fact]
    public void Calculate_NonGramUnit_Throws()
    {
        var recipe = Recipe.Create("Dish", null, ["Step 1"], 5, 10, 4);
        recipe.AddIngredient(
            Ingredient.Create("Chicken", new NutritionProfile(100m, 10m, 20m, 5m)),
            new Quantity(1m, "kg"));

        var ex = Assert.Throws<NotSupportedException>(
            () => new NutritionCalculator().Calculate(recipe, servings: 4));

        Assert.Contains("gram quantities", ex.Message);
    }

    [Fact]
    public void Calculate_NonPositiveServings_Throws()
    {
        var recipe = CreateRecipe(("Chicken", new NutritionProfile(100m, 10m, 20m, 5m), 200m));

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new NutritionCalculator().Calculate(recipe, servings: 0));
    }

    [Fact]
    public void Calculate_NullRecipe_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new NutritionCalculator().Calculate(null!, servings: 2));
    }

    [Fact]
    public void ValidateAgainstGoals_WithinAllTolerances_ReturnsTrue()
    {
        var goals = new NutritionalGoals(2000m, 100m, 250m, 70m);
        var daily = new NutritionProfile(2100m, 110m, 275m, 77m);

        var valid = new NutritionCalculator().ValidateAgainstGoals(daily, goals, out var warnings);

        Assert.True(valid);
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateAgainstGoals_CaloriesOverFivePercent_ReturnsFalse()
    {
        var goals = new NutritionalGoals(2000m, 100m, 250m, 70m);

        var valid = new NutritionCalculator().ValidateAgainstGoals(
            new NutritionProfile(2200m, 100m, 250m, 70m),
            goals,
            out var warnings);

        Assert.False(valid);
        var calorieWarning = Assert.Single(warnings);
        Assert.Contains("Calories", calorieWarning);
        Assert.Contains("over", calorieWarning);
    }

    [Fact]
    public void ValidateAgainstGoals_CaloriesUnderFivePercent_ReturnsFalse()
    {
        var goals = new NutritionalGoals(2000m, 100m, 250m, 70m);

        var valid = new NutritionCalculator().ValidateAgainstGoals(
            new NutritionProfile(1800m, 100m, 250m, 70m),
            goals,
            out var warnings);

        Assert.False(valid);
        Assert.Contains("under", Assert.Single(warnings));
    }

    [Fact]
    public void ValidateAgainstGoals_MacroOverTenPercent_ReturnsFalse()
    {
        var goals = new NutritionalGoals(2000m, 100m, 250m, 70m);

        var valid = new NutritionCalculator().ValidateAgainstGoals(
            new NutritionProfile(2000m, 100m, 250m, 78m),
            goals,
            out _);

        Assert.False(valid);
    }

    [Fact]
    public void ValidateAgainstGoals_ExactlyAtTolerance_ReturnsTrue()
    {
        var goals = new NutritionalGoals(2000m, 100m, 250m, 70m);
        var daily = new NutritionProfile(2100m, 110m, 225m, 63m);

        var valid = new NutritionCalculator().ValidateAgainstGoals(daily, goals, out var warnings);

        Assert.True(valid);
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateAgainstGoals_ZeroTargetsAreSkipped()
    {
        var goals = new NutritionalGoals(0m, 0m, 0m, 0m);

        var valid = new NutritionCalculator().ValidateAgainstGoals(
            new NutritionProfile(99999m, 999m, 999m, 999m),
            goals,
            out var warnings);

        Assert.True(valid);
        Assert.Empty(warnings);
    }

    [Fact]
    public void ValidateAgainstGoals_FiberAndSodiumAreCheckedWhenPresent()
    {
        var goals = new NutritionalGoals(2000m, 100m, 250m, 70m, fiberPerDay: 30m, sodiumMgPerDay: 2000m);

        var valid = new NutritionCalculator().ValidateAgainstGoals(
            new NutritionProfile(2000m, 100m, 250m, 70m, Fiber: 20m, SodiumMg: 1700m),
            goals,
            out var warnings);

        Assert.False(valid);
        Assert.Contains(warnings, w => w.Contains("Fiber"));
        Assert.Contains(warnings, w => w.Contains("Sodium"));
    }

    [Fact]
    public void ValidateAgainstGoals_GoalsWithoutOptionalTargets_SkipOptionalChecks()
    {
        var goals = new NutritionalGoals(2000m, 100m, 250m, 70m);
        var daily = new NutritionProfile(2000m, 100m, 250m, 70m, Fiber: 5m, SodiumMg: 5000m);

        var valid = new NutritionCalculator().ValidateAgainstGoals(daily, goals, out _);

        Assert.True(valid);
    }

    private static Recipe CreateRecipe(params (string Name, NutritionProfile Per100g, decimal Grams)[] ingredients)
    {
        var recipe = Recipe.Create("Dish", null, ["Step 1"], 5, 10, 4);

        foreach (var (name, per100g, grams) in ingredients)
        {
            recipe.AddIngredient(Ingredient.Create(name, per100g), new Quantity(grams, "g"));
        }

        return recipe;
    }
}
