# AGENTS.md

## Repo state (important)

Design-phase repo — **no app code beyond the domain model, and no CI yet**. What exists:

- `agents/` — OpenClaw agent manifests, prompts, and workflow YAMLs (functional deliverable)
- `src/MealPrepPlanner.Domain/` — pure C# domain project (`MealPrepPlanner.Domain.csproj`); aggregates, value objects, domain events, and deterministic services only, zero external dependencies
- `tests/MealPrepPlanner.Tests/` — unified xUnit v3 suite (single suite for unit + future integration tests); `Unit/` now, `Integration/` later, same project
- `src/MealPrepPlanner.slnx` — solution file (XML `.slnx`); references the domain and test projects
- `src/Directory.Build.props` — C# build config; pins `net10.0` / C# 14 / warnings-as-errors for all projects under `src/` (tests import it via `tests/Directory.Build.props`)
- `docs/architecture/` and `docs/decisions/` — design docs and ADRs (source of truth for planned behavior)
- `.editorconfig` — formatting: `.cs` 4-space, YAML/JSON/MD 2-space; forbids trailing commas on the last element of multiline lists
- `.gitignore` — ignores .NET build output (`bin/`, `obj/`), IDE/OS files
- `README.md` — describes the *aspirational* full stack; `infrastructure/`, `.github/`, and web/app projects are planned but do not exist. Its quick-start commands (`nix develop`, `docker compose`, `dotnet ef`, `dotnet run`) cannot run yet.

Build and test:

```sh
dotnet build src/MealPrepPlanner.slnx
dotnet test  src/MealPrepPlanner.slnx
```

The test project is an MTP executable (`xunit.v3`); it needs `DOTNET_ROOT` exported on this NixOS machine or the native apphost cannot find the runtime. There is no lint/typecheck command beyond `dotnet format` (reliable for whitespace, but it silently skips the two .editorconfig IDE style rules — verify those manually when editing).

## OpenClaw agents (`agents/`)

OpenClaw runs **externally** (separate host/namespace); this directory is a config package it consumes via submodule/ConfigMap/volume mount. Per-agent layout:

```
agents/
├── workflows/<name>.yaml   # workflow definitions
├── _shared/                # shared prompts (system-base.txt), MCP client config
└── <agent-name>/
    ├── agent.yaml          # manifest (name, model, temperature, max_tokens, skills, memory)
    └── prompts/system.txt  # agent-specific system prompt
```

Conventions (see `agents/README.md` for the full schema):

- **Agents never do math or touch the DB.** All calculations, validation, and persistence go through MCP tools (e.g. `calculate_nutrition`, `validate_constraints`, `save_meal_plan_draft`). Prompts must enforce this.
- Agents must emit **structured JSON with a `reasoning` field**; include output schemas and few-shot examples in prompts.
- Agent names in a workflow's `agents:` map use snake_case aliases (`meal_planner`, not `meal-planner`).
- All agent prompt files should build on the shared base at `agents/_shared/prompts/system-base.txt`.
- The MCP tool catalog lives in `docs/architecture/mcp-tools.md` — keep tool names referenced in `agent.yaml` `skills:`, `workflows/*.yaml`, and prompt files in sync with it.
- Workflow steps reference other steps via `{{steps.<id>.output}}`; conditionals use `{{steps.<id>.output.approved == false}}` style templates.
- Adding an agent: create `agents/<name>/` with `agent.yaml` + `prompts/`, register it in the relevant workflow, add its MCP skills.
- OpenClaw injects env vars: `MCP_SERVER_URL`, `MCP_API_KEY`, `OPENCLAW_WORKFLOW_ID`, `OPENCLAW_AGENT_NAME`, `BACKEND_API_URL`. Every MCP call must carry header `X-Workflow-Id` (plus `X-Agent-Name` from `mcp-client-config.yaml`).

## Architecture invariants (for any new code/docs)

- Backend is the **source of truth**; AI agents are creative-only and call MCP tools, never the DB directly.
- Determinism (nutrition, budget, validation) belongs to the C# backend; creativity belongs to agents.
- Backend targets **C# 14 / .NET 10 (ASP.NET Core 10)**, pinned in `src/Directory.Build.props` (`net10.0`, `LangVersion 14`, Nullable, ImplicitUsings, `TreatWarningsAsErrors`). Any project added under `src/` inherits this automatically.
- Planned but unimplemented stack: PostgreSQL 16, Redis, MassTransit, MCP over HTTP/SSE.
- `docs/architecture/` (bounded-contexts, data-model, mcp-tools, ai-orchestration, communication-patterns) and `docs/decisions/` ADRs (001–007) are the contract for future implementation. Keep them in sync when you change related concepts.
