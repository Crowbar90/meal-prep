# Project Board: MealPrep Roadmap

This repo is tracked on the **`MealPrep Roadmap`** Projects v2 board (user-level, owner `Crowbar90`). It uses GitHub's native flow rather than a custom Jira-style board; this doc captures the conventions so any contributor can keep the board consistent.

**Board URL:** <https://github.com/users/Crowbar90/projects/2>

## Status (GitHub-native)

Use the default `Status` field exactly as GitHub ships it. No custom workflow field — Jira-style columns (Backlog / Ready / In Review / etc.) are out of scope.

| Status | Meaning |
| --- | --- |
| **Todo** | Triaged. Has labels, an assignee if appropriate, and a slice/component. Ready to pick up. |
| **In Progress** | Someone is actively working it. A branch and PR should exist or be imminent. |
| **Done** | PR merged and the worktree + branch cleaned up per `AGENTS.md § Workflow`. |

PRs auto-link from the branch name (`feat/<issue>-<slug>`) via the closing keyword in the PR body (e.g. `Closes #5`). The `Linked pull requests` field on each issue fills in once the PR opens.

## Sub-issues

Epics (`Slice N — ...`) are parent issues. Each child issue (`<epic-id>.<n>`) is added as a **sub-issue** of the epic, so the board view groups them and the `Sub-issues progress` field tracks completion automatically.

## Labels

In addition to GitHub's defaults (`bug`, `enhancement`, `documentation`, etc.), this repo adds:

### Slice

| Label | Description |
| --- | --- |
| `slice:dal` | Slice 2 follow-on — DAL hardening (Aspire migration runner + integration test) |
| `slice:webapi` | Slice 3 — MealPrepPlanner.Api (ASP.NET Core 10 REST API) |
| `slice:mcp` | Slice 4 — MealPrepPlanner.Mcp (MCP server for AI agents) |

Every issue carries exactly one `slice:*` label so the board view can filter by slice.

### Component

| Label | Description |
| --- | --- |
| `component:dal` | Data access / EF Core / migrations |
| `component:api` | ASP.NET Core REST endpoints / controllers / middleware |
| `component:mcp` | MCP server / tools / transport |
| `component:infra` | Build pipeline, Aspire AppHost, ServiceDefaults, container wiring |
| `component:docs` | `docs/`, `README.md`, `AGENTS.md` updates |
| `component:tests` | xUnit v3 tests |
| `component:auth` | Authentication / authorization / API keys |

An issue can carry one or more `component:*` labels.

## Workflow

1. **Triage.** Open the issue, add the `slice:*` label and one or more `component:*` labels, set Status to `Todo`.
2. **Pick up.** Move Status to `In Progress` when you create the worktree (`feat/<issue>-<slug>`).
3. **Review.** The PR auto-links; leave Status as `In Progress` until it merges.
4. **Done.** Once the PR merges, the worktree + branch are cleaned up (see `AGENTS.md § Workflow`). Status flips to `Done` manually.

## Why no custom fields

The `coder` and `docs-writer` agents don't have a generic "create single-select field" verb available in the GitHub MCP; iteration fields are the only new-field option. Until we hit a reason that the default Status + Labels combo isn't enough (e.g. story-point estimation across many contributors), we keep the board lean and use GitHub's defaults.
