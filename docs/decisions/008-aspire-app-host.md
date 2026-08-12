# ADR 008: .NET Aspire for Local Orchestration and End-to-End Tests

## Status
Accepted

## Context
The repo is currently in the design phase: only the pure-domain project and an xUnit suite exist. No API project, no Infrastructure project, no containers — yet. The README already lists PostgreSQL 16, Redis, MassTransit, MCP over HTTP/SSE, and Kubernetes as the aspirational full stack.

We need two things before that stack exists:

1. **Local development.** Today the README points developers at `nix develop`, `docker compose -f infrastructure/docker/docker-compose.yml up -d`, and `dotnet ef database update`. None of those run yet because the infrastructure isn't real. We need a single command that boots the future stack on a developer's machine and exposes a dashboard for logs, traces, and endpoints.

2. **End-to-end tests.** When the API and Infrastructure projects land, we want a way to spin up the full dependency graph inside the test process so we can exercise it without mocks. This is a near-future need, not a today need.

`docker compose` and Kubernetes manifests will still own production and shared environments; we want a developer-machine story that complements them without duplicating their authoring effort.

## Decision
Adopt **.NET Aspire 13.4.x** as the local-development and end-to-end-test orchestration layer. The scaffolding lives in two projects under `src/`:

- `src/MealPrepPlanner.ServiceDefaults/` — class library (`IsAspireSharedProject=true`) holding the canonical `AddServiceDefaults()` / `MapDefaultEndpoints()` extension methods. Future service projects (e.g. `MealPrepPlanner.Api`) reference this and call `AddServiceDefaults()` from their `Program.cs`. Pinned package versions: `Microsoft.Extensions.Http.Resilience` 10.9.0, `Microsoft.Extensions.ServiceDiscovery` 10.9.0, OpenTelemetry instrumentation 1.17.0, runtime instrumentation 1.12.0.
- `src/MealPrepPlanner.AppHost/` — Aspire AppHost (`Aspire.AppHost.Sdk` 13.4.6, `Aspire.Hosting.AppHost` 13.4.6). Today it builds and runs an empty orchestration graph (`builder.Build().Run()`); no resources registered. Future runnable services will be added with `builder.AddProject<Projects.X>("x")` and wired together with the `WithReference()` chain.

The AppHost **does not** reference the Domain or ServiceDefaults projects today. Once a service project lands (API, Infrastructure), the AppHost adds a `<ProjectReference>` and the SDK auto-generates a strongly-typed `Projects.<Name>` type for the orchestrator to consume.

This ADR does **not** commit to:
- Wiring PostgreSQL 16 or Redis into the AppHost — that belongs to a future ADR when the persistence stack lands.
- Writing Aspire-based end-to-end tests — those live in a future `tests/MealPrepPlanner.Aspire.Tests/` project; deferred until at least one runnable service exists to test against.
- Replacing the planned Kubernetes / Kustomize / GitOps deployment. Aspire is local-dev only; production stays on K8s.

## Consequences

### Positive
- Single command (`dotnet run --project src/MealPrepPlanner.AppHost`) brings up the future stack with a dashboard for logs, traces, env vars, and endpoints — no manual docker-compose.
- Service discovery and resilience handlers are pre-wired in `AddServiceDefaults()`, so new services inherit OpenTelemetry, health checks (`/health`, `/alive`), and standard HTTP resilience without re-deciding each time.
- `Aspire.Hosting.Testing` (already on nuget.org, 7M+ downloads) drops in for end-to-end tests later — no new test framework.
- Same project file, language, and toolchain as the rest of the backend; no polyglot scripting, no separate docker-compose YAML to maintain for dev.

### Negative
- Two new projects and ~7 NuGet packages even though nothing runs yet. The cost is small but real and should be justified again if the AppHost grows beyond local-dev.
- Pinned Aspire versions (`13.4.6`) require periodic bumps to stay on a supported channel.
- `Aspire.Hosting.AppHost` transitively pulls in Kubernetes manifests tooling (`KubernetesClient`), gRPC, and protobuf — heavy for what is currently an empty orchestrator. Acceptable for now; revisit if it bloats developer-machine restore time materially.
- Aspire is a moving target (Microsoft-shipped, but still relatively young). Long-term support for any specific 13.x version is not guaranteed the way ASP.NET Core LTS is. We accept the risk because the AppHost is local-only and a rewrite to a different orchestrator would be isolated to two projects.

## Alternatives Considered
- **`docker compose` only.** Reuse the production-shaped compose file. Rejected because we don't have a compose file yet and we want telemetry + service discovery baked in for free; rewriting it as Aspire resources later would be wasted work.
- **Microsoft Tye (predecessor to Aspire).** Rejected: Tye is in maintenance mode and Microsoft has directed users to Aspire since v8.
- **Custom scripts + `dotnet run` per project.** Rejected: no shared telemetry, no dashboard, no service-discovery glue. The cost would dwarf the cost of adopting Aspire.
- **Skip the AppHost until the API lands.** Tempting (nothing runs today), but the Aspire scaffolding is cheap and forces a decision about project layout and `ServiceDefaults` shape before the API project makes assumptions that are hard to undo.

## Notes
- On NixOS, the Aspire AppHost ships native binaries (the `dcp` orchestrator and the dashboard) that are dynamically linked against `glibc`. To run them, enable `programs.nix-ld.enable = true` in the NixOS configuration. See `https://nix.dev/permalink/stub-ld` for the canonical fix.