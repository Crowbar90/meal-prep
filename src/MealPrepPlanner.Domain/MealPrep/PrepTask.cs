namespace MealPrepPlanner.Domain.MealPrep;

using MealPrepPlanner.Domain.Shared;
using MealPrepPlanner.Domain.UserPreferences;

/// <summary>
/// A single batch-cooking or prep step within a <see cref="PrepSchedule"/>.
/// Child entity, mutated only through the schedule aggregate.
/// </summary>
public class PrepTask : Entity
{
    private readonly List<string> _recipesUsing = [];
    private readonly List<Equipment> _equipmentNeeded = [];

    private PrepTask()
    {
        Description = string.Empty;
    }

    internal PrepTask(
        Guid id,
        DayOfWeek day,
        string description,
        int durationMinutes,
        string? reheatingInstructions)
        : base(id)
    {
        Day = day;
        Description = description;
        DurationMinutes = durationMinutes;
        ReheatingInstructions = reheatingInstructions;
    }

    public DayOfWeek Day { get; }

    public string Description { get; }

    public IReadOnlyList<string> RecipesUsing => _recipesUsing;

    public int DurationMinutes { get; }

    public IReadOnlyList<Equipment> EquipmentNeeded => _equipmentNeeded;

    public string? ReheatingInstructions { get; }

    public static PrepTask Create(
        DayOfWeek day,
        string description,
        int durationMinutes,
        IReadOnlyList<string>? recipesUsing = null,
        IReadOnlyList<Equipment>? equipmentNeeded = null,
        string? reheatingInstructions = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Prep task description must not be empty.", nameof(description));

        if (durationMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(durationMinutes), "Duration must be a positive number.");

        var task = new PrepTask(Guid.NewGuid(), day, description, durationMinutes, reheatingInstructions);
        task._recipesUsing.AddRange(recipesUsing ?? []);
        task._equipmentNeeded.AddRange(equipmentNeeded ?? []);
        return task;
    }
}
