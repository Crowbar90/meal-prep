# AI Meal Prep Planner

An AI-assisted weekly meal planning application with a deterministic C# backend and creative AI agents orchestrated by OpenClaw.

## Architecture Philosophy

- **AI is responsible for creativity** — inventing recipes, combining flavors, adapting to constraints.
- **C# backend is responsible for determinism** — nutrition calculations, budget math, validation, data persistence.
- **OpenClaw orchestrates the workflow** — agents collaborate through structured workflows triggered by Telegram.
- **The backend is always the source of truth.** AI agents never touch the database directly; they call MCP tools.

## Repository Structure

```
mealprep/
├── openclaw/             # OpenClaw agent definitions, prompts, and workflows
├── docs/                # Architecture documentation and ADRs
├── infrastructure/      # Docker, Kubernetes (Kustomize), Nix
├── src/                 # C# backend (Clean Architecture)
├── tests/               # Unit, integration, and agent approval tests
└── .github/workflows/   # CI/CD
```

## Quick Start (Local Development)

> **Design phase.** The domain model, the Aspire AppHost, and the persistence
> layer (`MealPrepPlanner.Dal`) exist today. Step 2 boots the dashboard and a
> Postgres 18 container; steps 3 and 4 activate once the API project lands.

```bash
# 1. Enter Nix shell (dependencies)
nix develop

# 2. Run the Aspire AppHost (opens the local dashboard + Postgres 18 container)
dotnet run --project src/MealPrepPlanner.AppHost

# 3. (Future) Run migrations
cd src && dotnet ef database update --project MealPrepPlanner.Dal --startup-project MealPrepPlanner.Dal

# 4. (Future) Run the API
cd src/Api && dotnet run

# 5. View the project board
#    https://github.com/users/Crowbar90/projects/2
#    Active iteration: "Cooking optimization (next)" — Pantry + Shopping/cost
#    are deferred; the next iteration focuses on the Meal Prep bounded context.

# 6. Run tests
dotnet test src/MealPrepPlanner.slnx
```

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Backend | C# 14, ASP.NET Core 10 |
| Local orchestration | .NET Aspire 13 (AppHost + ServiceDefaults) |
| Database | PostgreSQL 18 |
| Messaging | MassTransit (in-memory → RabbitMQ later) |
| Cache | Redis |
| AI Orchestration | OpenClaw (external, deployed separately) |
| LLM | GPT-4.1 / Claude (via OpenClaw) |
| Protocol | MCP (Model Context Protocol) |
| Deployment | Kubernetes (k3s), Kustomize, GitOps |
| Infrastructure | Proxmox, NixOS, Cloudflare Tunnel |

## Communication Patterns

```
Telegram Bot → OpenClaw Workflow → AI Agents → MCP Tools → Backend API
                                    ↓
                              REST API ← Web UI (admin)
```

- **OpenClaw → Backend**: MCP over HTTP/SSE
- **Frontend ↔ Backend**: REST/JSON
- **Backend internal**: MassTransit domain events

## Contributing

See `docs/architecture/` for detailed design documentation.
See `docs/decisions/` for Architecture Decision Records (ADRs).
See `docs/project-board.md` for the [`MealPrep Roadmap`](https://github.com/users/Crowbar90/projects/2) workflow and label conventions.

## License

MIT
