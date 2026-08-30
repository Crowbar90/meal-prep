namespace MealPrepPlanner.Domain.MealPrep;

using MealPrepPlanner.Domain.MealPrep.Events;
using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Aggregate root for the weekly batch-cooking and prep plan. Persisted as
/// <c>prep_schedules</c> per <c>docs/architecture/data-model.md</c>. Status
/// transitions are enforced: <c>draft → feasibility_checked → finalized
/// → archived</c>. The agent proposes a draft; the deterministic
/// <see cref="Services.PrepFeasibilityValidator"/> runs against the draft;
/// only finalized schedules are acted on.
/// </summary>
public class PrepSchedule : Entity
{
    private readonly List<PrepTask> _tasks = [];
    private readonly List<DomainEvent> _domainEvents = [];

    private PrepSchedule()
    {
        FeasibilityViolations = null;
    }

    private PrepSchedule(Guid id, Guid householdId, Guid mealPlanId, DateOnly weekStartDate)
        : base(id)
    {
        HouseholdId = householdId;
        MealPlanId = mealPlanId;
        WeekStartDate = weekStartDate;
        Status = PrepScheduleStatus.Draft;
        Version = 1;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid HouseholdId { get; }

    public Guid MealPlanId { get; }

    public DateOnly WeekStartDate { get; }

    public PrepScheduleStatus Status { get; private set; }

    public IReadOnlyList<PrepTask> Tasks => _tasks;

    public Guid? WorkflowId { get; private set; }

    public int Version { get; private set; }

    public PrepFeasibilityViolationsDocument? FeasibilityViolations { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Factory: creates a new draft schedule with no tasks. Emits
    /// <see cref="PrepScheduleDraftCreated"/>.
    /// </summary>
    public static PrepSchedule CreateDraft(
        Guid householdId,
        Guid mealPlanId,
        DateOnly weekStartDate,
        Guid correlationId = default)
    {
        if (householdId == Guid.Empty)
            throw new ArgumentException("Household id must not be empty.", nameof(householdId));

        if (mealPlanId == Guid.Empty)
            throw new ArgumentException("Meal plan id must not be empty.", nameof(mealPlanId));

        var schedule = new PrepSchedule(Guid.NewGuid(), householdId, mealPlanId, weekStartDate);
        schedule._domainEvents.Add(new PrepScheduleDraftCreated(
            schedule.Id,
            mealPlanId,
            householdId,
            weekStartDate,
            correlationId));
        return schedule;
    }

    public PrepTask AddTask(PrepTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        EnsureStatus(PrepScheduleStatus.Draft, PrepScheduleStatus.FeasibilityChecked);
        _tasks.Add(task);
        Touch();
        return task;
    }

    public void AssignWorkflow(Guid workflowId)
    {
        if (workflowId == Guid.Empty)
            throw new ArgumentException("Workflow id must not be empty.", nameof(workflowId));

        WorkflowId = workflowId;
        Touch();
    }

    public void MarkFeasibilityChecked(
        PrepFeasibilityViolationsDocument violations,
        Guid correlationId = default)
    {
        ArgumentNullException.ThrowIfNull(violations);

        EnsureStatus(PrepScheduleStatus.Draft, PrepScheduleStatus.FeasibilityChecked);

        FeasibilityViolations = violations;
        Status = PrepScheduleStatus.FeasibilityChecked;
        Version++;
        Touch();

        _domainEvents.Add(new PrepScheduleFeasibilityChecked(
            Id,
            violations.Violations.Count,
            violations.Violations.Count == 0,
            correlationId));
    }

    public void Finalize(Guid correlationId = default)
    {
        EnsureStatus(PrepScheduleStatus.FeasibilityChecked);

        if (FeasibilityViolations is { Violations.Count: > 0 })
            throw new InvalidOperationException(
                "Cannot finalize a prep schedule that has open feasibility violations. Re-run the validator and resolve violations first.");

        Status = PrepScheduleStatus.Finalized;
        Version++;
        Touch();

        _domainEvents.Add(new PrepScheduleFinalized(Id, correlationId));
    }

    public void Archive(Guid correlationId = default)
    {
        if (Status == PrepScheduleStatus.Archived)
            throw new InvalidOperationException("The prep schedule is already archived.");

        Status = PrepScheduleStatus.Archived;
        Touch();

        _domainEvents.Add(new PrepScheduleArchived(Id, correlationId));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void Touch()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private void EnsureStatus(params PrepScheduleStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new InvalidOperationException(
                $"Invalid status transition. Current status is '{Status}'; allowed statuses: {string.Join(", ", allowed)}.");
        }
    }
}
