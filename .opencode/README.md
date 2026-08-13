# Development Agents (`.opencode/`)

Development agents for opencode — distinct from the **OpenClaw runtime agents** in `openclaw/`. These agents help you build this repository; OpenClaw agents are the product's runtime AI, deployed externally.

## Setup

- `opencode.json` sets `coder` as the **default primary agent**, so every new session starts talking to it.
- `.editorconfig` is injected into every session's context via `instructions` — do not restate formatting rules in prompts.
- Models run on **OpenCode Go** (`opencode-go/`): $12 per 5h, $30/week, $60/month pooled. Free Zen models remain available as fallback. All Go models are not trained on your data and have 0-day retention (except Grok 4.5 and GPT 5.6 Luna, which have 30-day).

## `coder` (primary, default)

Standard coding agent. Full tool access. Front-loads repo invariants (read AGENTS.md + docs/ first, ADRs final → ask on conflict) and delegates specialized work to the subagents below whenever possible. Runs on `opencode-go/minimax-m3`. Does not run `git` or `gh` commands (delegates them to `git-helper`).

## Subagents (`agents/`)

| Agent | Model | When to use |
|-------|-------|-------------|
| `domain-modeler` | `opencode-go/deepseek-v4-flash` | Scaffold a new DDD aggregate (entities, value objects, domain events) + tests, mirroring `docs/architecture/data-model.md` and existing aggregates |
| `test-writer` | `opencode-go/deepseek-v4-flash` | Write or extend xUnit v3 tests mirroring `tests/Unit/`, then run `dotnet test` |
| `code-reviewer` | `opencode-go/deepseek-v4-flash` | Read-only review of a change against `docs/` invariants + best practices |
| `docs-compliance-checker` | `opencode-go/mimo-v2.5` | Verify an implementation matches `docs/` (architecture + decisions); report drift |
| `docs-writer` | `opencode-go/mimo-v2.5` | Create or amend `docs/`, `README.md`, `AGENTS.md` (propose-then-apply) |
| `prompt-maintainer` | `opencode-go/mimo-v2.5` | Keep agent prompts and the MCP tool catalog (`docs/architecture/mcp-tools.md`) in sync |
| `git-helper` | `opencode-go/mimo-v2.5` | Draft commit messages and PR descriptions, runs `git` for inspection and for write commands, asks before writes (commit, push, reset). Also runs `gh` (GitHub CLI) for interaction with GitHub (such as opening PRs).|

Invoke a subagent with `@name`, or let `coder` delegate via the Task tool.

## Workflow

All non-planning work happens on a git worktree under `.worktrees/`, branched off `main`, PR'd back to `main`. See `AGENTS.md § Workflow` for the full rule. The `git-helper` agent owns the worktree lifecycle commands; `coder` and the other agents delegate to it.

## GitHub integration

Default to the GitHub MCP (`mcp__github__*`) for all GitHub operations — never `gh` from agents. The board for this repo is the `MealPrep Roadmap` Projects v2 project (user-level, owner `Crowbar90`). Open all issues and PRs through the MCP so the board updates automatically.
