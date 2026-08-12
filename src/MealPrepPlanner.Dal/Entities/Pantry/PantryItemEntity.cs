namespace MealPrepPlanner.Dal.Entities.Pantry;

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.Pantry.PantryItem</c>.
/// </summary>
public class PantryItemEntity
{
    public Guid Id { get; set; }

    public Guid HouseholdId { get; set; }

    public Guid IngredientId { get; set; }

    public decimal QuantityAmount { get; set; }

    public string QuantityUnit { get; set; } = string.Empty;

    public DateOnly DateAdded { get; set; }

    public DateOnly? ExpiresAt { get; set; }

    public string Status { get; set; } = "available";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
