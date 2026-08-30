namespace MealPrepPlanner.Domain.MealPrep;

/// <summary>
/// JSONB-serialized envelope stored in <c>prep_schedules.feasibility_violations</c>.
/// Holds the deterministic verdict from the last validator run; null until
/// the schedule has been through feasibility checking at least once.
/// </summary>
public sealed record PrepFeasibilityViolationsDocument(
    DateTimeOffset CheckedAt,
    IReadOnlyList<PrepFeasibilityViolationDocument> Violations);

public enum PrepViolationKind
{
    EquipmentUnavailable,
    TimeBudgetExceeded,
    FoodSafetyWindow,
    RecipeNotFound,
    OverlappingEquipmentUsage
}

/// <summary>
/// A single violation returned by <c>PrepFeasibilityValidator</c>. Carries
/// enough context for the agent to refine the draft without re-running the
/// whole pipeline.
/// </summary>
public sealed record PrepFeasibilityViolationDocument(
    PrepViolationKind Kind,
    Guid PrepTaskId,
    Guid RecipeId,
    string Message,
    string? Suggestion = null);
