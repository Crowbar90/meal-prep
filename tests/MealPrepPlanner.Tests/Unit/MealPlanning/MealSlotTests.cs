namespace MealPrepPlanner.Tests.Unit.MealPlanning;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.Recipes;

public class MealSlotTests
{
    [Fact]
    public void AssignRecipe_SetsRecipeAndServings()
    {
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today));
        var slot = plan.AddSlot(DayOfWeek.Tuesday, MealType.Lunch, 2);
        var recipe = CreateRecipe();

        plan.AssignRecipe(slot, recipe, 4);

        Assert.Equal(recipe.Id, slot.RecipeId);
        Assert.Equal(recipe.Name, slot.RecipeName);
        Assert.Same(recipe, slot.Recipe);
        Assert.Equal(4, slot.Servings);
    }

    [Fact]
    public void AssignRecipe_ReplacesPreviousAssignment()
    {
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today));
        var slot = plan.AddSlot(DayOfWeek.Tuesday, MealType.Lunch, 2);
        var first = CreateRecipe();
        var second = CreateRecipe();

        plan.AssignRecipe(slot, first, 2);
        plan.AssignRecipe(slot, second, 3);

        Assert.Equal(second.Id, slot.RecipeId);
        Assert.Equal(second.Name, slot.RecipeName);
        Assert.Equal(3, slot.Servings);
    }

    [Fact]
    public void AssignRecipe_NonPositiveServings_Throws()
    {
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today));
        var slot = plan.AddSlot(DayOfWeek.Tuesday, MealType.Lunch, 2);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => plan.AssignRecipe(slot, CreateRecipe(), 0));
    }

    private static Recipe CreateRecipe() =>
        Recipe.Create("Stew", null, ["Step 1"], 20, 30, 4);
}
