namespace MealPrepPlanner.Tests.Unit.MealPrep;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.MealPrep;
using MealPrepPlanner.Domain.MealPrep.Services;
using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.UserPreferences;

public class PrepFeasibilityValidatorTests
{
    private static readonly DateTimeOffset CheckedAt = new(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid HouseholdId = Guid.NewGuid();
    private static readonly Guid MealPlanId = Guid.NewGuid();
    private static readonly DateOnly WeekStart = new(2026, 8, 17);

    [Fact]
    public void Validate_HappyPath_ReportsValid()
    {
        var recipe = BuildRecipe(RecipeId: Guid.NewGuid(), tags: ["chicken"]);
        var schedule = BuildSchedule(
            BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Dinner, earliest: 0, latest: 60, equipmentIds: ["oven"]));

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 120, foodSafetyHours: 72);
        var recipes = new Dictionary<Guid, Recipe> { [recipe.Id] = recipe };

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes, CheckedAt);

        Assert.True(result.Valid);
        Assert.Empty(result.Document.Violations);
    }

    [Fact]
    public void Validate_EquipmentUnavailable_EmitsViolation()
    {
        var recipe = BuildRecipe(RecipeId: Guid.NewGuid(), tags: []);
        var schedule = BuildSchedule(
            BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Dinner, earliest: 0, latest: 60, equipmentIds: ["pressure_cooker"]));

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 120, foodSafetyHours: 72);
        var recipes = new Dictionary<Guid, Recipe> { [recipe.Id] = recipe };

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes, CheckedAt);

        Assert.False(result.Valid);
        var violation = Assert.Single(result.Document.Violations);
        Assert.Equal(PrepViolationKind.EquipmentUnavailable, violation.Kind);
        Assert.Contains("pressure_cooker", violation.Message);
    }

    [Fact]
    public void Validate_TimeBudgetExceeded_EmitsViolation()
    {
        var recipe = BuildRecipe(RecipeId: Guid.NewGuid(), tags: []);
        var schedule = BuildSchedule(
            BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Dinner, earliest: 0, latest: 180, equipmentIds: ["oven"]));

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 60, foodSafetyHours: 72);
        var recipes = new Dictionary<Guid, Recipe> { [recipe.Id] = recipe };

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes, CheckedAt);

        Assert.False(result.Valid);
        var violation = Assert.Single(result.Document.Violations);
        Assert.Equal(PrepViolationKind.TimeBudgetExceeded, violation.Kind);
        Assert.Contains("exceeds the household's per-meal ceiling", violation.Message);
    }

    [Fact]
    public void Validate_FoodSafetyWindow_PerishableRecipeOverDefaultWindow_EmitsViolation()
    {
        var recipe = BuildRecipe(RecipeId: Guid.NewGuid(), tags: ["chicken"]);
        // 80 hours > 72-hour default window. Time budget set high enough to isolate the food-safety check.
        var schedule = BuildSchedule(
            BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Dinner, earliest: 0, latest: 80 * 60, equipmentIds: ["oven"]));

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 80 * 60, foodSafetyHours: 72);
        var recipes = new Dictionary<Guid, Recipe> { [recipe.Id] = recipe };

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes, CheckedAt);

        Assert.False(result.Valid);
        var violation = Assert.Single(result.Document.Violations);
        Assert.Equal(PrepViolationKind.FoodSafetyWindow, violation.Kind);
    }

    [Fact]
    public void Validate_FoodSafetyWindow_NonPerishableRecipe_DoesNotEmit()
    {
        var recipe = BuildRecipe(RecipeId: Guid.NewGuid(), tags: ["pasta"]);
        var schedule = BuildSchedule(
            BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Dinner, earliest: 0, latest: 80 * 60, equipmentIds: ["oven"]));

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 80 * 60, foodSafetyHours: 72);
        var recipes = new Dictionary<Guid, Recipe> { [recipe.Id] = recipe };

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes, CheckedAt);

        Assert.True(result.Valid);
        Assert.Empty(result.Document.Violations);
    }

    [Fact]
    public void Validate_OverlappingEquipmentUsage_SameDaySameEquipment_EmitsViolation()
    {
        var recipe = BuildRecipe(RecipeId: Guid.NewGuid(), tags: []);
        var taskA = BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Lunch, earliest: 0, latest: 60, equipmentIds: ["oven"]);
        var taskB = BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Dinner, earliest: 30, latest: 120, equipmentIds: ["oven"]);
        var schedule = BuildSchedule(taskA, taskB);

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 240, foodSafetyHours: 72);
        var recipes = new Dictionary<Guid, Recipe> { [recipe.Id] = recipe };

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes, CheckedAt);

        Assert.False(result.Valid);
        var violation = Assert.Single(result.Document.Violations);
        Assert.Equal(PrepViolationKind.OverlappingEquipmentUsage, violation.Kind);
        Assert.Contains("oven", violation.Message);
    }

    [Fact]
    public void Validate_OverlappingEquipmentUsage_DifferentDays_DoesNotEmit()
    {
        var recipe = BuildRecipe(RecipeId: Guid.NewGuid(), tags: []);
        var taskA = BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Lunch, earliest: 0, latest: 60, equipmentIds: ["oven"]);
        var taskB = BuildTask(recipe.Id, DayOfWeek.Monday, MealType.Dinner, earliest: 30, latest: 120, equipmentIds: ["oven"]);
        var schedule = BuildSchedule(taskA, taskB);

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 240, foodSafetyHours: 72);
        var recipes = new Dictionary<Guid, Recipe> { [recipe.Id] = recipe };

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes, CheckedAt);

        Assert.True(result.Valid);
        Assert.Empty(result.Document.Violations);
    }

    [Fact]
    public void Validate_RecipeNotFound_EmitsViolation()
    {
        var missingRecipeId = Guid.NewGuid();
        var schedule = BuildSchedule(
            BuildTask(missingRecipeId, DayOfWeek.Sunday, MealType.Dinner, earliest: 0, latest: 60, equipmentIds: ["oven"]));

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 120, foodSafetyHours: 72);

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes: new Dictionary<Guid, Recipe>(), CheckedAt);

        Assert.False(result.Valid);
        var violation = Assert.Single(result.Document.Violations);
        Assert.Equal(PrepViolationKind.RecipeNotFound, violation.Kind);
    }

    [Fact]
    public void Validate_AggregatesMultipleViolations()
    {
        var recipe = BuildRecipe(RecipeId: Guid.NewGuid(), tags: []);
        var schedule = BuildSchedule(
            BuildTask(recipe.Id, DayOfWeek.Sunday, MealType.Dinner, earliest: 0, latest: 180, equipmentIds: ["pressure_cooker"]));

        var prefs = BuildPreferences(equipment: ["oven"], maxMinutes: 60, foodSafetyHours: 72);
        var recipes = new Dictionary<Guid, Recipe> { [recipe.Id] = recipe };

        var result = new PrepFeasibilityValidator().Validate(schedule, prefs, recipes, CheckedAt);

        Assert.False(result.Valid);
        Assert.Equal(2, result.Document.Violations.Count);
        Assert.Contains(result.Document.Violations, v => v.Kind == PrepViolationKind.EquipmentUnavailable);
        Assert.Contains(result.Document.Violations, v => v.Kind == PrepViolationKind.TimeBudgetExceeded);
    }

    [Fact]
    public void Validate_NullArguments_Throw()
    {
        var schedule = BuildSchedule();
        var prefs = BuildPreferences();
        var recipes = new Dictionary<Guid, Recipe>();

        Assert.Throws<ArgumentNullException>(() => new PrepFeasibilityValidator().Validate(null!, prefs, recipes, CheckedAt));
        Assert.Throws<ArgumentNullException>(() => new PrepFeasibilityValidator().Validate(schedule, null!, recipes, CheckedAt));
        Assert.Throws<ArgumentNullException>(() => new PrepFeasibilityValidator().Validate(schedule, prefs, null!, CheckedAt));
    }

    private static PrepSchedule BuildSchedule(params PrepTask[] tasks)
    {
        var schedule = PrepSchedule.CreateDraft(HouseholdId, MealPlanId, WeekStart);
        foreach (var task in tasks)
        {
            schedule.AddTask(task);
        }

        return schedule;
    }

    private static PrepTask BuildTask(
        Guid recipeId,
        DayOfWeek day,
        MealType mealType,
        int earliest,
        int latest,
        IReadOnlyList<string>? equipmentIds = null) =>
        PrepTask.Create(
            recipeId,
            day,
            mealType,
            batchSizeServings: 4,
            earliestStartOffsetMinutes: earliest,
            latestFinishOffsetMinutes: latest,
            equipmentIds: equipmentIds);

    private static Recipe BuildRecipe(Guid RecipeId, IReadOnlyList<string> tags) =>
        Recipe.Create(
            name: "Test recipe",
            description: null,
            instructions: ["Step 1"],
            prepTimeMinutes: 10,
            cookTimeMinutes: 20,
            baseServings: 2,
            tags: tags);

    private static Preferences BuildPreferences(
        IReadOnlyList<string>? equipment = null,
        int maxMinutes = 120,
        int foodSafetyHours = 72) =>
        new(
            equipment: equipment?.Select(e => new Equipment(e)).ToList() ?? [],
            maxCookingTimeMinutes: maxMinutes,
            foodSafetyWindowHours: foodSafetyHours);
}
