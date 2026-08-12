namespace MealPrepPlanner.Dal.Entities.Recipes;

/// <summary>
/// Persistence projection of a recipe line item. The Domain's <c>RecipeIngredient</c>
/// carries a snapshot of the ingredient; the DAL mirrors the same fields and adds
/// the FK back to <see cref="RecipeEntity"/>.
/// </summary>
public class RecipeIngredientEntity
{
    public Guid Id { get; set; }

    public Guid RecipeId { get; set; }

    public Guid IngredientId { get; set; }

    public decimal QuantityAmount { get; set; }

    public string QuantityUnit { get; set; } = string.Empty;

    public bool IsOptional { get; set; }

    public string? Preparation { get; set; }

    /// <summary>Snapshot of the ingredient name at recipe-write time.</summary>
    public string IngredientName { get; set; } = string.Empty;
}
