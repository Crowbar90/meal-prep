# ADR 004: Many Small MCP Tools

## Status
Accepted

## Context
We need a protocol for AI agents to interact with the backend. Options: single monolithic tool (`GenerateMealPlan`) or many small tools.

## Decision
Expose **many small, single-purpose MCP tools** rather than a few large ones.

## Tool Examples
- `SearchRecipes()`, `GetRecipe()`, `ScaleRecipe()`
- `CalculateNutrition()`, `ValidateMealPlanNutrition()`
- `GenerateShoppingList()`, `EstimateCost()`
- `FindIngredientSubstitutions()`
- `GetPantry()`, `GetPreferences()`
- `SaveMealPlanDraft()`

## Consequences

### Positive
- Composability: Agents combine tools in unanticipated ways.
- Testability: Each tool is independently unit testable.
- Observability: Know exactly which tool failed.
- Caching: `GetRecipe` and `CalculateNutrition` are highly cacheable.
- Security: Fine-grained permissions per tool.

### Negative
- More API surface area to maintain.
- Agents may make many sequential calls (mitigated by parallel calls and caching).
- Requires good documentation (see `docs/architecture/mcp-tools.md`).
