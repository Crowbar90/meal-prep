namespace MealPrepPlanner.Domain.Shared;

/// <summary>
/// A reference to a piece of kitchen equipment (e.g. "instant_pot", "oven").
/// Wraps a normalized name so comparisons across recipes and prep tasks are
/// case-insensitive and trim-stable.
/// </summary>
public sealed class EquipmentRef : ValueObject
{
    public EquipmentRef(string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Equipment name must not be empty.", nameof(name));

        Name = Normalize(name);
    }

    public string Name { get; }

    public static string Normalize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Trim().ToLowerInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
    }

    public override string ToString() => Name;
}
