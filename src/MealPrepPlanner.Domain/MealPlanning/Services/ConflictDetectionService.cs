namespace MealPrepPlanner.Domain.MealPlanning.Services;

using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.UserPreferences;

/// <summary>
/// Detects hard-constraint violations (allergies, equipment availability, cooking
/// time budget) in a meal plan draft. Deterministic; owned by the backend, never by AI.
/// </summary>
public sealed class ConflictDetectionService
{
    public ConflictResult DetectConflicts(MealPlan plan, Preferences preferences)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(preferences);

        var violations = new List<ConflictViolation>();

        foreach (var slot in plan.Slots)
        {
            if (slot.Recipe is null)
                continue;

            DetectAllergens(slot, preferences, violations);
            DetectEquipment(slot, preferences, violations);
            DetectTimeBudget(slot, preferences, violations);
        }

        return violations.Count == 0
            ? ConflictResult.None()
            : ConflictResult.Invalid(violations);
    }

    private static void DetectAllergens(
        MealSlot slot,
        Preferences preferences,
        List<ConflictViolation> violations)
    {
        var recipe = slot.Recipe!;

        foreach (var ingredient in recipe.Ingredients)
        {
            foreach (var allergen in ingredient.Allergens)
            {
                if (preferences.DietaryRestrictions.Any(r => r.ConflictsWith(allergen)))
                    violations.Add(new ConflictViolation(
                    ConflictSeverity.Error,
                    ConflictType.Allergy,
                    $"Recipe '{recipe.Name}' contains '{allergen}', violating a household dietary restriction.",
                    slot.DayOfWeek,
                    slot.MealType,
                    recipe.Id));
            }
        }
    }

    private static void DetectEquipment(
        MealSlot slot,
        Preferences preferences,
        List<ConflictViolation> violations)
    {
        var recipe = slot.Recipe!;
        var owned = preferences.Equipment.Select(e => e.Name).ToHashSet(StringComparer.Ordinal);

        foreach (var needed in recipe.EquipmentNeeded)
        {
            if (!owned.Contains(needed.Name))
                violations.Add(new ConflictViolation(
                ConflictSeverity.Error,
                ConflictType.Equipment,
                $"Recipe '{recipe.Name}' requires equipment '{needed.Name}' that is not available.",
                slot.DayOfWeek,
                slot.MealType,
                recipe.Id));
        }
    }

    private static void DetectTimeBudget(
        MealSlot slot,
        Preferences preferences,
        List<ConflictViolation> violations)
    {
        var recipe = slot.Recipe!;
        var totalMinutes = recipe.PrepTimeMinutes + recipe.CookTimeMinutes;

        if (totalMinutes > preferences.MaxCookingTimeMinutes)
            violations.Add(new ConflictViolation(
            ConflictSeverity.Error,
            ConflictType.Time,
            $"Recipe '{recipe.Name}' takes {totalMinutes} minutes, exceeding the {preferences.MaxCookingTimeMinutes} minute limit.",
            slot.DayOfWeek,
            slot.MealType,
            recipe.Id));
    }
}
