# Bounded Contexts

This document defines the bounded contexts of the AI Meal Prep Planner domain. Each context has clear boundaries, responsibilities, and integration points.

## Context Map

```
┌─────────────────────────────────────────────────────────────┐
│                        User Preferences                        │
│  (households, members, dietary restrictions, goals)           │
└─────────────────────────────────────────────────────────────┘
         │                           │
         ▼                           ▼
┌──────────────┐            ┌──────────────┐
│   Pantry     │◄──────────►│ Meal Planning │
│  (inventory) │            │  (weekly plan)│
└──────────────┘            └──────┬───────┘
         │                         │
         ▼                         ▼
┌──────────────┐            ┌──────────────┐
│   Recipes    │◄──────────►│   Shopping   │
│  (catalog)   │            │  (lists, cost)│
└──────────────┘            └──────────────┘
         │
         ▼
┌──────────────┐
│   Nutrition  │
│  (calculations)
└──────────────┘
```

## Context Definitions

### 1. Meal Planning

**Responsibilities:**
- Weekly meal plan structure (days, slots, meals)
- Plan versioning and history
- Plan status lifecycle (draft → pending review → finalized → archived)
- Slot assignment (which recipe goes where)
- Meal prep flagging (which meals are prepped vs. cooked fresh)

**Aggregate Roots:**
- `MealPlan` — the weekly plan itself
- `MealSlot` — a single meal position within a plan

**Domain Events:**
- `MealPlanDraftCreated`
- `MealPlanNutritionValidated`
- `MealPlanFinalized`
- `MealPlanArchived`

**AI Agent:** Meal Planner

**Integration Points:**
- Reads from: User Preferences, Pantry, Recipes
- Publishes to: Nutrition (for validation), Shopping (for list generation)

---

### 2. Nutrition

**Responsibilities:**
- Calorie calculations
- Macro calculations (protein, carbs, fat)
- Micro calculations (vitamins, minerals)
- RDA (Recommended Daily Allowance) lookups
- Nutritional validation against goals
- Recipe scaling nutrition

**Aggregate Roots:**
- `NutritionProfile` — value object, not an aggregate
- `NutritionalGoals` — per-household targets

**Domain Services:**
- `NutritionCalculator` — deterministic calculations
- `NutritionValidator` — goal compliance checking

**AI Agent:** Nutrition Reviewer

**Rules:**
- This context is **purely deterministic**. No AI creativity here.
- All calculations use standardized USDA/EU food composition data.
- Tolerance: ±10% for macros, ±5% for calories.

---

### 3. Recipes

**Responsibilities:**
- Recipe storage and retrieval
- Recipe scaling (ingredient quantities for different servings)
- Ingredient substitution mapping
- Recipe combination (merging two recipes)
- Recipe adaptation (modifying for constraints)
- Tagging and categorization

**Aggregate Roots:**
- `Recipe` — the recipe itself
- `Ingredient` — canonical ingredient reference

**Domain Services:**
- `RecipeScaler` — deterministic scaling
- `SubstitutionEngine` — maps alternatives for dietary constraints

**AI Agent:** Recipe Generator

**Integration Points:**
- Provides recipes to Meal Planning
- Provides nutrition data to Nutrition context

---

### 4. Shopping

**Responsibilities:**
- Shopping list generation from meal plans
- Supermarket price estimation
- Cost optimization across supermarkets
- Ingredient-to-supermarket mapping
- Purchase history tracking

**Aggregate Roots:**
- `ShoppingList` — list for a specific plan
- `ShoppingListItem` — individual line item

**Domain Services:**
- `ShoppingListGenerator` — deterministic list creation
- `CostEstimator` — price lookup and calculation

**AI Agent:** Shopping Optimizer

**Integration Points:**
- Triggered by `MealPlanFinalized` event
- Reads from Pantry to exclude owned ingredients

---

### 5. Pantry

**Responsibilities:**
- Ingredient inventory tracking
- Expiration date monitoring
- Quantity management (add, consume, waste)
- Ingredient reservation (mark as "planned for use")
- Waste reporting

**Aggregate Roots:**
- `PantryItem` — a specific ingredient in the pantry

**Domain Events:**
- `PantryItemAdded`
- `PantryItemReserved`
- `PantryItemExpired`
- `PantryItemConsumed`

**No AI Agent** — purely backend logic.

**Integration Points:**
- Provides `get_pantry` data to Meal Planner
- Listens to `MealPlanFinalized` to reserve ingredients

---

### 6. User Preferences

**Responsibilities:**
- Household composition (people, ages, activity levels)
- Dietary restrictions (allergies, intolerances, diets)
- Nutritional goals (calories, macros, micros)
- Food preferences (liked/disliked ingredients, cuisines)
- Available equipment
- Available cooking time budgets
- Budget constraints
- Preferred supermarkets

