using System.Numerics;

namespace Katas.L1.Numbers;

public static class Numbers
{
    public static int CheckedAdd(int a, int b) => checked(a + b);

    public static int WrappingAdd(int a, int b) => unchecked(a + b);

    public static decimal RoundMoney(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    public static decimal RoundBankers(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.ToEven);

    public static bool NearlyEqual(double a, double b, double relativeTolerance = 1e-9)
    {
        if (a == b)
        {
            return true;
        }

        if (double.IsNaN(a) || double.IsNaN(b) || double.IsInfinity(a) || double.IsInfinity(b))
        {
            return false;
        }

        return Math.Abs(a - b) <= relativeTolerance * Math.Max(Math.Abs(a), Math.Abs(b));
    }

    public static bool IsPowerOfTwo(long n) => n > 0 && (n & (n - 1)) == 0;

    public static int CountSetBits(uint value) => BitOperations.PopCount(value);

    public static BigInteger Factorial(int n)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);

        BigInteger result = BigInteger.One;
        for (var i = 2; i <= n; i++)
        {
            result *= i;
        }

        return result;
    }
}
