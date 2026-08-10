namespace MealPrepPlanner.Domain.MealPrep;

using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Aggregate root for the weekly batch-cooking and prep plan.
/// </summary>
public class PrepSchedule : Entity
{
    private readonly List<PrepTask> _tasks = [];
    private readonly List<DomainEvent> _domainEvents = [];

    private PrepSchedule()
    {
    }

    private PrepSchedule(Guid id, Guid mealPlanId)
        : base(id)
    {
        MealPlanId = mealPlanId;
    }

    public Guid MealPlanId { get; }

    public IReadOnlyList<PrepTask> Tasks => _tasks;

    public int TotalPrepTimeMinutes => _tasks.Sum(t => t.DurationMinutes);

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    public static PrepSchedule Create(Guid mealPlanId, IReadOnlyList<PrepTask>? tasks = null, Guid correlationId = default)
    {
        if (mealPlanId == Guid.Empty)
            throw new ArgumentException("Meal plan id must not be empty.", nameof(mealPlanId));

        var schedule = new PrepSchedule(Guid.NewGuid(), mealPlanId);
        schedule._tasks.AddRange(tasks ?? []);
        schedule._domainEvents.Add(new Events.PrepScheduleGenerated(schedule.Id, mealPlanId, correlationId));
        return schedule;
    }

    public void AddTask(PrepTask task)
    {
        ArgumentNullException.ThrowIfNull(task);
        _tasks.Add(task);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}
