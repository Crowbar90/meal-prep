namespace MealPrepPlanner.Dal.Entities.Audit;

/// <summary>
/// Raw prompt/response log per AI call. Backed by <c>ai_execution_logs</c>.
/// Append-only.
/// </summary>
public class AiExecutionLogEntity
{
    public long Id { get; set; }

    public Guid WorkflowId { get; set; }

    public string AgentName { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;

    public string? Response { get; set; }

    public int? TokensUsed { get; set; }

    public int? DurationMs { get; set; }

    public string? Model { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
