namespace MealPrepPlanner.Domain.Shared;

/// <summary>
/// Base class for domain events. Events are immutable and are raised inside
/// aggregate methods, never from the application layer.
/// </summary>
public abstract class DomainEvent
{
    protected DomainEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    public Guid CorrelationId { get; }

    public DateTimeOffset OccurredAt { get; }
}
