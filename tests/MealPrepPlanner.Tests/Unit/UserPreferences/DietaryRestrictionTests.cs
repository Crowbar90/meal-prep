namespace MealPrepPlanner.Tests.Unit.UserPreferences;

using MealPrepPlanner.Domain.UserPreferences;

public class DietaryRestrictionTests
{
    [Fact]
    public void Constructor_NormalizesName()
    {
        Assert.Equal("nut-free", new DietaryRestriction(" Nut-Free ").Name);
        Assert.Equal("vegan", new DietaryRestriction("VEGAN").Name);
    }

    [Fact]
    public void ConflictsWith_ExactName_ReturnsTrue()
    {
        var restriction = new DietaryRestriction("vegan");

        Assert.True(restriction.ConflictsWith("Vegan"));
    }

    [Fact]
    public void ConflictsWith_FreeForm_MatchesForbiddenSubstring()
    {
        var restriction = new DietaryRestriction("nut-free");

        Assert.True(restriction.ConflictsWith("peanuts"));
        Assert.True(restriction.ConflictsWith("hazelnut"));
        Assert.False(restriction.ConflictsWith("soy"));
    }

    [Fact]
    public void ConflictsWith_NoForm_MatchesForbiddenSubstring()
    {
        var restriction = new DietaryRestriction("no-dairy");

        Assert.True(restriction.ConflictsWith("dairy"));
        Assert.False(restriction.ConflictsWith("milk"));
    }

    [Fact]
    public void ConflictsWith_SubstringForms_MatchEitherDirection()
    {
        var restriction = new DietaryRestriction("soy");

        Assert.True(restriction.ConflictsWith("soy sauce"));
    }

    [Fact]
    public void ConflictsWith_EmptyAllergen_ReturnsFalse()
    {
        var restriction = new DietaryRestriction("nut-free");

        Assert.False(restriction.ConflictsWith("   "));
    }

    [Fact]
    public void ConflictsWith_UnrelatedNames_ReturnsFalse()
    {
        var restriction = new DietaryRestriction("gluten");

        Assert.False(restriction.ConflictsWith("chicken"));
    }
}
