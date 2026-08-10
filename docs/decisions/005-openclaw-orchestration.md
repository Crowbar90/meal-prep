# ADR 005: OpenClaw Orchestrates Meal Generation Workflows

## Status
Accepted

## Context
Meal plan generation is a long-running, multi-agent workflow. We considered MassTransit Sagas vs. OpenClaw's built-in workflow engine.

## Decision
**OpenClaw orchestrates the meal generation workflow.** MassTransit is used only for backend-internal domain events.

## Rationale
- OpenClaw has native workflow primitives: sequential, parallel, hierarchical, feedback-loop.
- OpenClaw manages agent lifecycle, retries, and state persistence.
- Telegram is the primary entry point; OpenClaw has native Telegram triggers.
- OpenClaw provides structured logging with reasoning traces out of the box.
- Using MassTransit sagas for the same flow would create a "split-brain" where workflow state lives in two systems.

## MassTransit Remains For
- Backend-internal domain events (`MealPlanFinalized` → Shopping list generation)
- Cross-context notifications
- Future horizontal scaling of backend consumers

## Consequences

### Positive
- Single orchestrator with clear ownership.
- Native Telegram integration.
- Declarative workflow definitions in YAML.
- Unified structured logging for causality reconstruction.

### Negative
- OpenClaw is a separate system to deploy and monitor.
- Workflow definitions live in YAML (less type safety than C#).
- Debugging requires understanding both OpenClaw and backend logs.
