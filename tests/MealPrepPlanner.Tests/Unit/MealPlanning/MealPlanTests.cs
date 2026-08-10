namespace MealPrepPlanner.Tests.Unit.MealPlanning;

using MealPrepPlanner.Domain.MealPlanning;
using MealPrepPlanner.Domain.MealPlanning.Events;
using MealPrepPlanner.Domain.Recipes;
using MealPrepPlanner.Domain.Shared;

public class MealPlanTests
{
    private static readonly Guid HouseholdId = Guid.NewGuid();
    private static readonly DateOnly WeekStart = new(2026, 8, 10);

    [Fact]
    public void CreateDraft_SetsDraftStatusAndEmitsEvent()
    {
        var correlationId = Guid.NewGuid();

        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart, correlationId);

        Assert.Equal(MealPlanStatus.Draft, plan.Status);
        Assert.Equal(HouseholdId, plan.HouseholdId);
        Assert.Equal(WeekStart, plan.WeekStartDate);
        var created = Assert.Single(plan.DomainEvents.OfType<MealPlanDraftCreated>());
        Assert.Equal(plan.Id, created.MealPlanId);
        Assert.Equal(HouseholdId, created.HouseholdId);
        Assert.Equal(WeekStart, created.WeekStartDate);
        Assert.Equal(correlationId, created.CorrelationId);
    }

    [Fact]
    public void CreateDraft_EmptyHouseholdId_Throws()
    {
        Assert.Throws<ArgumentException>(() => MealPlan.CreateDraft(Guid.Empty, WeekStart));
    }

    [Fact]
    public void AddSlot_AddsSlotWithDefaults()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);

        var slot = plan.AddSlot(DayOfWeek.Monday, MealType.Dinner, 4);

        Assert.Equal(DayOfWeek.Monday, slot.DayOfWeek);
        Assert.Equal(MealType.Dinner, slot.MealType);
        Assert.Equal(4, slot.Servings);
        Assert.False(slot.IsPrepMeal);
        Assert.Null(slot.PrepNotes);
        Assert.Single(plan.Slots);
    }

    [Fact]
    public void AddSlot_NonDraftStatus_Throws()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        plan.SubmitForReview();

        Assert.Throws<InvalidOperationException>(
            () => plan.AddSlot(DayOfWeek.Monday, MealType.Dinner, 4));
    }

    [Fact]
    public void AddSlot_NonPositiveServings_Throws()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => plan.AddSlot(DayOfWeek.Monday, MealType.Dinner, 0));
    }

    [Fact]
    public void AssignRecipe_SetsRecipeOnSlot()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        var slot = plan.AddSlot(DayOfWeek.Tuesday, MealType.Lunch, 2);
        var recipe = CreateRecipe();

        plan.AssignRecipe(slot, recipe, 4);

        Assert.Equal(recipe.Id, slot.RecipeId);
        Assert.Equal(recipe.Name, slot.RecipeName);
        Assert.Same(recipe, slot.Recipe);
        Assert.Equal(4, slot.Servings);
    }

    [Fact]
    public void AssignRecipe_ForeignSlot_Throws()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        var slot = plan.AddSlot(DayOfWeek.Monday, MealType.Dinner, 4);
        var otherPlan = MealPlan.CreateDraft(HouseholdId, WeekStart);

        var ex = Assert.Throws<InvalidOperationException>(
            () => otherPlan.AssignRecipe(slot, CreateRecipe(), 4));

        Assert.Contains("does not belong", ex.Message);
    }

    [Fact]
    public void AssignRecipe_AllowedInPendingReview()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        var slot = plan.AddSlot(DayOfWeek.Wednesday, MealType.Dinner, 2);
        plan.SubmitForReview();

        plan.AssignRecipe(slot, CreateRecipe(), 3);

        Assert.Equal(3, slot.Servings);
    }

    [Fact]
    public void AssignRecipe_FinalizedPlan_Throws()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        var slot = plan.AddSlot(DayOfWeek.Wednesday, MealType.Dinner, 2);
        plan.SubmitForReview();
        plan.Finalize();

        Assert.Throws<InvalidOperationException>(
            () => plan.AssignRecipe(slot, CreateRecipe(), 3));
    }

    [Fact]
    public void SubmitForReview_TransitionsToPendingReview()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);

        plan.SubmitForReview();

        Assert.Equal(MealPlanStatus.PendingReview, plan.Status);
    }

    [Fact]
    public void SubmitForReview_NonDraftStatus_Throws()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        plan.SubmitForReview();

        Assert.Throws<InvalidOperationException>(() => plan.SubmitForReview());
    }

    [Fact]
    public void ValidateNutrition_EmitsEventWithDetails()
    {
        var correlationId = Guid.NewGuid();
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        var profile = new NutritionProfile(2000m, 100m, 250m, 70m);

        plan.ValidateNutrition(profile, isValid: false, ["Calories over"], correlationId);

        var validated = Assert.Single(plan.DomainEvents.OfType<MealPlanNutritionValidated>());
        Assert.Equal(plan.Id, validated.MealPlanId);
        Assert.False(validated.IsValid);
        Assert.Equal(["Calories over"], validated.Warnings);
        Assert.Equal(profile, validated.DailyProfile);
        Assert.Equal(correlationId, validated.CorrelationId);
    }

    [Fact]
    public void ValidateNutrition_FinalizedPlan_Throws()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        plan.SubmitForReview();
        plan.Finalize();

        Assert.Throws<InvalidOperationException>(
            () => plan.ValidateNutrition(new NutritionProfile(2000m, 100m, 250m, 70m), isValid: true));
    }

    [Fact]
    public void Finalize_OnlyFromPendingReview()
    {
        var draft = MealPlan.CreateDraft(HouseholdId, WeekStart);
        Assert.Throws<InvalidOperationException>(() => draft.Finalize());

        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        plan.SubmitForReview();
        var correlationId = Guid.NewGuid();

        plan.Finalize(correlationId);

        Assert.Equal(MealPlanStatus.Finalized, plan.Status);
        var finalized = Assert.Single(plan.DomainEvents.OfType<MealPlanFinalized>());
        Assert.Equal(plan.Id, finalized.MealPlanId);
        Assert.Equal(HouseholdId, finalized.HouseholdId);
        Assert.Equal(correlationId, finalized.CorrelationId);
        Assert.True(DateTimeOffset.UtcNow - finalized.FinalizedAt < TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Archive_TransitionsAndEmitsEvent()
    {
        var correlationId = Guid.NewGuid();
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);

        plan.Archive(correlationId);

        Assert.Equal(MealPlanStatus.Archived, plan.Status);
        var archived = Assert.Single(plan.DomainEvents.OfType<MealPlanArchived>());
        Assert.Equal(plan.Id, archived.MealPlanId);
        Assert.Equal(correlationId, archived.CorrelationId);
    }

    [Fact]
    public void Archive_AlreadyArchived_Throws()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);
        plan.Archive();

        Assert.Throws<InvalidOperationException>(() => plan.Archive());
    }

    [Fact]
    public void ClearDomainEvents_EmptiesEventList()
    {
        var plan = MealPlan.CreateDraft(HouseholdId, WeekStart);

        plan.ClearDomainEvents();

        Assert.Empty(plan.DomainEvents);
    }

    private static Recipe CreateRecipe() =>
        Recipe.Create("Stew", null, ["Step 1"], 20, 30, 4);
}
