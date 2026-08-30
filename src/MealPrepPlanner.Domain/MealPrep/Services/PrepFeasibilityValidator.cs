namespace MealPrepPlanner.Domain.MealPrep.Services;

using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;
using MealPrepPlanner.Domain.UserPreferences;

/// <summary>
/// Deterministic evaluator for a draft <see cref="PrepSchedule"/>. Pure C# —
/// no I/O, no AI, no clock dependency. Returns the structured verdict that
/// the Meal Prep Optimizer agent iterates against. Per ADR 003 ("AI does not
/// own business logic"), feasibility is always decided by the backend.
///
/// <para>Rule set:</para>
/// <list type="bullet">
///   <item><description><c>EquipmentUnavailable</c> — task requires an equipment id not in household preferences.</description></item>
///   <item><description><c>TimeBudgetExceeded</c> — task duration exceeds <c>Preferences.MaxCookingTimeMinutes</c>.</description></item>
///   <item><description><c>FoodSafetyWindow</c> — task's latest finish is more than <c>Preferences.FoodSafetyWindowHours</c> after its earliest start, and the recipe is flagged perishable.</description></item>
///   <item><description><c>RecipeNotFound</c> — task references a recipe id not present in the supplied catalog.</description></item>
///   <item><description><c>OverlappingEquipmentUsage</c> — two tasks sharing an equipment id have overlapping time windows on the same day.</description></item>
/// </list>
/// </summary>
public sealed class PrepFeasibilityValidator
{
    private static readonly HashSet<string> PerishableTags = new(StringComparer.OrdinalIgnoreCase)
    {
        "chicken",
        "fish",
        "seafood",
        "salad",
        "raw",
        "dairy",
        "egg"
    };

    public PrepFeasibilityResult Validate(
        PrepSchedule schedule,
        Preferences preferences,
        IReadOnlyDictionary<Guid, Recipe> recipes,
        DateTimeOffset checkedAt)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        ArgumentNullException.ThrowIfNull(preferences);
        ArgumentNullException.ThrowIfNull(recipes);

        var violations = new List<PrepFeasibilityViolationDocument>();
        var ownedEquipment = preferences.Equipment
            .Select(e => EquipmentRef.Normalize(e.Name))
            .ToHashSet(StringComparer.Ordinal);
        var safetyConstraint = new FoodSafetyConstraint(preferences.FoodSafetyWindowHours);

        var tasksById = schedule.Tasks.ToDictionary(t => t.Id);

        foreach (var task in schedule.Tasks)
        {
            ValidateRecipeExists(task, recipes, violations);
            ValidateEquipment(task, ownedEquipment, violations);
            ValidateTimeBudget(task, preferences, violations);
            ValidateFoodSafetyWindow(task, recipes, safetyConstraint, violations);
        }

        DetectOverlappingEquipmentUsage(schedule.Tasks, violations);

