namespace MealPrepPlanner.Tests.Unit.Shared;

using MealPrepPlanner.Domain.Shared;

public class NutritionProfileTests
{
    [Fact]
    public void Add_SumsMacros()
    {
        var first = new NutritionProfile(100m, 10m, 20m, 5m);
        var second = new NutritionProfile(200m, 20m, 40m, 10m);

        var sum = first + second;

        Assert.Equal(new NutritionProfile(300m, 30m, 60m, 15m), sum);
    }

    [Fact]
    public void Add_CombinesNullableMicronutrients()
    {
        var withFiber = new NutritionProfile(0m, 0m, 0m, 0m, Fiber: 3m);
        var withSodium = new NutritionProfile(0m, 0m, 0m, 0m, SodiumMg: 500m);

        var sum = withFiber + withSodium;

        Assert.Equal(3m, sum.Fiber);
        Assert.Equal(500m, sum.SodiumMg);
    }

    [Fact]
    public void Add_TwoNullMicronutrients_StaysNull()
    {
        var sum = new NutritionProfile(0m, 0m, 0m, 0m) + new NutritionProfile(0m, 0m, 0m, 0m);

        Assert.Null(sum.Fiber);
        Assert.Null(sum.SodiumMg);
    }

    [Fact]
    public void Scale_MultipliesMacrosAndMicronutrients()
    {
        var profile = new NutritionProfile(100m, 10m, 20m, 5m, Fiber: 3m, SodiumMg: 500m);

        var scaled = profile.Scale(1.5m);

        Assert.Equal(new NutritionProfile(150m, 15m, 30m, 7.5m, Fiber: 4.5m, SodiumMg: 750m), scaled);
    }

    [Fact]
    public void Scale_PreservesNullMicronutrients()
    {
        var scaled = new NutritionProfile(10m, 1m, 2m, 3m).Scale(2m);

        Assert.Null(scaled.Fiber);
    }

    [Fact]
    public void Round_RoundsAllValuesAwayFromZero()
    {
        var profile = new NutritionProfile(12.34m, 5.67m, 8.91m, 1.005m, Fiber: 2.345m);

        var rounded = profile.Round(1);

        Assert.Equal(new NutritionProfile(12.3m, 5.7m, 8.9m, 1.0m, Fiber: 2.3m), rounded);
    }
}
