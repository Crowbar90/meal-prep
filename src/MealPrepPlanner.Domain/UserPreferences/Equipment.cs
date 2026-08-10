namespace MealPrepPlanner.Domain.UserPreferences;

/// <summary>
/// A piece of kitchen equipment (e.g. "instant_pot", "oven"). Names are normalized
/// (trimmed, lowercased) so comparisons are stable.
/// </summary>
public readonly record struct Equipment
{
    public Equipment(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name must not be empty.", nameof(name));

        Name = Normalize(name);
    }

    public string Name { get; }

    public static string Normalize(string value) => value.Trim().ToLowerInvariant();

    public override string ToString() => Name;
}
