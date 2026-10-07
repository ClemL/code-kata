using System.Globalization;

namespace Katas.L1.Dates;

public static class DateKata
{
    public static int AgeOn(DateOnly birthDate, DateOnly onDate)
    {
        if (onDate < birthDate)
        {
            throw new ArgumentOutOfRangeException(nameof(onDate), "The date must not be before the birth date.");
        }

        var age = onDate.Year - birthDate.Year;
        if (birthDate > onDate.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    public static int BusinessDaysBetween(DateOnly start, DateOnly endExclusive)
    {
        if (endExclusive < start)
        {
            throw new ArgumentException("The end date must not be before the start date.", nameof(endExclusive));
        }

        var count = 0;
        for (var day = start; day < endExclusive; day = day.AddDays(1))
        {
            if (day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
            {
                count++;
            }
        }

        return count;
    }

    public static DateOnly NextOccurrence(DateOnly from, DayOfWeek day)
    {
        var offset = ((int)day - (int)from.DayOfWeek + 7) % 7;
        return from.AddDays(offset == 0 ? 7 : offset);
    }

    public static DateOnly EndOfMonth(DateOnly date) =>
        new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));

    public static int QuarterOf(DateOnly date) => (date.Month - 1) / 3 + 1;

    public static DateOnly? ParseIsoDate(string? text) =>
        DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
            ? date
            : null;

    public static bool IsSameInstant(DateTimeOffset a, DateTimeOffset b) => a == b;

    public static string ToUtcStamp(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    public static string FormatDuration(TimeSpan span)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(span, TimeSpan.Zero);

        var clock = span.ToString(@"hh\h\ mm\m\ ss\s", CultureInfo.InvariantCulture);
        return span.Days > 0 ? $"{span.Days}d {clock}" : clock;
    }
}
