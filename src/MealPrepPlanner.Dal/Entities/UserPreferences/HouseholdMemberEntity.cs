namespace MealPrepPlanner.Dal.Entities.UserPreferences;

public class HouseholdMemberEntity
{
    public Guid Id { get; set; }

    public Guid HouseholdId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Age { get; set; }

    public string Sex { get; set; } = string.Empty;

    public decimal WeightKg { get; set; }

    public int HeightCm { get; set; }

    public string ActivityLevel { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}
