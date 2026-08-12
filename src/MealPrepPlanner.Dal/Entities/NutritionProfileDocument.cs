namespace MealPrepPlanner.Dal.Entities;

/// <summary>
/// Persistence-layer projection of a nutrition-per-100g record. Maps to <c>jsonb</c>.
/// Decimal fields keep raw precision; rounding belongs to the domain services.
/// </summary>
public sealed class NutritionProfileDocument
{
    public decimal Calories { get; set; }

    public decimal Protein { get; set; }

    public decimal Carbs { get; set; }

    public decimal Fat { get; set; }

    public decimal? Fiber { get; set; }

    public decimal? Sugar { get; set; }

    public decimal? SodiumMg { get; set; }

    public decimal? IronMg { get; set; }

    public decimal? CalciumMg { get; set; }

    public decimal? VitaminCMg { get; set; }

    public decimal? VitaminDIu { get; set; }
}
