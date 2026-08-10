namespace MealPrepPlanner.Domain.MealPlanning;

public enum ConflictSeverity
{
    Warning,
    Error
}

public enum ConflictType
{
    Allergy,
    Equipment,
    Time
}

/// <summary>
/// A single hard- or soft-constraint violation found in a meal plan draft.
/// </summary>
public readonly record struct ConflictViolation(
    ConflictSeverity Severity,
    ConflictType Type,
    string Message,
    DayOfWeek? Day,
    MealType? Meal,
    Guid? RecipeId);

/// <summary>
/// Outcome of checking a meal plan draft against household preferences.
/// </summary>
public sealed record ConflictResult(bool Valid, IReadOnlyList<ConflictViolation> Violations)
{
    public static ConflictResult None() => new(true, []);

    public static ConflictResult Invalid(IReadOnlyList<ConflictViolation> violations) =>
        new(false, violations);
}