**Aggregate Roots:**
- `Household` — the top-level aggregate
- `HouseholdMember` — individual within household
- `Preferences` — consolidated settings

**No AI Agent** — purely backend data.

**Integration Points:**
- Read by virtually all other contexts
- Updated via REST API (web UI)

---

### 7. Meal Prep

**Responsibilities:**
- Batch cooking schedule generation
- Prep step parallelization
- Equipment scheduling (oven, stove, Instant Pot)
- Food safety timing (how far in advance can something be prepped)
- Reheating instructions

**Aggregate Roots:**

`PrepSchedule` — the weekly prep plan.

| Field | Type | Description |
| --- | --- | --- |
| `Id` | UUID | PK |
| `MealPlanId` | UUID | FK → MealPlan being scheduled |
| `HouseholdId` | UUID | FK → Household |
| `WeekStartDate` | DATE | Monday of the prep week |
| `Status` | enum | `draft`, `feasibility_checked`, `finalized`, `archived` |
| `Tasks` | ICollection\<PrepTask\> | Child entities |
| `WorkflowId` | UUID? | OpenClaw workflow that generated this |
| `Version` | INT | Optimistic concurrency |
| `CreatedAt` / `UpdatedAt` | TIMESTAMPTZ | Audit timestamps |

Factory methods:
- `PrepSchedule.CreateDraft(mealPlanId, householdId, weekOf)` — creates a `draft` schedule with no tasks.
- `Finalize()` — transitions to `finalized` after feasibility validation passes.
- `Archive()` — transitions to `archived`; schedule is read-only.

`PrepTask` — individual batch prep task (child of `PrepSchedule`).

| Field | Type | Description |
| --- | --- | --- |
| `Id` | UUID | PK |
| `PrepScheduleId` | UUID | FK → PrepSchedule (CASCADE delete) |
| `RecipeId` | UUID | FK → Recipe being prepped |
| `DayOfWeek` | VARCHAR(10) | Which day the prep happens |
| `MealType` | VARCHAR(20) | Breakfast, lunch, dinner, snack |
| `BatchSizeServings` | INT | How many servings this batch produces |
| `EarliestStartOffset` | INT | Minutes from week_start_date |
| `LatestFinishOffset` | INT | Hard deadline for this task |
| `EquipmentIds` | text[] | Required equipment IDs |
| `Steps` | JSONB | Ordered prep steps |
| `AssignedToSlotIds` | UUID[] | Links to meal_slots.id |

**Domain Events:**
- `PrepScheduleDraftCreated`
- `PrepScheduleFeasibilityChecked` (carries violation count)
- `PrepScheduleFinalized`
- `PrepScheduleArchived`

**AI Agent:** Meal Prep Optimizer

**Determinism boundary (2026-08-16):**
The Meal Prep Optimizer agent is creative-only — it proposes draft schedules and iterates on violations. The `PrepFeasibilityValidator` is a deterministic C# domain service that evaluates feasibility (equipment conflicts, time overlaps, food safety). The agent calls `validate_prep_schedule` and refines the draft up to a configurable max-attempts. See ADR 003 ("AI does not own business logic").

**Anti-corruption layer:**
Meal Prep reads `MealPlan` via the application-service snapshot (does not query `meal_plans` table directly), consistent with the rest of the doc.

**Integration Points:**
- Reads finalized meal plans
- Outputs prep schedules that reference Meal Planning slots

---

## Anti-Corruption Layers

Each context exposes a **public API** (application services / DTOs) that other contexts consume. No context directly queries another context's database tables.

**Example:** Shopping context does not query `recipes` or `meal_slots` tables directly. It receives a `MealPlanFinalized` event with a snapshot of needed ingredients, or calls `get_shopping_list_data` via the application layer.

## Context Ownership

| Context | Primary Project | Owner (conceptual) |
|---------|----------------|-------------------|
| Meal Planning | `MealPrepPlanner.Domain` | Meal Planner agent + backend |
| Nutrition | `MealPrepPlanner.Domain` | Backend only |
| Recipes | `MealPrepPlanner.Domain` | Recipe Generator agent + backend |
| Shopping | `MealPrepPlanner.Domain` | Shopping Optimizer agent + backend |
| Pantry | `MealPrepPlanner.Domain` | Backend only |
| User Preferences | `MealPrepPlanner.Domain` | Backend only |
| Meal Prep | `MealPrepPlanner.Domain` | Meal Prep Optimizer agent + backend |

All contexts live in the same monolithic backend for now. If extracted to microservices later, these boundaries define the split lines.
