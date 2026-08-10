namespace MealPrepPlanner.Domain.UserPreferences;

using MealPrepPlanner.Domain.Shared;

/// <summary>
/// An individual within a household. Child entity of the <see cref="Household"/>
/// aggregate; mutated only through it.
/// </summary>
public class HouseholdMember : Entity
{
    private HouseholdMember()
    {
        Name = string.Empty;
    }

    internal HouseholdMember(
        Guid id,
        string name,
        int age,
        Sex sex,
        decimal weightKg,
        int heightCm,
        ActivityLevel activityLevel)
        : base(id)
    {
        Name = name;
        Age = age;
        Sex = sex;
        WeightKg = weightKg;
        HeightCm = heightCm;
        ActivityLevel = activityLevel;
    }

    public string Name { get; }

    public int Age { get; }

    public Sex Sex { get; }

    public decimal WeightKg { get; }

    public int HeightCm { get; }

    public ActivityLevel ActivityLevel { get; }
}
