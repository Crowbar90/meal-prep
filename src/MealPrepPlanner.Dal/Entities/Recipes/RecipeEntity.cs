namespace MealPrepPlanner.Dal.Entities.Recipes;

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.Recipes.Recipe</c>.
/// Instructions are stored as <c>text[]</c>. Tags and EquipmentNeeded are
/// <c>text[]</c> (scalar lists, queryable with GIN).
/// </summary>
public class RecipeEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string[] Instructions { get; set; } = [];

    public int PrepTimeMinutes { get; set; }

    public int CookTimeMinutes { get; set; }

    public int BaseServings { get; set; }

    public string[] EquipmentNeeded { get; set; } = [];

    public string[] Tags { get; set; } = [];

    public string? Source { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<RecipeIngredientEntity> Ingredients { get; set; } = [];
}
