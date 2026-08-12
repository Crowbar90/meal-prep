namespace MealPrepPlanner.Dal.Entities.Shopping;

public class SupermarketEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Chain { get; set; }

    public string? Location { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
