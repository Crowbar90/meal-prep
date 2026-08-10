namespace MealPrepPlanner.Tests.Unit.UserPreferences;

using MealPrepPlanner.Domain.UserPreferences;

public class EquipmentTests
{
    [Fact]
    public void Constructor_NormalizesName()
    {
        Assert.Equal("instant pot", new Equipment(" Instant Pot ").Name);
    }

    [Fact]
    public void Constructor_EmptyName_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => new Equipment("   "));

        Assert.Contains("must not be empty", ex.Message);
    }

    [Fact]
    public void Constructor_NullName_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Equipment(null!));
    }

    [Fact]
    public void Equality_ComparesNormalizedName()
    {
        Assert.Equal(new Equipment("Oven"), new Equipment("oven"));
        Assert.NotEqual(new Equipment("oven"), new Equipment("stove"));
    }

    [Fact]
    public void ToString_ReturnsNormalizedName()
    {
        Assert.Equal("air fryer", new Equipment(" Air Fryer ").ToString());
    }
}
