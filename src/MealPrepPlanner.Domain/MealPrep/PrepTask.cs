namespace MealPrepPlanner.Domain.MealPrep;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.Shared;

/// <summary>
/// A single batch-cooking or prep step within a <see cref="PrepSchedule"/>.
/// Child entity of the <see cref="PrepSchedule"/> aggregate; only mutated
/// through the aggregate root. Persisted as <c>prep_tasks</c> per
/// <c>docs/architecture/data-model.md</c>.
/// </summary>
public class PrepTask : Entity
{
    private readonly List<string> _equipmentIds = [];
    private readonly List<Guid> _assignedToSlotIds = [];

    private PrepTask()
    {
        RecipeId = Guid.Empty;
        DayOfWeek = DayOfWeek.Monday;
        MealType = MealType.Dinner;
        Steps = [];
    }

    internal PrepTask(
        Guid id,
        Guid recipeId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        int batchSizeServings,
        int earliestStartOffsetMinutes,
        int latestFinishOffsetMinutes,
        IReadOnlyList<string>? equipmentIds = null,
        IReadOnlyList<PrepStepDocument>? steps = null,
        IReadOnlyList<Guid>? assignedToSlotIds = null)
        : base(id)
    {
        if (recipeId == Guid.Empty)
            throw new ArgumentException("Recipe id must not be empty.", nameof(recipeId));

        if (batchSizeServings <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSizeServings), "Batch size must be a positive number of servings.");

        if (earliestStartOffsetMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(earliestStartOffsetMinutes), "Earliest start offset must be non-negative.");

        if (latestFinishOffsetMinutes < earliestStartOffsetMinutes)
            throw new ArgumentOutOfRangeException(
                nameof(latestFinishOffsetMinutes),
                $"Latest finish ({latestFinishOffsetMinutes}m) must be at or after earliest start ({earliestStartOffsetMinutes}m).");

        RecipeId = recipeId;
        DayOfWeek = dayOfWeek;
        MealType = mealType;
        BatchSizeServings = batchSizeServings;
        EarliestStartOffsetMinutes = earliestStartOffsetMinutes;
        LatestFinishOffsetMinutes = latestFinishOffsetMinutes;
        Steps = (steps ?? []).ToList();

        _equipmentIds.AddRange((equipmentIds ?? []).Where(s => !string.IsNullOrWhiteSpace(s)));
        _assignedToSlotIds.AddRange(assignedToSlotIds ?? []);
    }

    public Guid RecipeId { get; }

    public DayOfWeek DayOfWeek { get; }

    public MealType MealType { get; }

    public int BatchSizeServings { get; }

    public int EarliestStartOffsetMinutes { get; }

    public int LatestFinishOffsetMinutes { get; }

    public IReadOnlyList<string> EquipmentIds => _equipmentIds;

    public IReadOnlyList<PrepStepDocument> Steps { get; }

    public IReadOnlyList<Guid> AssignedToSlotIds => _assignedToSlotIds;

    public TimeWindow Window => new(EarliestStartOffsetMinutes, LatestFinishOffsetMinutes);

    public static PrepTask Create(
        Guid recipeId,
        DayOfWeek dayOfWeek,
        MealType mealType,
        int batchSizeServings,
        int earliestStartOffsetMinutes,
        int latestFinishOffsetMinutes,
        IReadOnlyList<string>? equipmentIds = null,
        IReadOnlyList<PrepStepDocument>? steps = null,
        IReadOnlyList<Guid>? assignedToSlotIds = null) =>
        new(
            Guid.NewGuid(),
            recipeId,
            dayOfWeek,
            mealType,
            batchSizeServings,
            earliestStartOffsetMinutes,
            latestFinishOffsetMinutes,
            equipmentIds,
            steps,
            assignedToSlotIds);
}

/// <summary>
/// One ordered step in a prep task (e.g. "Dice onions", "Bake at 180C for
/// 25 minutes"). Persisted as JSONB inside <c>prep_tasks.steps</c>.
/// </summary>
public sealed record PrepStepDocument(
    int Order,
    string Description,
    int? DurationMinutes = null,
    string? EquipmentId = null);
