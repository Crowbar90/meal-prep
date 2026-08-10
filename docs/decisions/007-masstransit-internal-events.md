# ADR 007: MassTransit for Internal Domain Events

## Status
Accepted

## Context
We need asynchronous communication between backend bounded contexts (MealPlanning → Shopping, Pantry → Notifications).

## Decision
Use **MassTransit with in-memory transport** initially. Migrate to RabbitMQ when horizontal scaling requires it.

## Why MassTransit Over MediatR
- MediatR is in-process only and hides dependencies.
- MassTransit makes messaging explicit and supports future distribution.
- We dislike the "magic" of MediatR's service locator pattern.

## Configuration
- Start with `UsingInMemory()` for single-instance deployment.
- Switch to `UsingRabbitMq()` when multiple backend replicas are needed.
- Use PostgreSQL saga repository if saga state must survive restarts.

## Consequences

### Positive
- Explicit messaging. Dependencies are visible in code.
- Easy migration path from in-memory to RabbitMQ.
- Built-in saga support for long-running backend processes.

### Negative
- In-memory transport does not survive process restarts.
- Adding RabbitMQ later requires infrastructure changes.
- More ceremony than MediatR for simple in-process events.
