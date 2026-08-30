namespace MealPrepPlanner.Dal.Entities.MealPrep;

using MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence projection of
/// <c>MealPrepPlanner.Domain.MealPrep.PrepFeasibilityViolationsDocument</c>.
/// Stored as <c>jsonb</c> in <c>prep_schedules.feasibility_violations</c>.
/// </summary>
public sealed class PrepFeasibilityViolationsRow
{
    public DateTimeOffset CheckedAt { get; set; }

    public List<PrepFeasibilityViolationRow> Violations { get; set; } = [];
}

public enum PrepViolationKindRow
{
    EquipmentUnavailable,
    TimeBudgetExceeded,
    FoodSafetyWindow,
    RecipeNotFound,
    OverlappingEquipmentUsage
}

public sealed class PrepFeasibilityViolationRow
{
    public PrepViolationKindRow Kind { get; set; }

    public Guid PrepTaskId { get; set; }

    public Guid RecipeId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Suggestion { get; set; }
}
