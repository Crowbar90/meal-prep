# ADR 006: No Event Sourcing (Marten)

## Status
Accepted

## Context
We need to reconstruct why meal plans were generated. We considered full event sourcing with Marten vs. an append-only Decision Stream.

## Decision
Use a **relational model with an append-only Decision Stream table**, not full event sourcing.

## Rationale
- Full event sourcing (Marten) adds significant complexity: projections, schema evolution, upcasters, rebuilds.
- Our primary need is **causality reconstruction** (why did an agent make a choice?), not **temporal state queries** (what did the plan look like at time T?).
- The Decision Stream (`decision_events` table) captures: who decided, what was the input context, what was output, and why.
- Meal plan versions are handled by simple versioning (`meal_plans.version`).

## What We Get
- Full audit trail of agent decisions.
- Ability to query causal chains by `workflow_id`.
- Structured reasoning for every decision.

## What We Give Up
- Cannot replay a workflow with modified inputs (what-if analysis).
- Cannot query aggregate state at arbitrary historical timestamps.

## Revisit If
- We need temporal queries as a core user feature.
- Debugging requires seeing exact plan state at step 3 of generation.
- Team grows and can support Marten complexity.

## Consequences

### Positive
- Lower complexity. Team can onboard quickly.
- Standard PostgreSQL tooling (backups, migrations, queries).
- No projection maintenance.

### Negative
- Decision Stream is a log, not a source of truth for current state.
- Cannot reconstruct historical state without replaying logs manually.
