# Communication Patterns

This document defines how components communicate in the AI Meal Prep Planner.

## Pattern Overview

| Communication | Protocol | Direction | Use Case |
|---------------|----------|-----------|----------|
| Telegram → OpenClaw | Webhook / Long-polling | Incoming | User triggers workflow |
| OpenClaw → Backend | MCP over HTTP/SSE | Outgoing | Agent tool calls |
| Backend → OpenClaw | REST / WebSocket | Outgoing | Workflow status, admin |
| Frontend ↔ Backend | REST/JSON | Bidirectional | Web UI admin tasks |
| Backend internal | MassTransit (in-memory) | Pub/Sub | Domain events |
| Backend → PostgreSQL | TCP/SQL | Outgoing | Data persistence |
| Backend → Redis | TCP | Outgoing | Caching |

## MCP (Model Context Protocol)

### Why MCP?

MCP is an open protocol for connecting AI assistants to data sources and tools. It provides:
- Standardized tool discovery and calling
- Structured input/output schemas
- Built-in error handling
- Transport abstraction (HTTP, SSE, stdio)

### MCP Server Implementation

The backend exposes an MCP server alongside the REST API:

```csharp
// Program.cs
builder.Services.AddMcpServer()
    .WithToolsFromAssembly(typeof(Program).Assembly);

app.MapMcp("/mcp");
```

### MCP Tool Registration

```csharp
[McpTool("calculate_nutrition")]
public class NutritionTools
{
    [McpToolMethod]
    public async Task<NutritionProfile> CalculateNutrition(
        [McpParameter] Guid recipeId,
        [McpParameter] int servings)
    {
        // Implementation
    }
}
```

### MCP vs. REST for AI

| Aspect | MCP | REST |
|--------|-----|------|
| Schema discovery | Automatic (tool definitions) | Manual (OpenAPI) |
| Function calling | Native | Custom wrapper needed |
| Error format | Standardized | Application-defined |
| Streaming | SSE support | Requires custom SSE |
| AI-native | Yes | Requires adaptation |

**Decision:** Use MCP for AI agent communication. Use REST for human-facing web UI.

## MassTransit Internal Events

### When to Use MassTransit

Use MassTransit for **backend-internal** domain events:

- `MealPlanFinalized` → Shopping context generates list
- `PantryItemExpired` → Notification context alerts user
- `RecipeCreated` → Search index updated

### When NOT to Use MassTransit

Do NOT use MassTransit for:
- OpenClaw workflow steps (OpenClaw owns this)
- AI agent orchestration (OpenClaw owns this)
- Synchronous user requests (use REST/MCP directly)

### Event Types

**Domain Events** (in-process, within bounded context):
```csharp
// Occurs within the MealPlanning context
public record MealSlotAssigned(Guid MealPlanId, Guid MealSlotId, Guid RecipeId);
```

**Integration Events** (cross-context, via MassTransit):
```csharp
// Crosses from MealPlanning to Shopping
public record MealPlanFinalized(Guid MealPlanId, Guid HouseholdId, DateTimeOffset FinalizedAt);
```

### MassTransit Configuration

```csharp
services.AddMassTransit(x =>
{
    x.AddConsumer<MealPlanFinalizedConsumer>();

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});
```

**Future:** Switch to RabbitMQ transport when:
- Multiple backend replicas need event distribution
- Events must survive pod restarts
- Backpressure handling required

## REST API Design

### Resource Naming

```
GET    /api/households/{id}/mealplans           # List meal plans
POST   /api/households/{id}/mealplans            # Create manually
GET    /api/households/{id}/mealplans/{planId}   # Get specific plan
PUT    /api/households/{id}/mealplans/{planId}   # Update plan
DELETE /api/households/{id}/mealplans/{planId}   # Archive plan
GET    /api/households/{id}/pantry               # Get pantry
POST   /api/households/{id}/pantry               # Add item
GET    /api/households/{id}/preferences          # Get preferences
PUT    /api/households/{id}/preferences          # Update preferences
GET    /api/recipes                              # Search recipes
GET    /api/recipes/{id}                         # Get recipe
```

### Response Format

```json
{
  "data": { /* resource */ },
  "meta": {
    "request_id": "uuid",
    "timestamp": "2026-08-10T10:00:00Z"
  }
}
```

### Error Format

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Recipe requires equipment not available: pressure_cooker",
    "details": [
      { "field": "recipe.equipment_needed", "issue": "missing_equipment" }
    ]
  }
}
```

## Correlation and Tracing

### Correlation ID Propagation

```
Telegram Request
  └── X-Request-Id: req-123
        └── OpenClaw Workflow
              └── X-Workflow-Id: wf-456
                    └── MCP Tool Call
                          └── X-Workflow-Id: wf-456 (passed to backend)
                                └── Backend Processing
                                      └── X-Request-Id: req-123 (logged)
                                            └── Database Query
                                                  └── logged with req-123
```

### OpenTelemetry

All components emit OpenTelemetry traces:
- **Spans:** HTTP request, MCP tool call, DB query, LLM API call
- **Attributes:** `workflow_id`, `agent_name`, `tool_name`, `household_id`
- **Exporter:** OTLP to Jaeger or Grafana Tempo

## Security

### Authentication

- **REST API:** JWT tokens (ASP.NET Core 10 Identity)
- **MCP Server:** API key (X-Api-Key header)
- **OpenClaw → Backend:** mTLS or API key

### Authorization

- All endpoints validate `household_id` against the authenticated user
- Users cannot access other households' data
- MCP tools enforce the same authorization as REST endpoints

### Data Flow

```
┌─────────┐     ┌─────────┐     ┌─────────┐     ┌─────────┐
│ Telegram│────►│OpenClaw │────►│  MCP    │────►│ Backend │
│  (user) │     │ (agent) │     │ (tools) │     │ (truth) │
└─────────┘     └─────────┘     └─────────┘     └─────────┘
     │                                              │
     │         AI NEVER TOUCHES DB                  ▼
     │         DIRECTLY                             ┌─────────┐
     └──────────────────────────────────────────────│PostgreSQL│
                                                   └─────────┘
```
