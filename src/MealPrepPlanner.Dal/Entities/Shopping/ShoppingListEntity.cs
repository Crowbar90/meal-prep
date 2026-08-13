namespace MealPrepPlanner.Dal.Entities.Shopping;

public class ShoppingListEntity
{
    public Guid Id { get; set; }

    public Guid MealPlanId { get; set; }

    public Guid? SupermarketId { get; set; }

    public decimal? EstimatedTotalCost { get; set; }

    public string Currency { get; set; } = "EUR";

    public string Status { get; set; } = "pending";

    public DateTimeOffset CreatedAt { get; set; }

    public List<ShoppingListItemEntity> Items { get; set; } = [];
}
