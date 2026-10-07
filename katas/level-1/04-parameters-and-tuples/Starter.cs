namespace Katas.L1.Parameters;

public readonly record struct Point3(double X, double Y, double Z);

public static class Args
{
    public static void Swap<T>(ref T a, ref T b) => throw new NotImplementedException();

    public static bool TryDivide(int dividend, int divisor, out int quotient, out int remainder) =>
        throw new NotImplementedException();

    public static int Sum(params ReadOnlySpan<int> values) => throw new NotImplementedException();

    public static (int Min, int Max) MinMax(params ReadOnlySpan<int> values) => throw new NotImplementedException();

    public static string Greet(string name, int age = 0, string greeting = "Hello") =>
        throw new NotImplementedException();

    public static double Distance(in Point3 a, in Point3 b) => throw new NotImplementedException();
}
