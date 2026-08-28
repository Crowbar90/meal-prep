# Development Agents (`.opencode/`)

Development agents for opencode — distinct from the **OpenClaw runtime agents** in `openclaw/`. These agents help you build this repository; OpenClaw agents are the product's runtime AI, deployed externally.

## Setup

- `opencode.json` sets `build` (opencode's built-in default primary agent) as the primary, so every new session starts talking to it.
- `.editorconfig` is injected into every session's context via `instructions` — do not restate formatting rules in prompts.
- Models run on **OpenCode Go** (`opencode-go/`): $12 per 5h, $30/week, $60/month pooled. Free Zen models remain available as fallback. All Go models are not trained on your data and have 0-day retention (except Grok 4.5 and GPT 5.6 Luna, which have 30-day).

## Subagents (`agents/`)

| Agent | Model | When to use |
|-------|-------|-------------|
| `domain-modeler` | `opencode-go/deepseek-v4-flash` | Scaffold a new DDD aggregate (entities, value objects, domain events) + tests, mirroring `docs/architecture/data-model.md` and existing aggregates |
| `test-writer` | `opencode-go/deepseek-v4-flash` | Write or extend xUnit v3 tests mirroring `tests/Unit/`, then run `dotnet test` |
| `code-reviewer` | `opencode-go/deepseek-v4-flash` | Read-only review of a change against `docs/` invariants + best practices |
| `docs-compliance-checker` | `opencode-go/mimo-v2.5` | Verify an implementation matches `docs/` (architecture + decisions); report drift |
| `docs-writer` | `opencode-go/mimo-v2.5` | Create or amend `docs/`, `README.md`, `AGENTS.md` (propose-then-apply) |
| `prompt-maintainer` | `opencode-go/mimo-v2.5` | Keep agent prompts and the MCP tool catalog (`docs/architecture/mcp-tools.md`) in sync |

Invoke a subagent with `@name`. The primary `build` agent has full tool access and delegates to these specialists.

## GitHub integration

Default to the GitHub MCP (`mcp__github__*`) for all GitHub operations. The board for this repo is the `MealPrep Roadmap` Projects v2 project (user-level, owner `Crowbar90`). Open all issues and PRs through the MCP so the board updates automatically.
