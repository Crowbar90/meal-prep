# OpenClaw Agents

This directory contains all agent definitions, prompts, and workflow configurations for OpenClaw.

> **Important:** OpenClaw itself is deployed **externally** (on a separate Kubernetes namespace or host). This directory is a configuration package that OpenClaw consumes via Git submodule, ConfigMap, or volume mount.

## Directory Layout

```
agents/
├── workflows/              # OpenClaw workflow definitions (YAML)
├── _shared/                # Shared prompts, tool configs, utilities
└── <agent-name>/           # One directory per agent
    ├── agent.yaml          # Agent manifest (model, role, skills)
    └── prompts/            # Agent-specific prompt files
```

## Adding a New Agent

1. Create a directory: `agents/<agent-name>/`
2. Write `agent.yaml` following the schema below
3. Add prompt files in `prompts/`
4. Register the agent in the relevant workflow under `workflows/`
5. Add MCP tool permissions in `agent.yaml` → `skills`

## Agent Manifest Schema

```yaml
name: string           # Unique agent identifier
role: string           # Human-readable role description
model: string          # LLM model identifier (e.g., gpt-4.1, claude-3-5-sonnet)
prompts:
  system: string       # Path to system prompt file (relative to agent dir)
temperature: float     # 0.0-2.0 (0.7 for creativity, 0.2 for review tasks)
max_tokens: int        # Maximum response tokens
skills:                # MCP tools this agent is allowed to call
  - tool_name_1
  - tool_name_2
memory:
  enabled: bool        # Whether agent persists context across sessions
  scope: string       # "workflow" | "session" | "global"
```

## Workflow Schema

```yaml
name: string
trigger:
  type: string         # "event" | "schedule" | "webhook"
  source: string       # "telegram.command" | "telegram.message" | "cron"
  filters: object      # Trigger-specific filters

agents: object         # Map of agent aliases → agent definitions

workflow:
  - id: string         # Unique step ID
    run: string        # "agent.<name>" | "tool.<name>"
    input: object      # Step inputs (templated)
    output: string     # Variable name to capture output
    condition: string  # Optional: conditional execution (templated)
    retries: int       # Optional: override default retries

policies:
  retries:
    max_attempts: int
    backoff: string    # "exponential" | "linear" | "fixed"
  timeout: int         # Seconds per step

logging:
  level: string        # "DEBUG" | "INFO" | "WARN" | "ERROR"
  include:             # Fields to include in structured logs
    - step_id
    - agent_decision
    - tool_response
    - reasoning
    - execution_time
```

## Environment-Specific Configuration

OpenClaw should inject these environment variables when loading agents:

| Variable | Description |
|----------|-------------|
| `MCP_SERVER_URL` | Backend MCP server endpoint |
| `MCP_API_KEY` | Authentication key for MCP server |
| `OPENCLAW_WORKFLOW_ID` | Unique ID for the current workflow execution |
| `OPENCLAW_AGENT_NAME` | Name of the agent making the call (sent as `X-Agent-Name`) |
| `BACKEND_API_URL` | REST API base URL (for non-MCP admin tasks) |

## OpenClaw-Native Tools

`tool.send_telegram_message` in `workflows/*.yaml` is an **OpenClaw-native tool**, not a backend MCP tool — it is intentionally absent from `docs/architecture/mcp-tools.md`. Do not add it there. Only backend-exposed tools belong in the MCP catalog.

## Correlation IDs

Every MCP tool call must include the header `X-Workflow-Id` set to the current OpenClaw workflow execution ID. This allows the backend to correlate decisions across the entire pipeline.

## Prompt Engineering Guidelines

1. **Always include the output schema** — agents must produce structured JSON
2. **Include few-shot examples** for recipe generation quality
3. **Add a reflection step** — agent critiques its own output before returning
4. **Never include secrets or API keys** in prompts
5. **Reference MCP tools explicitly** — tell the agent which tools are available and when to use them
