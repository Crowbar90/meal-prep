namespace MealPrepPlanner.Dal.Entities.UserPreferences;

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.UserPreferences.Household</c>.
/// Plain POCO; the DAL does not reuse Domain types so the Domain stays ORM-free.
/// </summary>
public class HouseholdEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<HouseholdMemberEntity> Members { get; set; } = [];

    public PreferencesEntity? Preferences { get; set; }
}
