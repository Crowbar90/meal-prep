# Data Model

This document describes the core database schema for the AI Meal Prep Planner. The database is **PostgreSQL** with a relational model. JSONB is used sparingly for semi-structured data.

## Design Principles

1. **Normalize core entities.** Recipes, ingredients, households have strict schemas.
2. **Use JSONB for extensible data.** Nutrition profiles, preferences, and equipment lists vary.
3. **Append-only audit tables.** Decision events and AI execution logs are never updated.
4. **Version meal plans.** Each finalized plan creates a new version row.

---

## Entity Relationship Diagram

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   households    │────►│     members     │     │   preferences   │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                                               │
         │                                               │
         ▼                                               ▼
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│  meal_plans     │◄────│  meal_slots     │     │ pantry_items    │
└─────────────────┘     └─────────────────┘     └─────────────────┘
         │                       │
         │                       ▼
         │               ┌─────────────────┐
         │               │     recipes     │
         │               └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│ shopping_lists  │◄────│recipe_ingredients │
└─────────────────┘     └─────────────────┘
         │                       │
         ▼                       ▼
┌─────────────────┐     ┌─────────────────┐
│shopping_list_items│    │   ingredients   │
└─────────────────┘     └─────────────────┘
         │
         ▼
┌─────────────────┐
│  supermarkets   │
└─────────────────┘
```

---

## Core Tables

### `households`

The top-level aggregate for a group of people sharing meals.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `name` | VARCHAR(100) | NOT NULL | Household name |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | Creation timestamp |
| `updated_at` | TIMESTAMPTZ | DEFAULT NOW() | Last update timestamp |

### `household_members`

Individuals within a household with their own nutritional needs.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `household_id` | UUID | FK → households | Parent household |
| `name` | VARCHAR(100) | NOT NULL | Member name |
| `age` | INT | CHECK > 0 | Age in years |
| `sex` | VARCHAR(10) | | "male", "female", "other" |
| `weight_kg` | DECIMAL(5,2) | | Body weight |
| `height_cm` | INT | | Height in cm |
| `activity_level` | VARCHAR(20) | | "sedentary", "light", "moderate", "active", "very_active" |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |

### `preferences`

Consolidated household preferences. JSONB for flexibility.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `household_id` | UUID | FK → households, UNIQUE | One per household |
| `dietary_restrictions` | JSONB | NOT NULL DEFAULT '[]' | `["vegan", "nut-free", "gluten-free"]` |
| `nutrition_goals` | JSONB | NOT NULL DEFAULT '{}' | `{"calories_per_day": 2100, "protein_per_day": 120}` |
| `equipment` | JSONB | NOT NULL DEFAULT '[]' | `["instant_pot", "air_fryer", "oven"]` |
| `max_cooking_time_minutes` | INT | DEFAULT 60 | Per-meal time ceiling |
| `weekly_budget` | DECIMAL(8,2) | | Weekly grocery budget |
| `preferred_supermarkets` | JSONB | DEFAULT '[]' | `["aldi", "lidl"]` |
| `food_preferences` | JSONB | DEFAULT '{}' | `{"liked_cuisines": [...], "disliked_ingredients": [...]}` |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |
| `updated_at` | TIMESTAMPTZ | DEFAULT NOW() | |

**Indexes:**
- GIN index on `dietary_restrictions`
- GIN index on `food_preferences`

### `ingredients`

Canonical ingredient reference with base nutritional data.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `name` | VARCHAR(200) | NOT NULL | Ingredient name |
| `category` | VARCHAR(50) | | "protein", "vegetable", "grain", "dairy", etc. |
| `default_unit` | VARCHAR(20) | NOT NULL | "g", "ml", "piece", "tbsp" |
| `nutrition_per_100g` | JSONB | NOT NULL | `{"calories": 120, "protein": 9, "carbs": 0, "fat": 8, ...}` |
| `allergens` | JSONB | DEFAULT '[]' | `["peanuts", "gluten"]` |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |

**Indexes:**
- GIN index on `allergens`
- Full-text search index on `name`

### `recipes`

Recipe catalog. Instructions and metadata.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `name` | VARCHAR(200) | NOT NULL | Recipe name |
| `description` | TEXT | | Short description |
| `instructions` | TEXT[] | NOT NULL | Ordered array of steps |
| `prep_time_minutes` | INT | NOT NULL | Prep time |
| `cook_time_minutes` | INT | NOT NULL | Cook time |
| `total_time_minutes` | INT | GENERATED | `prep_time + cook_time` |
| `base_servings` | INT | NOT NULL DEFAULT 2 | Original recipe yield |
| `equipment_needed` | JSONB | DEFAULT '[]' | `["oven", "knife"]` |
| `tags` | JSONB | DEFAULT '[]' | `["vegan", "italian", "quick"]` |
| `source` | VARCHAR(50) | | "user", "ai_generated", "imported" |
| `created_by` | UUID | FK → households | Null for system recipes |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |
| `updated_at` | TIMESTAMPTZ | DEFAULT NOW() | |

**Indexes:**
- GIN index on `tags`
- GIN index on `equipment_needed`
- Full-text search index on `name` + `description`

### `recipe_ingredients`

Junction table linking recipes to ingredients with quantities.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `recipe_id` | UUID | FK → recipes | Parent recipe |
| `ingredient_id` | UUID | FK → ingredients | Referenced ingredient |
| `quantity` | DECIMAL(10,3) | NOT NULL | Amount for `base_servings` |
| `unit` | VARCHAR(20) | NOT NULL | Unit of measurement |
| `is_optional` | BOOLEAN | DEFAULT FALSE | Can be omitted? |
| `preparation` | VARCHAR(100) | | "diced", "minced", "sliced" |

### `meal_plans`

Weekly meal plan aggregate root.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `household_id` | UUID | FK → households | Owner |
| `week_start_date` | DATE | NOT NULL | Monday of the planned week |
| `status` | VARCHAR(20) | DEFAULT 'draft' | "draft", "pending_review", "finalized", "archived" |
| `total_estimated_cost` | DECIMAL(8,2) | | Computed cost |
| `total_cooking_time_minutes` | INT | | Computed time |
| `nutrition_summary` | JSONB | | Aggregated weekly nutrition |
| `version` | INT | DEFAULT 1 | Incremented on each save |
| `workflow_id` | UUID | | OpenClaw workflow that generated this |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |
| `updated_at` | TIMESTAMPTZ | DEFAULT NOW() | |

**Indexes:**
- Composite index on `(household_id, week_start_date)`
- Index on `status`

### `meal_slots`

Individual meal positions within a plan.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `meal_plan_id` | UUID | FK → meal_plans | Parent plan |
| `day_of_week` | VARCHAR(10) | NOT NULL | "monday" through "sunday" |
| `meal_type` | VARCHAR(20) | NOT NULL | "breakfast", "lunch", "dinner", "snack" |
| `recipe_id` | UUID | FK → recipes | Assigned recipe (null if AI-generated pending save) |
| `recipe_name` | VARCHAR(200) | | Name when recipe_id is null (AI draft) |
| `servings` | INT | NOT NULL | Number of servings for this slot |
| `is_prep_meal` | BOOLEAN | DEFAULT FALSE | Prepped in advance? |
| `prep_notes` | TEXT | | Batch prep instructions |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |

**Indexes:**
- Composite index on `(meal_plan_id, day_of_week, meal_type)`

### `pantry_items`

Current household inventory.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `household_id` | UUID | FK → households | Owner |
| `ingredient_id` | UUID | FK → ingredients | Referenced ingredient |
| `quantity` | DECIMAL(10,3) | NOT NULL | Current amount |
| `unit` | VARCHAR(20) | NOT NULL | Unit |
| `date_added` | DATE | DEFAULT CURRENT_DATE | |
| `expires_at` | DATE | | Expiration date |
| `status` | VARCHAR(20) | DEFAULT 'available' | "available", "reserved", "consumed", "expired" |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |
| `updated_at` | TIMESTAMPTZ | DEFAULT NOW() | |

**Indexes:**
- Composite index on `(household_id, status)`
- Index on `expires_at` (for waste reduction queries)

### `shopping_lists`

Generated shopping lists for meal plans.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `meal_plan_id` | UUID | FK → meal_plans | Source plan |
| `supermarket_id` | UUID | FK → supermarkets | Target store |
| `estimated_total_cost` | DECIMAL(8,2) | | |
| `currency` | VARCHAR(3) | DEFAULT 'EUR' | |
| `status` | VARCHAR(20) | DEFAULT 'pending' | "pending", "shopping", "completed" |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |

### `shopping_list_items`

Individual items in a shopping list.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `shopping_list_id` | UUID | FK → shopping_lists | Parent list |
| `ingredient_id` | UUID | FK → ingredients | |
| `quantity_needed` | DECIMAL(10,3) | | For the recipes |
| `quantity_to_buy` | DECIMAL(10,3) | | Rounded to package size |
| `unit` | VARCHAR(20) | | |
| `estimated_price` | DECIMAL(6,2) | | |
| `purchased` | BOOLEAN | DEFAULT FALSE | |
| `price_at_purchase` | DECIMAL(6,2) | | Actual price paid |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |

### `supermarkets`

Store reference with pricing data.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `name` | VARCHAR(100) | NOT NULL | Store name |
| `chain` | VARCHAR(50) | | "Aldi", "Lidl", etc. |
| `location` | VARCHAR(200) | | Address or general area |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |

### `supermarket_prices`

Historical and current prices per ingredient per store.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `supermarket_id` | UUID | FK → supermarkets | |
| `ingredient_id` | UUID | FK → ingredients | |
| `price` | DECIMAL(6,2) | NOT NULL | Price per default unit |
| `currency` | VARCHAR(3) | DEFAULT 'EUR' | |
| `package_size` | DECIMAL(10,3) | | Typical package quantity |
| `package_unit` | VARCHAR(20) | | |
| `recorded_at` | TIMESTAMPTZ | DEFAULT NOW() | |

**Indexes:**
- Composite index on `(supermarket_id, ingredient_id, recorded_at DESC)`

---

## Audit Tables

### `decision_events`

Append-only log of every decision made during meal plan generation. This is the **causality reconstruction** table.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | BIGSERIAL | PK | Unique identifier |
| `workflow_id` | UUID | NOT NULL | OpenClaw workflow execution ID |
| `sequence_number` | INT | NOT NULL | Order within workflow |
| `timestamp` | TIMESTAMPTZ | DEFAULT NOW() | Decision time |
| `actor_type` | VARCHAR(20) | CHECK IN ('USER', 'AI_AGENT', 'BACKEND_SERVICE') | Who decided |
| `actor_name` | VARCHAR(100) | NOT NULL | Agent or service name |
| `decision_type` | VARCHAR(50) | NOT NULL | "PROPOSED", "VALIDATED", "MODIFIED", "APPROVED", etc. |
| `input_context` | JSONB | NOT NULL | Full state at decision time |
| `output_decision` | JSONB | NOT NULL | What was decided |
| `reasoning` | TEXT | | Natural language explanation |
| `parent_decision_id` | BIGINT | FK → decision_events | Causal chain |

**Indexes:**
- Unique on `(workflow_id, sequence_number)`
- Index on `workflow_id`
- Index on `actor_name, timestamp`
- GIN index on `input_context`
- GIN index on `output_decision`

### `ai_execution_logs`

Raw AI agent inputs and outputs for debugging.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| `id` | BIGSERIAL | PK | |
| `workflow_id` | UUID | NOT NULL | |
| `agent_name` | VARCHAR(100) | NOT NULL | |
| `prompt` | TEXT | NOT NULL | Full prompt sent to LLM |
| `response` | TEXT | | Full LLM response |
| `tokens_used` | INT | | Total tokens (prompt + completion) |
| `duration_ms` | INT | | API call duration |
| `model` | VARCHAR(50) | | "gpt-4.1", etc. |
| `created_at` | TIMESTAMPTZ | DEFAULT NOW() | |

**Indexes:**
- Index on `workflow_id`
- Index on `agent_name, created_at`

---

## JSONB Schemas

### `nutrition_per_100g` (ingredients)

```json
{
  "calories": 120,
  "protein": 9.0,
  "carbs": 0.0,
  "fat": 8.0,
  "fiber": 0.0,
  "sugar": 0.0,
  "sodium_mg": 45,
  "iron_mg": 0.8,
  "calcium_mg": 15,
  "vitamin_c_mg": 0,
  "vitamin_d_iu": 0
}
```

### `nutrition_goals` (preferences)

```json
{
  "calories_per_day": 2100,
  "protein_per_day": 120,
  "carbs_per_day": 250,
  "fat_per_day": 70,
  "fiber_per_day": 30,
  "sodium_mg_per_day": 2300
}
```

### `food_preferences` (preferences)

```json
{
  "liked_cuisines": ["italian", "indian", "mexican"],
  "disliked_ingredients": ["cilantro", "mushrooms", "anchovies"],
  "liked_ingredients": ["garlic", "chickpeas", "spinach"],
  "preferred_proteins": ["chicken", "tofu", "lentils"],
  "max_spice_level": 3
}
```

---

## Migrations Strategy

Use **EF Core Migrations** for schema evolution.

```bash
cd src
dotnet ef migrations add InitialCreate --project Infrastructure --startup-project Api
dotnet ef database update --project Infrastructure --startup-project Api
```

**Rules:**
- Never modify existing migration files after they've been applied to production.
- For data migrations, use SQL in migration files, not C# seeding.
- Review migration SQL before applying to production.
