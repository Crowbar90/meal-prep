namespace MealPrepPlanner.Domain.Shared;

/// <summary>
/// An inclusive time window expressed in minutes offset from a reference
/// point (typically the prep week's Monday 00:00 UTC).
/// <see cref="EarliestStartOffsetMinutes"/> &lt;= <see cref="LatestFinishOffsetMinutes"/>.
/// </summary>
public sealed class TimeWindow : ValueObject
{
    public TimeWindow(int earliestStartOffsetMinutes, int latestFinishOffsetMinutes)
    {
        if (earliestStartOffsetMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(earliestStartOffsetMinutes), "Earliest start offset must be non-negative.");

        if (latestFinishOffsetMinutes < earliestStartOffsetMinutes)
            throw new ArgumentOutOfRangeException(
                nameof(latestFinishOffsetMinutes),
                $"Latest finish ({latestFinishOffsetMinutes}m) must be at or after earliest start ({earliestStartOffsetMinutes}m).");

        EarliestStartOffsetMinutes = earliestStartOffsetMinutes;
        LatestFinishOffsetMinutes = latestFinishOffsetMinutes;
    }

    public int EarliestStartOffsetMinutes { get; }

    public int LatestFinishOffsetMinutes { get; }

    public TimeSpan Duration => TimeSpan.FromMinutes(LatestFinishOffsetMinutes - EarliestStartOffsetMinutes);

    public bool OverlapsWith(TimeWindow other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return EarliestStartOffsetMinutes < other.LatestFinishOffsetMinutes
            && other.EarliestStartOffsetMinutes < LatestFinishOffsetMinutes;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return EarliestStartOffsetMinutes;
        yield return LatestFinishOffsetMinutes;
    }
}
