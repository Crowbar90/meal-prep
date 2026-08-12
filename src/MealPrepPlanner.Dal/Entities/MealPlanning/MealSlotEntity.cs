namespace MealPrepPlanner.Dal.Entities.MealPlanning;

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.MealPlanning.MealSlot</c>.
/// </summary>
public class MealSlotEntity
{
    public Guid Id { get; set; }

    public Guid MealPlanId { get; set; }

    /// <summary>Day name string (e.g. "monday"). Domain uses <see cref="DayOfWeek"/>; here it is stored as the canonical lower-case name.</summary>
    public string DayOfWeek { get; set; } = string.Empty;

    /// <summary>Meal type string ("breakfast"/"lunch"/"dinner"/"snack").</summary>
    public string MealType { get; set; } = string.Empty;

    public Guid? RecipeId { get; set; }

    public string? RecipeName { get; set; }

    public int Servings { get; set; }

    public bool IsPrepMeal { get; set; }

    public string? PrepNotes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
