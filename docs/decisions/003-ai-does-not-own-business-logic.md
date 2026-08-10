# ADR 003: AI Does Not Own Business Logic

## Status
Accepted

## Context
AI models are creative but non-deterministic. They can hallucinate calorie counts, miscompute budgets, or violate hard constraints (allergies).

## Decision
AI agents are responsible for **creativity only**. The C# backend is responsible for **all deterministic work**.

## AI Responsibilities
- Inventing recipes
- Combining recipes
- Modifying recipes for constraints
- Adapting recipes to preferences
- Explaining decisions
- Asking clarifying questions
- Optimizing meal prep schedules

## Backend Responsibilities
- Calculating nutrition (calories, macros, micros)
- Validating nutrition against goals
- Scaling recipes
- Detecting conflicts (allergies, equipment, time)
- Generating shopping lists
- Estimating costs
- Storing and retrieving data
- Exposing MCP tools

## Consequences

### Positive
- Deterministic calculations are testable, reproducible, and auditable.
- AI cannot accidentally violate safety constraints (allergies).
- Clear separation of concerns.

### Negative
- More code to write in C#.
- AI agents must call tools instead of computing directly, adding latency.
- Requires careful API design between agents and backend.
