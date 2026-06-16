using Atlas.Party.Domain.Shared.Exceptions;
using Atlas.SharedKernel.Domain;

namespace Atlas.Party.Domain.Shared;

/// <summary>
/// Closed date range [Start, End]. End is optional (open-ended means currently active).
/// </summary>
public sealed class DateRange : ValueObject
{
    public DateOnly Start { get; }
    public DateOnly? End { get; }

    public bool IsActive => End is null || End >= DateOnly.FromDateTime(DateTime.UtcNow);

    private DateRange(DateOnly start, DateOnly? end)
    {
        Start = start;
        End = end;
    }

    public static DateRange Create(DateOnly start, DateOnly? end = null)
    {
        if (end.HasValue && end.Value < start)
            throw new InvalidDateRangeException(start, end.Value);

        return new DateRange(start, end);
    }

    public static DateRange OpenEnded(DateOnly start) => Create(start);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End ?? DateOnly.MaxValue;
    }
}
