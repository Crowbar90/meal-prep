namespace MealPrepPlanner.Dal.Entities.MealPrep;

/// <summary>
/// Persistence projection of a single prep step inside a <c>prep_tasks.steps</c>
/// JSONB payload.
/// </summary>
public sealed class PrepTaskStepRow
{
    public int Order { get; set; }

    public string Description { get; set; } = string.Empty;

    public int? DurationMinutes { get; set; }

    public string? EquipmentId { get; set; }
}

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.MealPrep.PrepTask</c>.
/// Persisted as <c>prep_tasks</c> per <c>docs/architecture/data-model.md</c>.
/// </summary>
public class PrepTaskEntity
{
    public Guid Id { get; set; }

    public Guid PrepScheduleId { get; set; }

    public Guid RecipeId { get; set; }

    public string DayOfWeek { get; set; } = string.Empty;

    public string MealType { get; set; } = string.Empty;

    public int BatchSizeServings { get; set; }

    public int? EarliestStartOffsetMinutes { get; set; }

    public int? LatestFinishOffsetMinutes { get; set; }

    public string[] EquipmentIds { get; set; } = [];

    public List<PrepTaskStepRow> Steps { get; set; } = [];

    public Guid[] AssignedToSlotIds { get; set; } = [];

    public DateTimeOffset CreatedAt { get; set; }
}
