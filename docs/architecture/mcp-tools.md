# MCP Tools Catalog

This document catalogs all MCP (Model Context Protocol) tools exposed by the backend. These tools are the **only** interface between AI agents and the backend. Agents never access the database directly.

## Design Principles

1. **Many small tools, not few large ones.** Each tool has a single responsibility.
2. **Deterministic tools return facts.** AI tools return creative output.
3. **All tools validate input.** Never trust agent-provided IDs or data.
4. **All tools include `X-Workflow-Id` correlation.** Every call is traceable.
5. **Tools are idempotent where possible.** Especially write operations.

---

## Recipe Tools

### `search_recipes`

Search the recipe database by criteria.

**Input:**
```json
{
  "query": "lentil curry",
  "tags": ["vegan", "gluten-free"],
  "max_time_minutes": 45,
  "equipment": ["instant_pot"],
  "exclude_ingredients": ["nuts"],
  "limit": 10
}
```

**Output:**
```json
{
  "recipes": [
    {
      "id": "uuid",
      "name": "Red Lentil Dal",
      "description": "...",
      "prep_time_minutes": 10,
      "cook_time_minutes": 25,
      "tags": ["vegan", "gluten-free", "indian"],
      "nutrition_per_serving": { "calories": 320, "protein": 18, ... }
    }
  ],
  "total_count": 42
}
```

**Backend Logic:** Full-text search on PostgreSQL, filtered by tags and time.

---

### `get_recipe`

Retrieve a complete recipe by ID.

**Input:**
```json
{ "recipe_id": "uuid" }
```

**Output:** Full recipe object (see Recipe schema in data-model.md).

**Backend Logic:** Repository lookup with caching (Redis).

---

### `scale_recipe`

Scale a recipe to a target number of servings.

**Input:**
```json
{
  "recipe_id": "uuid",
  "target_servings": 4
}
```

**Output:** Scaled recipe with adjusted ingredient quantities.

**Backend Logic:** Deterministic linear scaling. Nutrition scales proportionally.

---

### `combine_recipes`

Merge two or more recipes into a single dish (e.g., "rice bowl with protein and veg").

**Input:**
```json
{
  "recipe_ids": ["uuid1", "uuid2"],
  "combination_strategy": "bowl",
  "target_servings": 2
}
```

**Output:** Combined recipe object.

**Backend Logic:** Aggregates ingredients, sums nutrition, concatenates instructions with ordering hints.

---

## Nutrition Tools

### `calculate_nutrition`

Calculate complete nutrition profile for a recipe or meal plan.

**Input:**
```json
{
  "recipe_id": "uuid",
  "servings": 2
}
```

**Output:**
```json
{
  "calories": 450,
  "protein": 22.5,
  "carbs": 55.0,
  "fat": 12.0,
  "fiber": 8.0,
  "sugar": 4.0,
  "sodium_mg": 320,
  "iron_mg": 3.2,
  "calcium_mg": 45,
  "vitamin_c_mg": 12
}
```

**Backend Logic:** Sums ingredient nutrition × quantity, divided by servings. Uses USDA food composition database.

---

### `validate_meal_plan_nutrition`

Check if a meal plan meets nutritional goals.

**Input:**
```json
{
  "meal_plan_draft": { /* MealPlan object */ },
  "nutrition_goals": {
    "calories_per_day": 2100,
    "protein_per_day": 120,
    "carbs_per_day": 250,
    "fat_per_day": 70
  }
}
```

**Output:**
```json
{
  "valid": true,
  "daily_results": [
    {
      "day": "monday",
      "calories": 2080,
      "protein": 125,
      "within_tolerance": true,
      "warnings": ["Fiber is slightly low"]
    }
  ],
  "overall_assessment": "Plan meets nutritional goals within 10% tolerance."
}
```

**Backend Logic:** Calls `calculate_nutrition` for each recipe, aggregates by day, compares to goals.

---

### `get_rda`

Get Recommended Daily Allowances for a household member.

**Input:**
```json
{ "household_member_id": "uuid" }
```

**Output:** RDA profile based on age, sex, weight, activity level.

**Backend Logic:** WHO/EFSA RDA tables.

---

