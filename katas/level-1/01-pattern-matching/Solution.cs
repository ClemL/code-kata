namespace Katas.L1.PatternMatching;

public static class Patterns
{
    public static string FizzBuzz(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(n);

        return (n % 3, n % 5) switch
        {
            (0, 0) => "FizzBuzz",
            (0, _) => "Fizz",
            (_, 0) => "Buzz",
            _ => n.ToString(),
        };
    }

    public static string Grade(int score) => score switch
    {
        < 0 or > 100 => throw new ArgumentOutOfRangeException(nameof(score), score, "Score must be between 0 and 100."),
        >= 90 => "A",
        >= 80 => "B",
        >= 70 => "C",
        >= 60 => "D",
        _ => "F",
    };

    public static string Quadrant(int x, int y) => (x, y) switch
    {
        (0, 0) => "origin",
        (0, _) => "y-axis",
        (_, 0) => "x-axis",
        (> 0, > 0) => "Q1",
        (< 0, > 0) => "Q2",
        (< 0, < 0) => "Q3",
        _ => "Q4",
    };

    public static string Describe(object? value) => value switch
    {
        null => "null",
        0 => "zero",
        int and < 0 => "negative int",
        int => "positive int",
        string { Length: 0 } => "empty string",
        string s => $"string of length {s.Length}",
        bool b => b ? "yes" : "no",
        double d when double.IsNaN(d) => "not a number",
        double => "double",
        _ => $"unknown: {value.GetType().Name}",
    };

    public static bool IsWeekend(DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}
