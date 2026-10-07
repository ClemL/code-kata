using System.Globalization;

namespace Katas.L5.CSharp14;

public static class SequenceExtensions
{
    extension<T>(IEnumerable<T> source)
    {
        public bool IsEmpty => throw new NotImplementedException();
    }

    extension(string text)
    {
        public int WordCount => throw new NotImplementedException();
    }
}

public static class DateOnlyExtensions
{
    extension(DateOnly date)
    {
        public int Quarter => throw new NotImplementedException();

        public DateOnly EndOfMonth => throw new NotImplementedException();
    }

    extension(DateOnly)
    {
        public static DateOnly FirstOfMonth(int year, int month) => throw new NotImplementedException();

        public static DateOnly ParseIso(string text) => throw new NotImplementedException();

        public static int operator -(DateOnly left, DateOnly right) => throw new NotImplementedException();
    }
}

public sealed class Customer
{
    public DateTimeOffset? LastSeen { get; set; }

    public int LoyaltyPoints { get; set; }
}

public static class Accounts
{
    public static void Touch(Customer? customer, DateTimeOffset now) => throw new NotImplementedException();

    public static void AddPoints(Customer? customer, int points) => throw new NotImplementedException();
}

public delegate bool TryParser<T>(string? text, out T value);

public static class Parsers
{
    public static TryParser<int> Int => throw new NotImplementedException();

    public static TryParser<DateOnly> IsoDate => throw new NotImplementedException();
}

public static class Names
{
    public static (string List, string Dictionary) Of() => throw new NotImplementedException();
}

public sealed class RunningTotal
{
    public decimal Total => throw new NotImplementedException();

    public int Count => throw new NotImplementedException();

    public void operator +=(decimal amount) => throw new NotImplementedException();
}
