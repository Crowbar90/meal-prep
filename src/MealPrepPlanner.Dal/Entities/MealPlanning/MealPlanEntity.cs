namespace MealPrepPlanner.Dal.Entities.MealPlanning;

using MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence projection of <c>MealPrepPlanner.Domain.MealPlanning.MealPlan</c>.
/// </summary>
public class MealPlanEntity
{
    public Guid Id { get; set; }

    public Guid HouseholdId { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public string Status { get; set; } = "draft";

    public decimal? TotalEstimatedCost { get; set; }

    public int? TotalCookingTimeMinutes { get; set; }

    public MealPlanNutritionSummaryDocument? NutritionSummary { get; set; }

    public int Version { get; set; }

    public Guid? WorkflowId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<MealSlotEntity> Slots { get; set; } = [];
}
