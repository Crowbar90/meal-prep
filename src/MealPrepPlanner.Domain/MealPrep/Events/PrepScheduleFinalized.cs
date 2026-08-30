namespace MealPrepPlanner.Domain.MealPrep.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PrepScheduleFinalized : DomainEvent
{
    public PrepScheduleFinalized(Guid prepScheduleId, Guid correlationId)
        : base(correlationId)
    {
        PrepScheduleId = prepScheduleId;
    }

    public Guid PrepScheduleId { get; }
}
