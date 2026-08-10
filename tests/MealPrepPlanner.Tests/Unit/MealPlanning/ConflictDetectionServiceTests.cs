namespace MealPrepPlanner.Tests.Unit.MealPlanning;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.MealPlanning.Services;
using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;
using MealPrepPlanner.Domain.UserPreferences;

public class ConflictDetectionServiceTests
{
    [Fact]
    public void DetectConflicts_EmptyPlan_ReturnsValid()
    {
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today));
        var preferences = new Preferences();

        var result = new ConflictDetectionService().DetectConflicts(plan, preferences);

        Assert.True(result.Valid);
        Assert.Empty(result.Violations);
    }

    [Fact]
    public void DetectConflicts_UnassignedSlot_IsSkipped()
    {
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today));
        plan.AddSlot(DayOfWeek.Monday, MealType.Dinner, 4);

        var result = new ConflictDetectionService().DetectConflicts(plan, new Preferences());

        Assert.True(result.Valid);
    }

    [Fact]
    public void DetectConflicts_AllergenConflict_ReportsErrorViolation()
    {
        var plan = PlanWithAssignedRecipe(allergens: ["peanuts"]);
        var preferences = CreatePreferences([new DietaryRestriction("nut-free")]);

        var result = new ConflictDetectionService().DetectConflicts(plan, preferences);

        Assert.False(result.Valid);
        var violation = Assert.Single(result.Violations);
        Assert.Equal(ConflictSeverity.Error, violation.Severity);
        Assert.Equal(ConflictType.Allergy, violation.Type);
        Assert.Contains("peanuts", violation.Message);
        Assert.Equal(DayOfWeek.Monday, violation.Day);
        Assert.Equal(MealType.Dinner, violation.Meal);
    }

    [Fact]
    public void DetectConflicts_MatchingRestriction_DoesNotFlagUnrelatedAllergen()
    {
        var plan = PlanWithAssignedRecipe(allergens: ["peanuts"]);
        var preferences = CreatePreferences([new DietaryRestriction("gluten")]);

        var result = new ConflictDetectionService().DetectConflicts(plan, preferences);

        Assert.True(result.Valid);
    }

    [Fact]
    public void DetectConflicts_MissingEquipment_ReportsViolation()
    {
        var plan = PlanWithAssignedRecipe(equipment: [new Equipment("oven")]);
        var preferences = CreatePreferences();

        var result = new ConflictDetectionService().DetectConflicts(plan, preferences);

        Assert.False(result.Valid);
        var violation = Assert.Single(result.Violations);
        Assert.Equal(ConflictType.Equipment, violation.Type);
        Assert.Contains("oven", violation.Message);
    }

    [Fact]
    public void DetectConflicts_OwnedEquipment_IsAccepted()
    {
        var plan = PlanWithAssignedRecipe(equipment: [new Equipment("oven")]);
        var preferences = CreatePreferences(equipment: [new Equipment("oven")]);

        var result = new ConflictDetectionService().DetectConflicts(plan, preferences);

        Assert.True(result.Valid);
    }

    [Fact]
    public void DetectConflicts_TimeOverBudget_ReportsViolation()
    {
        var plan = PlanWithAssignedRecipe(prepTime: 40, cookTime: 40);
        var preferences = CreatePreferences(maxCookingTimeMinutes: 60);

        var result = new ConflictDetectionService().DetectConflicts(plan, preferences);

        Assert.False(result.Valid);
        var violation = Assert.Single(result.Violations);
        Assert.Equal(ConflictType.Time, violation.Type);
        Assert.Contains("80 minutes", violation.Message);
    }

    [Fact]
    public void DetectConflicts_TimeExactlyAtBudget_IsAccepted()
    {
        var plan = PlanWithAssignedRecipe(prepTime: 20, cookTime: 40);
        var preferences = CreatePreferences(maxCookingTimeMinutes: 60);

        var result = new ConflictDetectionService().DetectConflicts(plan, preferences);

        Assert.True(result.Valid);
    }

    [Fact]
    public void DetectConflicts_MultipleViolations_ReportedTogether()
    {
        var plan = PlanWithAssignedRecipe(
            prepTime: 50,
            cookTime: 50,
            allergens: ["peanuts"],
            equipment: [new Equipment("oven")]);
        var preferences = CreatePreferences(
            [new DietaryRestriction("nut-free")],
            maxCookingTimeMinutes: 60);

        var result = new ConflictDetectionService().DetectConflicts(plan, preferences);

        Assert.False(result.Valid);
        Assert.Equal(3, result.Violations.Count);
        Assert.Contains(result.Violations, v => v.Type == ConflictType.Allergy);
        Assert.Contains(result.Violations, v => v.Type == ConflictType.Equipment);
        Assert.Contains(result.Violations, v => v.Type == ConflictType.Time);
    }

    private static Preferences CreatePreferences(
        IReadOnlyList<DietaryRestriction>? restrictions = null,
        IReadOnlyList<Equipment>? equipment = null,
        int maxCookingTimeMinutes = 60) =>
        new(dietaryRestrictions: restrictions, equipment: equipment, maxCookingTimeMinutes: maxCookingTimeMinutes);

    private static MealPlan PlanWithAssignedRecipe(
        int prepTime = 20,
        int cookTime = 30,
        string[]? allergens = null,
        IReadOnlyList<Equipment>? equipment = null)
    {
        var plan = MealPlan.CreateDraft(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Today));
        var slot = plan.AddSlot(DayOfWeek.Monday, MealType.Dinner, 4);
        var recipe = Recipe.Create(
            "Stew",
            null,
            ["Step 1"],
            prepTime,
            cookTime,
            4,
            equipmentNeeded: equipment);
        recipe.AddIngredient(
            Ingredient.Create("Peanuts", new NutritionProfile(100m, 10m, 5m, 5m), allergens: allergens),
            new Quantity(50m, "g"));
        plan.AssignRecipe(slot, recipe, 4);
        return plan;
    }
}
