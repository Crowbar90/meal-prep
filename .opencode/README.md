# Development Agents (`.opencode/`)

Development agents for opencode — distinct from the **OpenClaw runtime agents** in `openclaw/`. These agents help you build this repository; OpenClaw agents are the product's runtime AI, deployed externally.

## Setup

- `opencode.json` sets `coder` as the **default primary agent**, so every new session starts talking to it.
- `.editorconfig` is injected into every session's context via `instructions` — do not restate formatting rules in prompts.
- Subagents run on opencode Zen free models (`opencode/<model>`). Free models churn — treat model IDs as swappable.

## `coder` (primary, default)

Standard coding agent. Full tool access. Front-loads repo invariants (read AGENTS.md + docs/ first, ADRs final → ask on conflict) and delegates specialized work to the subagents below whenever possible. Runs on `opencode/deepseek-v4-flash-free`.

## Subagents (`agents/`)

| Agent | Model | When to use |
|-------|-------|-------------|
| `domain-modeler` | `opencode/deepseek-v4-flash-free` | Scaffold a new DDD aggregate (entities, value objects, domain events) + tests, mirroring `docs/architecture/data-model.md` and existing aggregates |
| `test-writer` | `opencode/deepseek-v4-flash-free` | Write or extend xUnit v3 tests mirroring `tests/Unit/`, then run `dotnet test` |
| `code-reviewer` | `opencode/deepseek-v4-flash-free` | Read-only review of a change against `docs/` invariants + best practices |
| `docs-compliance-checker` | `opencode/big-pickle` | Verify an implementation matches `docs/` (architecture + decisions); report drift |
| `docs-writer` | `opencode/big-pickle` | Create or amend `docs/`, `README.md`, `AGENTS.md` (propose-then-apply) |
| `prompt-maintainer` | `opencode/big-pickle` | Keep agent prompts and the MCP tool catalog (`docs/architecture/mcp-tools.md`) in sync |
| `git-helper` | `opencode/big-pickle` | Draft commit messages and PR descriptions from `git diff`/`git log`; never edits code |

Invoke a subagent with `@name`, or let `coder` delegate via the Task tool.