## Shopping Tools

### `generate_shopping_list`

Create a shopping list from a meal plan, excluding pantry items.

**Input:**
```json
{
  "meal_plan_id": "uuid",
  "exclude_pantry_items": true
}
```

**Output:**
```json
{
  "items": [
    {
      "ingredient_id": "uuid",
      "name": "chicken breast",
      "quantity_needed": 800,
      "unit": "g",
      "quantity_to_buy": 1000,
      "unit_to_buy": "g",
      "already_in_pantry": false
    }
  ],
  "estimated_total_items": 15
}
```

**Backend Logic:** Aggregates ingredients across all recipes, subtracts pantry inventory, rounds to realistic package sizes.

---

### `estimate_cost`

Estimate shopping cost at specified supermarkets.

**Input:**
```json
{
  "shopping_list_id": "uuid",
  "supermarket_ids": ["uuid1", "uuid2"]
}
```

**Output:**
```json
{
  "estimates": [
    {
      "supermarket_id": "uuid",
      "supermarket_name": "Aldi",
      "total_cost": 42.30,
      "currency": "EUR",
      "item_breakdown": [...]
    }
  ],
  "cheapest": "uuid"
}
```

**Backend Logic:** Price database lookup. Falls back to average if price unknown.

---

### `find_substitutions`

Find ingredient substitutions for dietary constraints or availability.

**Input:**
```json
{
  "ingredient_id": "uuid",
  "dietary_restrictions": ["vegan", "gluten-free"],
  "max_price_multiplier": 1.5
}
```

**Output:**
```json
{
  "substitutions": [
    {
      "ingredient_id": "uuid",
      "name": "firm tofu",
      "nutrition_delta": { "protein": -2, "calories": -30 },
      "price_estimate": 2.50,
      "suitability_score": 0.92
    }
  ]
}
```

**Backend Logic:** Substitution matrix based on nutrition profile similarity + constraint compatibility.

---

## Pantry Tools

### `get_pantry`

Retrieve current pantry inventory for a household.

**Input:**
```json
{ "household_id": "uuid" }
```

**Output:**
```json
{
  "items": [
    {
      "ingredient_id": "uuid",
      "name": "red lentils",
      "quantity": 500,
      "unit": "g",
      "date_added": "2026-08-01",
      "expires_at": "2026-08-11",
      "status": "fresh"
    }
  ]
}
```

**Backend Logic:** Repository query, ordered by expiration date.

---

### `add_to_pantry`

Add an item to the pantry.

**Input:**
```json
{
  "household_id": "uuid",
  "ingredient_id": "uuid",
  "quantity": 1000,
  "unit": "g",
  "expires_at": "2026-09-01"
}
```

**Output:** `{ "pantry_item_id": "uuid" }`

**Backend Logic:** Upsert logic. If same ingredient exists, aggregates quantity.

---

## Meal Plan Tools

### `get_meal_plan`

Retrieve a meal plan by ID.

**Input:**
```json
{ "meal_plan_id": "uuid" }
```

**Output:** Full MealPlan object.

---

### `save_meal_plan_draft`

Persist a draft meal plan.

**Input:**
```json
{
  "household_id": "uuid",
  "draft": { /* MealPlan object */ },
  "workflow_id": "uuid",
  "metadata": {
    "generated_by_agents": ["meal_planner", "recipe_generator"],
    "generation_timestamp": "2026-08-10T10:00:00Z"
  }
}
```

**Output:**
```json
{
  "meal_plan_id": "uuid",
  "status": "draft",
  "url": "/mealplans/uuid"
}
```

**Backend Logic:** Validates structure, saves to `meal_plans` and `meal_slots` tables, logs decision event.

---

### `get_preferences`

Retrieve consolidated user preferences for a household.

**Input:**
```json
{ "household_id": "uuid" }
```

**Output:**
```json
{
  "household_id": "uuid",
  "household": { /* Household object */ },
  "members": [...],
  "dietary_restrictions": ["vegan", "nut-free"],
  "nutrition_goals": { "calories_per_day": 2100, "protein_per_day": 120 },
  "equipment": ["instant_pot", "air_fryer"],
  "max_cooking_time_minutes": 45,
  "weekly_budget": 60.00,
  "preferred_supermarkets": ["aldi", "lidl"],
  "food_preferences": {
    "liked_cuisines": ["italian", "indian"],
    "disliked_ingredients": ["cilantro", "mushrooms"]
  }
}
```

