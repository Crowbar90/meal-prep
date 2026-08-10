namespace MealPrepPlanner.Tests.Unit.Shopping;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.Pantry;
using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;
using MealPrepPlanner.Domain.Shopping.Services;

public class ShoppingListGeneratorTests
{
    [Fact]
    public void Generate_AggregatesSameIngredientAcrossSlots()
    {
        var (recipe, _) = CreateRecipeWith("Chicken", new Quantity(200m, "g"));
        var plan = CreatePlan(
            (DayOfWeek.Monday, MealType.Dinner, recipe),
            (DayOfWeek.Tuesday, MealType.Lunch, recipe));

        var list = new ShoppingListGenerator().Generate(plan, []);

        var item = Assert.Single(list.Items);
        Assert.Equal("Chicken", item.IngredientName);
        Assert.Equal(new Quantity(400m, "g"), item.QuantityNeeded);
        Assert.Equal(new Quantity(400m, "g"), item.QuantityToBuy);
    }

    [Fact]
    public void Generate_SortsItemsByName()
    {
        var (recipe, _) = CreateRecipeWith(["Rice", "Chicken"]);
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));

        var list = new ShoppingListGenerator().Generate(plan, []);

        Assert.Equal(2, list.Items.Count);
        Assert.Equal("Chicken", list.Items[0].IngredientName);
        Assert.Equal("Rice", list.Items[1].IngredientName);
    }

    [Fact]
    public void Generate_ExcludesOptionalIngredients()
    {
        var (recipe, _) = CreateRecipeWith("Bacon", new Quantity(500m, "g"), isOptional: true);
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));

        var list = new ShoppingListGenerator().Generate(plan, []);

        Assert.Empty(list.Items);
    }

    [Fact]
    public void Generate_SubtractsAvailablePantryQuantity()
    {
        var (recipe, ingredientId) = CreateRecipeWith("Chicken", new Quantity(400m, "g"));
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));
        var pantryItem = PantryItem.Add(Guid.NewGuid(), ingredientId, new Quantity(300m, "g"));

        var list = new ShoppingListGenerator().Generate(plan, [pantryItem]);

        var item = Assert.Single(list.Items);
        Assert.Equal(new Quantity(400m, "g"), item.QuantityNeeded);
        Assert.Equal(new Quantity(100m, "g"), item.QuantityToBuy);
        Assert.True(item.PantryHas);
        Assert.Equal("Already in pantry", item.Notes);
    }

    [Fact]
    public void Generate_FullyCoveredByPantry_HasZeroToBuy()
    {
        var (recipe, ingredientId) = CreateRecipeWith("Chicken", new Quantity(200m, "g"));
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));
        var pantryItem = PantryItem.Add(Guid.NewGuid(), ingredientId, new Quantity(500m, "g"));

        var list = new ShoppingListGenerator().Generate(plan, [pantryItem]);

        var item = Assert.Single(list.Items);
        Assert.Equal(Quantity.Zero("g"), item.QuantityToBuy);
        Assert.True(item.PantryHas);
    }

    [Fact]
    public void Generate_RoundsGramsUpToHundredGrams()
    {
        var (recipe, _) = CreateRecipeWith("Chicken", new Quantity(250m, "g"));
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));

        var list = new ShoppingListGenerator().Generate(plan, []);

        var item = Assert.Single(list.Items);
        Assert.Equal(new Quantity(250m, "g"), item.QuantityNeeded);
        Assert.Equal(new Quantity(300m, "g"), item.QuantityToBuy);
        Assert.False(item.PantryHas);
        Assert.Null(item.Notes);
    }

    [Fact]
    public void Generate_RoundsNonGramUnitsToWholeUnits()
    {
        var (recipe, _) = CreateRecipeWith("Eggs", new Quantity(2.5m, "each"));
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));

        var list = new ShoppingListGenerator().Generate(plan, []);

        var item = Assert.Single(list.Items);
        Assert.Equal(new Quantity(3m, "each"), item.QuantityToBuy);
    }

    [Fact]
    public void Generate_NonAvailablePantryItemsAreIgnored()
    {
        var (recipe, ingredientId) = CreateRecipeWith("Chicken", new Quantity(200m, "g"));
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));
        var pantryItem = PantryItem.Add(Guid.NewGuid(), ingredientId, new Quantity(500m, "g"));
        pantryItem.Consume();

        var list = new ShoppingListGenerator().Generate(plan, [pantryItem]);

        var item = Assert.Single(list.Items);
        Assert.Equal(new Quantity(200m, "g"), item.QuantityToBuy);
        Assert.False(item.PantryHas);
    }

    [Fact]
    public void Generate_SameIngredientInDifferentRecipeUnits_Throws()
    {
        var chicken = CreateIngredient("Chicken");
        var gramsRecipe = RecipeWith(chicken, new Quantity(200m, "g"));
        var kilogramsRecipe = RecipeWith(chicken, new Quantity(1m, "kg"));
        var plan = CreatePlan(
            (DayOfWeek.Monday, MealType.Dinner, gramsRecipe),
            (DayOfWeek.Tuesday, MealType.Lunch, kilogramsRecipe));

        var ex = Assert.Throws<InvalidOperationException>(
            () => new ShoppingListGenerator().Generate(plan, []));

        Assert.Contains("multiple units", ex.Message);
    }

    [Fact]
    public void Generate_PantryUnitMismatch_Throws()
    {
        var (recipe, ingredientId) = CreateRecipeWith("Chicken", new Quantity(200m, "g"));
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));
        var pantryItem = PantryItem.Add(Guid.NewGuid(), ingredientId, new Quantity(1m, "kg"));

        var ex = Assert.Throws<InvalidOperationException>(
            () => new ShoppingListGenerator().Generate(plan, [pantryItem]));

        Assert.Contains("does not match", ex.Message);
    }

    [Fact]
    public void Generate_PantryItemsInMultipleUnits_Throws()
    {
        var (recipe, ingredientId) = CreateRecipeWith("Chicken", new Quantity(200m, "g"));
        var plan = CreatePlan((DayOfWeek.Monday, MealType.Dinner, recipe));
        var pantryItems = new[]
        {
            PantryItem.Add(Guid.NewGuid(), ingredientId, new Quantity(200m, "g")),
            PantryItem.Add(Guid.NewGuid(), ingredientId, new Quantity(1m, "kg"))
        };

        var ex = Assert.Throws<InvalidOperationException>(
            () => new ShoppingListGenerator().Generate(plan, pantryItems));

        Assert.Contains("multiple units", ex.Message);
    }

    [Fact]
    public void Generate_UnassignedSlot_Throws()
    {
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), new DateOnly(2026, 8, 10));
        plan.AddSlot(DayOfWeek.Monday, MealType.Dinner, 4);

        var ex = Assert.Throws<InvalidOperationException>(
            () => new ShoppingListGenerator().Generate(plan, []));

        Assert.Contains("no recipe", ex.Message);
    }

    [Fact]
    public void Generate_NullArguments_Throw()
    {
        var generator = new ShoppingListGenerator();

        Assert.Throws<ArgumentNullException>(() => generator.Generate(null!, []));
        Assert.Throws<ArgumentNullException>(
            () => generator.Generate(MealPlan.CreateDraft(Guid.NewGuid(), new DateOnly(2026, 8, 10)), null!));
    }

    private static Ingredient CreateIngredient(string name) =>
        Ingredient.Create(name, new NutritionProfile(100m, 10m, 5m, 5m));

    private static Recipe RecipeWith(Ingredient ingredient, Quantity quantity, bool isOptional = false)
    {
        var recipe = Recipe.Create("Dish", null, ["Step 1"], 10, 20, 4);
        recipe.AddIngredient(ingredient, quantity, isOptional);
        return recipe;
    }

    private static (Recipe Recipe, Guid IngredientId) CreateRecipeWith(
        string ingredientName,
        Quantity quantity,
        bool isOptional = false)
    {
        var ingredient = CreateIngredient(ingredientName);
        return (RecipeWith(ingredient, quantity, isOptional), ingredient.Id);
    }

    private static (Recipe Recipe, Guid IngredientId) CreateRecipeWith(IReadOnlyList<string> ingredientNames)
    {
        var recipe = Recipe.Create("Dish", null, ["Step 1"], 10, 20, 4);
        var firstIngredientId = Guid.Empty;

        foreach (var name in ingredientNames)
        {
            var ingredient = CreateIngredient(name);
            recipe.AddIngredient(ingredient, new Quantity(100m, "g"));
            firstIngredientId = ingredient.Id;
        }

        return (recipe, firstIngredientId);
    }

    private static MealPlan CreatePlan(params (DayOfWeek Day, MealType Type, Recipe Recipe)[] slots)
    {
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), new DateOnly(2026, 8, 10));

        foreach (var (day, type, recipe) in slots)
        {
            var slot = plan.AddSlot(day, type, 4);
            plan.AssignRecipe(slot, recipe, 4);
        }

        return plan;
    }
}
