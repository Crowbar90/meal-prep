# AGENTS.md

## Repo state (important)

Design-phase repo — **no app code or tests exist yet** (no `.cs`, `.csproj`, or `.sln` files). No CI. What exists:

- `agents/` — OpenClaw agent manifests, prompts, and workflow YAMLs (the only functional deliverable)
- `src/Directory.Build.props` — C# build config only; pins `net10.0` / C# 14 / warnings-as-errors for all future projects
- `docs/architecture/` and `docs/decisions/` — design docs and ADRs (source of truth for planned behavior)
- `.editorconfig` — formatting: `.cs` 4-space, YAML/JSON/MD 2-space
- `README.md` — describes the *aspirational* full stack; `tests/`, `infrastructure/`, `.github/`, and app projects are planned but do not exist. Its quick-start commands (`nix develop`, `docker compose`, `dotnet ef`, `dotnet run`) cannot run yet.

There are **no commits yet**. There is no lint/test/build/typecheck command to run — `dotnet build` has no projects to build.

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
