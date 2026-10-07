using System.Globalization;

namespace Katas.L5.CSharp14;

public static class SequenceExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public bool IsEmpty => !source.Any();
    }

    extension(string text)
    {
        public int WordCount => text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}

public static class DateOnlyExtensions
{
    extension(DateOnly date)
    {
        public int Quarter => (date.Month - 1) / 3 + 1;

        public DateOnly EndOfMonth => new(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
    }

    extension(DateOnly)
    {
        public static DateOnly FirstOfMonth(int year, int month) => new(year, month, 1);

        public static DateOnly ParseIso(string text) =>
            DateOnly.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);

        public static int operator -(DateOnly left, DateOnly right) => left.DayNumber - right.DayNumber;
    }
}

public sealed class Customer
{
    public DateTimeOffset? LastSeen { get; set; }

    public int LoyaltyPoints { get; set; }
}

public static class Accounts
{
    public static void Touch(Customer? customer, DateTimeOffset now) => customer?.LastSeen = now;

    public static void AddPoints(Customer? customer, int points) => customer?.LoyaltyPoints += points;
}

public delegate bool TryParser<T>(string? text, out T value);

public static class Parsers
{
    public static TryParser<int> Int { get; } =
        (text, out value) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

    public static TryParser<DateOnly> IsoDate { get; } =
        (text, out value) => DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
}

public static class Names
{
    public static (string List, string Dictionary) Of() => (nameof(List<>), nameof(Dictionary<,>));
}

public sealed class RunningTotal
{
    public decimal Total { get; private set; }

    public int Count { get; private set; }

    public void operator +=(decimal amount)
    {
        Total += amount;
        Count++;
    }
}
