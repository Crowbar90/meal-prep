namespace MealPrepPlanner.Domain.MealPlanning;

using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;

/// <summary>
/// A single meal position within a meal plan. A <see cref="MealSlot"/> is a child
/// entity of the <see cref="MealPlan"/> aggregate and is only mutated through it.
/// </summary>
public class MealSlot : Entity
{
    private MealSlot()
    {
    }

    internal MealSlot(
        Guid id,
        DayOfWeek dayOfWeek,
        MealType mealType,
        int servings,
        bool isPrepMeal,
        string? prepNotes)
        : base(id)
    {
        DayOfWeek = dayOfWeek;
        MealType = mealType;
        Servings = servings;
        IsPrepMeal = isPrepMeal;
        PrepNotes = prepNotes;
    }

    public DayOfWeek DayOfWeek { get; }

    public MealType MealType { get; }

    public Guid? RecipeId { get; private set; }

    public string? RecipeName { get; private set; }

    public int Servings { get; private set; }

    public bool IsPrepMeal { get; private set; }

    public string? PrepNotes { get; }

    /// <summary>
    /// Transient reference to the assigned recipe. Not part of the persisted shape;
    /// used by domain services (nutrition, conflicts, shopping) while a draft is in flight.
    /// </summary>
    public Recipe? Recipe { get; private set; }

    internal void AssignRecipe(Recipe recipe, int servings)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (servings <= 0)
            throw new ArgumentOutOfRangeException(nameof(servings), "Servings must be a positive number.");

        Recipe = recipe;
        RecipeId = recipe.Id;
        RecipeName = recipe.Name;
        Servings = servings;
    }
}
