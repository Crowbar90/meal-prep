# ADR 002: Database — PostgreSQL

## Status
Accepted (bumped 16 → 18 in the persistence-layer slice; see Notes).

## Context
We need a persistent store for relational data (recipes, meal plans, pantry, users) and semi-structured data (preferences, nutrition profiles).

## Decision
Use **PostgreSQL 18** as the primary database.

## Consequences

### Positive
- Mature, reliable, well-understood.
- Excellent JSONB support for semi-structured preferences and nutrition data.
- Full-text search for recipe search.
- GIN indexes for JSONB queries.
- CloudNativePG operator available for Kubernetes HA.
- Postgres 18 ships improved logical replication and the built-in `uuidv7()` function, available if we ever need time-ordered UUIDs (we don't today — see Notes).

### Negative
- Requires schema migrations (managed by EF Core).
- Not as flexible as a document store for rapidly changing schemas.

## Alternatives Considered
- **MongoDB:** Better JSON flexibility, but weaker transactional guarantees and relational query performance. Rejected.
- **SQLite:** Good for local dev, not suitable for concurrent production use. Rejected.
- **Event Store (Marten):** See ADR 006.

## Notes

### Bump 16 → 18 (2026)

The original ADR accepted Postgres 16. The persistence-layer slice (`MealPrepPlanner.Dal`) targets 18 for two reasons:

1. **Aspire 13.4.6's bundled image tag tracks the latest stable major.** Pinning to `18` in `MealPrepPlanner.AppHost/Program.cs` (`WithImageTag("18")`) keeps the local-development container aligned with the long-term target.
2. **EF Core 10 + Npgsql 10.0.3 work cleanly against PG 18.** No provider-version pinning required.

### Why `BIGSERIAL` for `decision_events.id` and `ai_execution_logs.id`

Per `docs/architecture/data-model.md` these audit tables use `BIGSERIAL` PKs rather than UUIDv7. The reasoning:

- All writes funnel into one Postgres sequence, even when the backend runs as multiple replicas behind a load balancer (the DB itself is the single source of truth — see ADR 003). This is **not** a distributed-id scenario, so UUIDv7's main benefit (cross-writer uniqueness without coordination) doesn't apply.
- `BIGSERIAL` is 8 bytes vs 16 for UUIDv7; smaller indexes on FKs (`decision_events.parent_decision_id`) and join columns.
- Ordered inserts → B-tree leaf doesn't split until wraparound; hot page stays in cache.
- `decision_events.sequence_number` (caller-supplied per ADR 005) provides the workflow-ordering guarantee that audit readers actually query on; the `id` is just an internal PK.

A future slice that introduces independent audit writers (active-active multi-region, serverless functions writing directly) should revisit this with UUIDv7 + `pg_partman`. Tracked separately; not pursued now.