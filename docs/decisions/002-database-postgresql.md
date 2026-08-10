# ADR 002: Database — PostgreSQL

## Status
Accepted

## Context
We need a persistent store for relational data (recipes, meal plans, pantry, users) and semi-structured data (preferences, nutrition profiles).

## Decision
Use **PostgreSQL 16** as the primary database.

## Consequences

### Positive
- Mature, reliable, well-understood.
- Excellent JSONB support for semi-structured preferences and nutrition data.
- Full-text search for recipe search.
- GIN indexes for JSONB queries.
- CloudNativePG operator available for Kubernetes HA.

### Negative
- Requires schema migrations (managed by EF Core).
- Not as flexible as a document store for rapidly changing schemas.

## Alternatives Considered
- **MongoDB:** Better JSON flexibility, but weaker transactional guarantees and relational query performance. Rejected.
- **SQLite:** Good for local dev, not suitable for concurrent production use. Rejected.
- **Event Store (Marten):** See ADR 006.
