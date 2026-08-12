namespace MealPrepPlanner.Dal.Entities.Audit;

/// <summary>
/// Append-only row capturing one agent or backend decision. Backed by the
/// <c>decision_events</c> table. <see cref="SequenceNumber"/> is supplied by the
/// caller (OpenClaw per ADR 005) and is unique within a <see cref="WorkflowId"/>.
/// </summary>
public class DecisionEventEntity
{
    public long Id { get; set; }

    public Guid WorkflowId { get; set; }

    public int SequenceNumber { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    /// <summary>"USER" | "AI_AGENT" | "BACKEND_SERVICE" — check-constrained.</summary>
    public string ActorType { get; set; } = string.Empty;

    public string ActorName { get; set; } = string.Empty;

    /// <summary>Free-form decision-type label (e.g. "PROPOSED", "VALIDATED", "APPROVED").</summary>
    public string DecisionType { get; set; } = string.Empty;

    public string InputContext { get; set; } = "{}";

    public string OutputDecision { get; set; } = "{}";

    public string? Reasoning { get; set; }

    public long? ParentDecisionId { get; set; }
}