**Backend Logic:** Aggregates data from `households`, `preferences`, `household_members` tables.

---

## Prep Tools

These tools were added on 2026-08-16 as part of the cooking-optimization slice (#28). They follow ADR 003's split: `propose_*` is the agent's responsibility, `validate_*` is the backend's.

### `propose_prep_schedule`

Create a draft prep schedule from a finalized meal plan. Returns a contract-only stub; the actual draft is filled in by the Meal Prep Optimizer agent.

**Input:**
```json
{
  "meal_plan_id": "uuid",
  "preferences_id": "uuid"
}
```

**Output:**
```json
{
  "meal_plan": { /* summarized MealPlan snapshot */ },
  "prep_schedule": {
    "status": "draft",
    "tasks": []
  }
}
```

**Backend Logic:** This tool returns a JSON-Schema contract; the actual draft is filled in by the Meal Prep Optimizer agent.

---

### `validate_prep_schedule`

Determine whether a proposed prep schedule is feasible — equipment conflicts, time overlaps, food safety constraints.

**Input:**
```json
{
  "draft": { /* PrepSchedule object with Tasks */ },
  "preferences_id": "uuid"
}
```

**Output:**
```json
{
  "valid": false,
  "violations": [
    {
      "kind": "equipment_conflict",
      "message": "Oven required for two tasks at overlapping times",
      "taskId": "uuid",
      "suggestion": "Move 'Bake Lasagna' to Saturday 6pm"
    }
  ]
}
```

**Backend Logic:** Deterministic; no AI, no LLM. See ADR 003. Uses `PrepFeasibilityValidator` domain service.

**Cache:** 60s in-memory cache keyed by content hash of the draft + preferences.

---

### `save_prep_schedule`

Persist a validated prep schedule and its tasks in a single transaction.

**Input:**
```json
{
  "household_id": "uuid",
  "draft": { /* PrepSchedule object with Tasks */ },
  "workflow_id": "uuid",
  "metadata": {
    "generated_by_agents": ["meal_prep_optimizer"],
    "generation_timestamp": "2026-08-16T10:00:00Z"
  }
}
```

**Output:**
```json
{
  "prep_schedule_id": "uuid",
  "status": "finalized",
  "url": "/prep-schedules/uuid"
}
```

**Backend Logic:** Validates structure, saves to `prep_schedules` + `prep_tasks`, logs decision event.

---

## Validation Tools

### `validate_constraints`

Check if a draft meal plan violates any hard constraints.

**Input:**
```json
{
  "draft": { /* MealPlan object */ },
  "preferences": { /* Preferences object */ }
}
```

**Output:**
```json
{
  "valid": false,
  "violations": [
    {
      "severity": "error",
      "type": "allergy",
      "message": "Recipe 'Peanut Noodles' contains peanuts, violating household nut allergy.",
      "day": "tuesday",
      "meal": "dinner",
      "recipe_id": "uuid"
    }
  ]
}
```

**Backend Logic:** Cross-references recipe ingredients against household dietary restrictions.

---

## Tool Implementation Notes

### Error Handling

All tools return structured errors:

```json
{
  "error": true,
  "error_code": "RECIPE_NOT_FOUND",
  "message": "Recipe with ID xyz not found",
  "suggestion": "Use search_recipes to find similar recipes"
}
```

### Rate Limiting

- `calculate_nutrition`: 100/min (CPU-bound)
- `search_recipes`: 200/min (DB-bound)
- `estimate_cost`: 50/min (external API)

### Caching

- `get_recipe`: 5 minutes (Redis)
- `calculate_nutrition`: 1 hour (deterministic, safe to cache)
- `get_preferences`: 1 minute (may change frequently)

### Security

- All tools validate the `household_id` against the authenticated user
- `X-Workflow-Id` is logged for audit but not used for authorization
- Write tools (`save_meal_plan_draft`, `add_to_pantry`) require explicit user confirmation for production data
