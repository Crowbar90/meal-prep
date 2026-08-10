namespace MealPrepPlanner.Domain.UserPreferences;

using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Aggregate root for a group of people sharing meals, their members, and preferences.
/// </summary>
public class Household : Entity
{
    private readonly List<HouseholdMember> _members = [];

    private Household()
    {
        Name = string.Empty;
    }

    private Household(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    public string Name { get; private set; }

    public IReadOnlyList<HouseholdMember> Members => _members;

    public Preferences? Preferences { get; private set; }

    public static Household Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Household name must not be empty.", nameof(name));

        return new Household(Guid.NewGuid(), name);
    }

    public HouseholdMember AddMember(
        string name,
        int age,
        Sex sex,
        decimal weightKg,
        int heightCm,
        ActivityLevel activityLevel)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Member name must not be empty.", nameof(name));

        if (age <= 0)
            throw new ArgumentOutOfRangeException(nameof(age), "Age must be a positive number.");

        if (weightKg <= 0)
            throw new ArgumentOutOfRangeException(nameof(weightKg), "Weight must be a positive number.");

        if (heightCm <= 0)
            throw new ArgumentOutOfRangeException(nameof(heightCm), "Height must be a positive number.");

        if (_members.Any(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"A member named '{name}' already exists in this household.");

        var member = new HouseholdMember(Guid.NewGuid(), name, age, sex, weightKg, heightCm, activityLevel);
        _members.Add(member);
        return member;
    }

    public void UpdatePreferences(Preferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        Preferences = preferences;
    }
}
