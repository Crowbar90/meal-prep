namespace MealPrepPlanner.Domain.MealPrep.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PrepScheduleFeasibilityChecked : DomainEvent
{
    public PrepScheduleFeasibilityChecked(
        Guid prepScheduleId,
        int violationCount,
        bool valid,
        Guid correlationId)
        : base(correlationId)
    {
        PrepScheduleId = prepScheduleId;
        ViolationCount = violationCount;
        Valid = valid;
    }

    public Guid PrepScheduleId { get; }

    public int ViolationCount { get; }

    public bool Valid { get; }
}
