namespace MealPrepPlanner.Domain.MealPlanning;

using MealPrepPlanner.Domain.MealPlanning.Events;
using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;

/// <summary>
/// Aggregate root for the weekly meal plan and its lifecycle
/// (draft → pending review → finalized → archived).
/// </summary>
public class MealPlan : Entity
{
    private readonly List<MealSlot> _slots = [];
    private readonly List<DomainEvent> _domainEvents = [];

    private MealPlan()
    {
    }

    private MealPlan(Guid id, Guid householdId, DateOnly weekStartDate)
        : base(id)
    {
        HouseholdId = householdId;
        WeekStartDate = weekStartDate;
        Status = MealPlanStatus.Draft;
    }

    public Guid HouseholdId { get; }

    public DateOnly WeekStartDate { get; }

    public MealPlanStatus Status { get; private set; }

    public IReadOnlyList<MealSlot> Slots => _slots;

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;

    public static MealPlan CreateDraft(Guid householdId, DateOnly weekStartDate, Guid correlationId = default)
    {
        if (householdId == Guid.Empty)
            throw new ArgumentException("Household id must not be empty.", nameof(householdId));

        var plan = new MealPlan(Guid.NewGuid(), householdId, weekStartDate);
        plan._domainEvents.Add(new MealPlanDraftCreated(plan.Id, householdId, weekStartDate, correlationId));
        return plan;
    }

    public MealSlot AddSlot(
        DayOfWeek dayOfWeek,
        MealType mealType,
        int servings,
        bool isPrepMeal = false,
        string? prepNotes = null)
    {
        EnsureStatus(MealPlanStatus.Draft);

        if (servings <= 0)
            throw new ArgumentOutOfRangeException(nameof(servings), "Servings must be a positive number.");

        var slot = new MealSlot(Guid.NewGuid(), dayOfWeek, mealType, servings, isPrepMeal, prepNotes);
        _slots.Add(slot);
        return slot;
    }

    public void AssignRecipe(MealSlot slot, Recipe recipe, int servings)
    {
        ArgumentNullException.ThrowIfNull(slot);
        ArgumentNullException.ThrowIfNull(recipe);

        EnsureStatus(MealPlanStatus.Draft, MealPlanStatus.PendingReview);

        if (!_slots.Contains(slot))
            throw new InvalidOperationException("The slot does not belong to this meal plan.");

        slot.AssignRecipe(recipe, servings);
    }

    public void SubmitForReview()
    {
        EnsureStatus(MealPlanStatus.Draft);
        Status = MealPlanStatus.PendingReview;
    }

    public void ValidateNutrition(
        NutritionProfile dailyProfile,
        bool isValid,
        IReadOnlyList<string>? warnings = null,
        Guid correlationId = default)
    {
        EnsureStatus(MealPlanStatus.Draft, MealPlanStatus.PendingReview);
        _domainEvents.Add(new MealPlanNutritionValidated(Id, isValid, warnings ?? [], dailyProfile, correlationId));
    }

    public void Finalize(Guid correlationId = default)
    {
        EnsureStatus(MealPlanStatus.PendingReview);
        Status = MealPlanStatus.Finalized;
        _domainEvents.Add(new MealPlanFinalized(Id, HouseholdId, DateTimeOffset.UtcNow, correlationId));
    }

    public void Archive(Guid correlationId = default)
    {
        if (Status == MealPlanStatus.Archived)
            throw new InvalidOperationException("The meal plan is already archived.");

        Status = MealPlanStatus.Archived;
        _domainEvents.Add(new MealPlanArchived(Id, correlationId));
    }

    public void ClearDomainEvents() => _domainEvents.Clear();

    private void EnsureStatus(params MealPlanStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new InvalidOperationException(
                $"Invalid status transition. Current status is '{Status}'; allowed statuses: {string.Join(", ", allowed)}.");
        }
    }
}
