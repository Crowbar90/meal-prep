# AI Orchestration Architecture

This document describes how AI agents are orchestrated, how they interact with the backend, and how decisions are traced.

## Orchestration Philosophy

- **OpenClaw owns the workflow.** It triggers agents, manages retries, and persists workflow state.
- **Agents own creativity.** They invent recipes, combine flavors, and adapt to constraints.
- **Backend owns determinism.** All calculations, validation, and data persistence happen in C#.
- **Agents never touch the database.** They call MCP tools.

## Workflow Execution Flow

```
Telegram /generate
       │
       ▼
┌──────────────┐
│  OpenClaw    │──► Generates workflow_id
│  Workflow    │
└──────────────┘
       │
       ▼
┌──────────────┐     ┌──────────────┐
│   Agent A    │────►│  MCP Tools   │────► Backend API
│ (MealPlanner)│     │              │
└──────────────┘     └──────────────┘
       │
       ▼ (output)
┌──────────────┐     ┌──────────────┐
│   Agent B    │────►│  MCP Tools   │────► Backend API
│(Nutrition    │     │              │
│  Reviewer)   │     └──────────────┘
└──────────────┘
       │
       ▼
┌──────────────┐     ┌──────────────┐
│   Agent C    │────►│  MCP Tools   │────► Backend API
│ (Meal Prep   │     │              │
│  Optimizer)  │     └──────────────┘
└──────────────┘
       │
       ▼ (output: validated PrepSchedule)
   [... Meal Prep Optimizer outputs validated PrepSchedule; then notify_user]
       │
       ▼
┌──────────────┐
│  notify_user │──► Telegram reply
└──────────────┘
```

## Agent Lifecycle

1. **Trigger:** OpenClaw receives a Telegram command or scheduled event
2. **Workflow Init:** OpenClaw creates a `workflow_id` and loads the workflow YAML
3. **Context Loading:** OpenClaw calls backend tools (`get_preferences`, `get_pantry`) to load initial state
4. **Agent Execution:** OpenClaw invokes the agent with context + prompt
5. **Tool Calling:** Agent calls MCP tools as needed (OpenClaw proxies these)
6. **Validation:** Agent output is validated (schema check, backend verification)
7. **State Update:** OpenClaw stores step output for downstream agents
8. **Iteration:** Next agent receives previous output as input
9. **Completion:** Final step sends Telegram notification

## Decision Tracing

Every decision is traceable through three correlated data sources:

### 1. OpenClaw Workflow Logs

OpenClaw emits structured JSON logs:

```json
{
  "workflow_id": "wf-456",
  "step_id": "generate_draft",
  "agent_name": "meal_planner",
  "timestamp": "2026-08-10T10:00:15Z",
  "input_context": { /* full context */ },
  "output_decision": { /* agent output */ },
  "reasoning": "Lentils expire in 2 days...",
  "tools_called": ["get_preferences", "get_pantry", "search_recipes"],
  "duration_ms": 4500
}
```

### 2. Backend Decision Events Table

The backend logs every tool call as a `decision_event`:

```json
{
  "workflow_id": "wf-456",
  "sequence_number": 2,
  "actor_type": "AI_AGENT",
  "actor_name": "meal_planner",
  "decision_type": "PROPOSED",
  "input_context": { /* pantry state, preferences */ },
  "output_decision": { /* proposed plan */ },
  "reasoning": "Lentils expire in 2 days..."
}
```

### 3. AI Execution Logs Table

Raw LLM interactions:

```json
{
  "workflow_id": "wf-456",
  "agent_name": "meal_planner",
  "prompt": "...full prompt...",
  "response": "...full LLM response...",
  "tokens_used": 2450,
  "duration_ms": 3200,
  "model": "gpt-4.1"
}
```

### Reconstructing "Why Lentils?"

Query:
```sql
SELECT * FROM decision_events
WHERE workflow_id = 'wf-456'
ORDER BY sequence_number;
```

Result shows the full causal chain:
1. User initiated plan
2. Pantry had lentils expiring in 2 days
3. MealPlanner proposed lentil curry
4. NutritionEngine validated it
5. Meal Prep Optimizer proposed a Sun 4pm prep batch; PrepFeasibilityValidator confirmed it (no violations).
6. ShoppingOptimizer confirmed no purchase needed

## Meal Prep Optimizer (2026-08-16)

The Meal Prep Optimizer is the third agent in the workflow, invoked after the Nutrition Reviewer.

Its job:
1. Receive a finalized meal plan.
2. Propose a `PrepSchedule` draft (equipment assignments, time windows, batching).
3. Call the deterministic `validate_prep_schedule` MCP tool.
4. If violations are found, refine the draft and re-validate — up to N=3 times (configurable via workflow YAML).
5. Output the validated `PrepSchedule` for the next agent.

**Determinism boundary:** The agent is creative-only; the `PrepFeasibilityValidator` is a pure C# domain service. See ADR 003 — agent creativity is bounded by backend evaluation.

**Audit:** The `feasibility_violations` JSONB column on `prep_schedules` captures the last validator result for audit (per `docs/architecture/data-model.md`).

**Decision tracing:** A `decision_event` row is written for every `propose_prep_schedule` call (decision_type=`PROPOSED`) and every `validate_prep_schedule` call (decision_type=`VALIDATED`), just like the existing meal_planner / recipe_generator rows.

## Retry and Failure Handling

### Agent-Level Retries

OpenClaw handles retries per the workflow policy:
- **Transient failures** (LLM rate limit, timeout): Exponential backoff, max 3 attempts
- **Schema validation failures**: Retry with corrected prompt
- **Persistent failures**: Workflow transitions to `FAILED` state, user notified

### Backend Tool Retries

- MCP client (in OpenClaw) retries backend calls with circuit breaker
- Backend never retries AI calls — that's OpenClaw's responsibility

### Compensation

If a later step fails after earlier steps succeeded:
- Draft meal plans in `draft` status can be safely abandoned
- No financial transactions occur during generation
- Pantry items are only reserved on `MealPlanFinalized`, not during draft

## Security Boundaries

| Boundary | Enforcement |
|----------|-------------|
| Agent → Backend | MCP tools only, API key auth |
| Backend → Database | EF Core, parameterized queries |
| User → Agent | Via Telegram/OpenClaw, no direct access |
| Agent → LLM | Via OpenClaw proxy, rate limited |

## Performance Considerations

- **Parallel tool calls:** Agents can call independent tools in parallel (e.g., `get_preferences` + `get_pantry`)
- **Caching:** Backend caches `get_recipe` and `calculate_nutrition` in Redis
- **Async generation:** Meal plan generation is async. User gets "Generating your plan..." immediately, notification when done.
- **Timeout:** 120 seconds per workflow step. LLM calls typically 5-15s.
