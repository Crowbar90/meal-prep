namespace MealPrepPlanner.Dal.Entities.Shopping;

/// <summary>
/// Append-mostly price history per (supermarket, ingredient). The latest row
/// per pair (by <see cref="RecordedAt"/> desc) is the current price.
/// </summary>
public class SupermarketPriceEntity
{
    public Guid Id { get; set; }

    public Guid SupermarketId { get; set; }

    public Guid IngredientId { get; set; }

    public decimal Price { get; set; }

    public string Currency { get; set; } = "EUR";

    public decimal? PackageSize { get; set; }

    public string? PackageUnit { get; set; }

    public DateTimeOffset RecordedAt { get; set; }
}
