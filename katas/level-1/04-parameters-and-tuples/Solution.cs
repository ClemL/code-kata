namespace Katas.L1.Parameters;

public readonly record struct Point3(double X, double Y, double Z);

public static class Args
{
    public static void Swap<T>(ref T a, ref T b) => (a, b) = (b, a);

    public static bool TryDivide(int dividend, int divisor, out int quotient, out int remainder)
    {
        if (divisor == 0)
        {
            quotient = 0;
            remainder = 0;
            return false;
        }

        (quotient, remainder) = Math.DivRem(dividend, divisor);
        return true;
    }

    public static int Sum(params ReadOnlySpan<int> values)
    {
        var total = 0;
        foreach (var value in values)
        {
            total += value;
        }

        return total;
    }

    public static (int Min, int Max) MinMax(params ReadOnlySpan<int> values)
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException("At least one value is required.", nameof(values));
        }

        int min = values[0], max = values[0];
        foreach (var value in values[1..])
        {
            if (value < min) min = value;
            if (value > max) max = value;
        }

        return (min, max);
    }

    public static string Greet(string name, int age = 0, string greeting = "Hello") =>
        age > 0 ? $"{greeting}, {name}! ({age})" : $"{greeting}, {name}!";

    public static double Distance(in Point3 a, in Point3 b)
    {
        var dx = a.X - b.X;
        var dy = a.Y - b.Y;
        var dz = a.Z - b.Z;
        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }
}
