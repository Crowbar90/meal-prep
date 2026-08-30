namespace MealPrepPlanner.Dal.Entities.MealPrep;

using MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.MealPrep.PrepSchedule</c>.
/// Persisted as <c>prep_schedules</c> per <c>docs/architecture/data-model.md</c>.
/// </summary>
public class PrepScheduleEntity
{
    public Guid Id { get; set; }

    public Guid HouseholdId { get; set; }

    public Guid MealPlanId { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public string Status { get; set; } = "draft";

    public Guid? WorkflowId { get; set; }

    public int Version { get; set; }

    public PrepFeasibilityViolationsRow? FeasibilityViolations { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<PrepTaskEntity> Tasks { get; set; } = [];
}