        var document = new PrepFeasibilityViolationsDocument(checkedAt, violations);
        return new PrepFeasibilityResult(document, violations.Count == 0);
    }

    private static void ValidateRecipeExists(
        PrepTask task,
        IReadOnlyDictionary<Guid, Recipe> recipes,
        List<PrepFeasibilityViolationDocument> violations)
    {
        if (!recipes.ContainsKey(task.RecipeId))
        {
            violations.Add(new PrepFeasibilityViolationDocument(
                PrepViolationKind.RecipeNotFound,
                task.Id,
                task.RecipeId,
                $"Recipe '{task.RecipeId}' referenced by prep task is not in the supplied catalog.",
                "Remove the task or load the missing recipe before re-validating."));
        }
    }

    private static void ValidateEquipment(
        PrepTask task,
        HashSet<string> ownedEquipment,
        List<PrepFeasibilityViolationDocument> violations)
    {
        foreach (var equipmentId in task.EquipmentIds)
        {
            var normalized = EquipmentRef.Normalize(equipmentId);
            if (!ownedEquipment.Contains(normalized))
            {
                violations.Add(new PrepFeasibilityViolationDocument(
                    PrepViolationKind.EquipmentUnavailable,
                    task.Id,
                    task.RecipeId,
                    $"Task requires equipment '{equipmentId}' which is not available in the household's equipment list.",
                    "Reschedule the task on a day the household owns this equipment, or substitute a recipe that uses owned equipment."));
            }
        }
    }

    private static void ValidateTimeBudget(
        PrepTask task,
        Preferences preferences,
        List<PrepFeasibilityViolationDocument> violations)
    {
        var durationMinutes = task.LatestFinishOffsetMinutes - task.EarliestStartOffsetMinutes;
        if (durationMinutes > preferences.MaxCookingTimeMinutes)
        {
            violations.Add(new PrepFeasibilityViolationDocument(
                PrepViolationKind.TimeBudgetExceeded,
                task.Id,
                task.RecipeId,
                $"Task duration {durationMinutes} minutes exceeds the household's per-meal ceiling of {preferences.MaxCookingTimeMinutes} minutes.",
                $"Split the task or extend its time window so duration ≤ {preferences.MaxCookingTimeMinutes} minutes."));
        }
    }

    private static void ValidateFoodSafetyWindow(
        PrepTask task,
        IReadOnlyDictionary<Guid, Recipe> recipes,
        FoodSafetyConstraint safetyConstraint,
        List<PrepFeasibilityViolationDocument> violations)
    {
        if (!recipes.TryGetValue(task.RecipeId, out var recipe))
        {
            return;
        }

        var isPerishable = recipe.Tags.Any(t => PerishableTags.Contains(t));
        if (!isPerishable)
        {
            return;
        }

        var taskDurationHours =
            (task.LatestFinishOffsetMinutes - task.EarliestStartOffsetMinutes) / 60.0;

        if (taskDurationHours > safetyConstraint.MaxStorageHours)
        {
            violations.Add(new PrepFeasibilityViolationDocument(
                PrepViolationKind.FoodSafetyWindow,
                task.Id,
                task.RecipeId,
                $"Perishable recipe '{recipe.Name}' has a prep window of {taskDurationHours:F1} hours, exceeding the {safetyConstraint.MaxStorageHours}-hour food-safety window.",
                "Re-batch the recipe closer to consumption, or freeze the batch (extends safe storage beyond the fridge window)."));
        }
    }

    private static void DetectOverlappingEquipmentUsage(
        IReadOnlyList<PrepTask> tasks,
        List<PrepFeasibilityViolationDocument> violations)
    {
        var byEquipment = new Dictionary<string, List<PrepTask>>(StringComparer.OrdinalIgnoreCase);

        foreach (var task in tasks)
        {
            foreach (var equipmentId in task.EquipmentIds)
            {
                if (!byEquipment.TryGetValue(equipmentId, out var bucket))
                {
                    bucket = [];
                    byEquipment[equipmentId] = bucket;
                }

                bucket.Add(task);
            }
        }

        foreach (var (equipmentId, bucket) in byEquipment)
        {
            if (bucket.Count < 2)
            {
                continue;
            }

            for (var i = 0; i < bucket.Count; i++)
            {
                for (var j = i + 1; j < bucket.Count; j++)
                {
                    var a = bucket[i];
                    var b = bucket[j];

                    if (a.DayOfWeek != b.DayOfWeek)
                    {
                        continue;
                    }

                    if (a.Window.OverlapsWith(b.Window))
                    {
                        var first = a.EarliestStartOffsetMinutes <= b.EarliestStartOffsetMinutes ? a : b;
                        var second = ReferenceEquals(first, a) ? b : a;

                        violations.Add(new PrepFeasibilityViolationDocument(
                            PrepViolationKind.OverlappingEquipmentUsage,
                            first.Id,
                            first.RecipeId,
                            $"Two tasks on {first.DayOfWeek} both need equipment '{equipmentId}' and overlap.",
                            $"Shift the later task (current earliest start {second.EarliestStartOffsetMinutes}m) so its window starts at or after {first.LatestFinishOffsetMinutes}m."));
                    }
                }
            }
        }
    }
}

/// <summary>
/// Verdict returned by <see cref="PrepFeasibilityValidator"/>.
/// <see cref="Valid"/> is true iff no violations were found. The full set of
/// violations is always returned so the agent can iterate against them.
/// </summary>
public sealed record PrepFeasibilityResult(
    PrepFeasibilityViolationsDocument Document,
    bool Valid);
