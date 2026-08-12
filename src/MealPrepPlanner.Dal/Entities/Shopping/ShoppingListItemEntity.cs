namespace MealPrepPlanner.Dal.Entities.Shopping;

public class ShoppingListItemEntity
{
    public Guid Id { get; set; }

    public Guid ShoppingListId { get; set; }

    public Guid IngredientId { get; set; }

    public decimal? QuantityNeededAmount { get; set; }

    public string? QuantityNeededUnit { get; set; }

    public decimal? QuantityToBuyAmount { get; set; }

    public string? QuantityToBuyUnit { get; set; }

    public decimal? EstimatedPriceAmount { get; set; }

    public string? EstimatedPriceCurrency { get; set; }

    public bool Purchased { get; set; }

    public decimal? PriceAtPurchaseAmount { get; set; }

    public string? PriceAtPurchaseCurrency { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
