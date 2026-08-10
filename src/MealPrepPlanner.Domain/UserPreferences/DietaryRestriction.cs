namespace MealPrepPlanner.Domain.UserPreferences;

/// <summary>
/// A dietary restriction such as "vegan", "nut-free", or "gluten-free".
/// Names are normalized (trimmed, lowercased) so comparisons are stable.
/// </summary>
public readonly record struct DietaryRestriction
{
    public DietaryRestriction(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = Normalize(name);
    }

    public string Name { get; }

    /// <summary>
    /// Best-effort matching between an ingredient allergen and this restriction.
    /// Handles "X-free", "no-X", and exact/substring forms; refinements belong in
    /// curated allergen→restriction mapping data.
    /// </summary>
    public bool ConflictsWith(string allergen)
    {
        var normalized = Normalize(allergen);
        if (normalized.Length == 0)
            return false;

        if (Name == normalized)
            return true;

        if (Name.EndsWith("-free", StringComparison.Ordinal))
        {
            var forbidden = Name[..^5];
            return forbidden.Length > 0 && normalized.Contains(forbidden, StringComparison.Ordinal);
        }

        if (Name.StartsWith("no-", StringComparison.Ordinal))
        {
            var forbidden = Name[3..];
            return forbidden.Length > 0 && normalized.Contains(forbidden, StringComparison.Ordinal);
        }

        return normalized.Contains(Name, StringComparison.Ordinal)
            || Name.Contains(normalized, StringComparison.Ordinal);
    }

    public static string Normalize(string value) => value.Trim().ToLowerInvariant();

    public override string ToString() => Name;
}
