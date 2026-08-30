namespace MealPrepPlanner.Domain.MealPrep.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class PrepScheduleArchived : DomainEvent
{
    public PrepScheduleArchived(Guid prepScheduleId, Guid correlationId)
        : base(correlationId)
    {
        PrepScheduleId = prepScheduleId;
    }

    public Guid PrepScheduleId { get; }
}
