namespace MealPrepPlanner.Domain.MealPlanning.Events;

using MealPrepPlanner.Domain.Shared;

public sealed class MealPlanNutritionValidated : DomainEvent
{
    public MealPlanNutritionValidated(
        Guid mealPlanId,
        bool isValid,
        IReadOnlyList<string> warnings,
        NutritionProfile dailyProfile,
        Guid correlationId)
        : base(correlationId)
    {
        MealPlanId = mealPlanId;
        IsValid = isValid;
        Warnings = warnings;
        DailyProfile = dailyProfile;
    }

    public Guid MealPlanId { get; }

    public bool IsValid { get; }

    public IReadOnlyList<string> Warnings { get; }

    public NutritionProfile DailyProfile { get; }
}
