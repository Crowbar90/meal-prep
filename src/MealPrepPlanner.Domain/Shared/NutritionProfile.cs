namespace MealPrepPlanner.Domain.Shared;

/// <summary>
/// Macro- and micronutrient values for one serving or per 100g.
/// </summary>
public readonly record struct NutritionProfile(
    decimal Calories,
    decimal Protein,
    decimal Carbs,
    decimal Fat,
    decimal? Fiber = null,
    decimal? Sugar = null,
    decimal? SodiumMg = null,
    decimal? IronMg = null,
    decimal? CalciumMg = null,
    decimal? VitaminCMg = null)
{
    public static NutritionProfile operator +(NutritionProfile left, NutritionProfile right) =>
        new(
            left.Calories + right.Calories,
            left.Protein + right.Protein,
            left.Carbs + right.Carbs,
            left.Fat + right.Fat,
            Combine(left.Fiber, right.Fiber),
            Combine(left.Sugar, right.Sugar),
            Combine(left.SodiumMg, right.SodiumMg),
            Combine(left.IronMg, right.IronMg),
            Combine(left.CalciumMg, right.CalciumMg),
            Combine(left.VitaminCMg, right.VitaminCMg));

    private static decimal? Combine(decimal? left, decimal? right) =>
        left + right ?? left ?? right;

    public NutritionProfile Scale(decimal factor) =>
        new(
            Calories * factor,
            Protein * factor,
            Carbs * factor,
            Fat * factor,
            Fiber * factor,
            Sugar * factor,
            SodiumMg * factor,
            IronMg * factor,
            CalciumMg * factor,
            VitaminCMg * factor);

    public NutritionProfile Round(int decimals) =>
        new(
            Math.Round(Calories, decimals, MidpointRounding.AwayFromZero),
            Math.Round(Protein, decimals, MidpointRounding.AwayFromZero),
            Math.Round(Carbs, decimals, MidpointRounding.AwayFromZero),
            Math.Round(Fat, decimals, MidpointRounding.AwayFromZero),
            Fiber is null ? null : Math.Round(Fiber.Value, decimals, MidpointRounding.AwayFromZero),
            Sugar is null ? null : Math.Round(Sugar.Value, decimals, MidpointRounding.AwayFromZero),
            SodiumMg is null ? null : Math.Round(SodiumMg.Value, decimals, MidpointRounding.AwayFromZero),
            IronMg is null ? null : Math.Round(IronMg.Value, decimals, MidpointRounding.AwayFromZero),
            CalciumMg is null ? null : Math.Round(CalciumMg.Value, decimals, MidpointRounding.AwayFromZero),
            VitaminCMg is null ? null : Math.Round(VitaminCMg.Value, decimals, MidpointRounding.AwayFromZero));
}
