using System.Numerics;

namespace Katas.L5.GenericMath;

public static class GenericStats
{
    public static T Sum<T>(IEnumerable<T> values) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T>
    {
        var total = T.AdditiveIdentity;
        foreach (var value in values)
        {
            total += value;
        }

        return total;
    }

    public static TResult Average<T, TResult>(IEnumerable<T> values)
        where T : INumberBase<T>
        where TResult : INumberBase<TResult>
    {
        var total = TResult.Zero;
        var count = 0;

        foreach (var value in values)
        {
            total += TResult.CreateChecked(value);
            count++;
        }

        if (count == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        return total / TResult.CreateChecked(count);
    }

    public static T StandardDeviation<T>(IReadOnlyCollection<T> values) where T : IFloatingPointIeee754<T>
    {
        if (values.Count == 0)
        {
            throw new InvalidOperationException("Sequence contains no elements.");
        }

        var n = T.CreateChecked(values.Count);
        var mean = Sum(values) / n;

        var squaredDeviations = T.Zero;
        foreach (var value in values)
        {
            var deviation = value - mean;
            squaredDeviations += deviation * deviation;
        }

        return T.Sqrt(squaredDeviations / n);
    }

    public static T Gcd<T>(T a, T b) where T : IBinaryInteger<T>
    {
        while (b != T.Zero)
        {
            (a, b) = (b, a % b);
        }

        return T.Abs(a);
    }

    public static T Fold<T>(IEnumerable<T> items) where T : IMonoid<T> =>
        items.Aggregate(T.Identity, T.Combine);
}

public readonly record struct Money(decimal Amount) : IAdditionOperators<Money, Money, Money>, IAdditiveIdentity<Money, Money>
{
    public static Money AdditiveIdentity => new(0m);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);
}

public interface IMonoid<TSelf> where TSelf : IMonoid<TSelf>
{
    static abstract TSelf Identity { get; }

    static abstract TSelf Combine(TSelf left, TSelf right);
}

public readonly record struct MaxInt(int Value) : IMonoid<MaxInt>
{
    public static MaxInt Identity => new(int.MinValue);

    public static MaxInt Combine(MaxInt left, MaxInt right) => new(Math.Max(left.Value, right.Value));
}

public readonly record struct Csv(string Value) : IMonoid<Csv>
{
    public static Csv Identity => new(string.Empty);

    public static Csv Combine(Csv left, Csv right) =>
        (left.Value.Length, right.Value.Length) switch
        {
            (0, _) => right,
            (_, 0) => left,
            _ => new Csv($"{left.Value},{right.Value}"),
        };
}
