using System.Numerics;

namespace Katas.L5.GenericMath;

public static class GenericStats
{
    public static T Sum<T>(IEnumerable<T> values) where T : IAdditionOperators<T, T, T>, IAdditiveIdentity<T, T> =>
        throw new NotImplementedException();

    public static TResult Average<T, TResult>(IEnumerable<T> values)
        where T : INumberBase<T>
        where TResult : INumberBase<TResult> =>
        throw new NotImplementedException();

    public static T StandardDeviation<T>(IReadOnlyCollection<T> values) where T : IFloatingPointIeee754<T> =>
        throw new NotImplementedException();

    public static T Gcd<T>(T a, T b) where T : IBinaryInteger<T> => throw new NotImplementedException();

    public static T Fold<T>(IEnumerable<T> items) where T : IMonoid<T> => throw new NotImplementedException();
}

public readonly record struct Money(decimal Amount) : IAdditionOperators<Money, Money, Money>, IAdditiveIdentity<Money, Money>
{
    public static Money AdditiveIdentity => throw new NotImplementedException();

    public static Money operator +(Money left, Money right) => throw new NotImplementedException();
}

public interface IMonoid<TSelf> where TSelf : IMonoid<TSelf>
{
    static abstract TSelf Identity { get; }

    static abstract TSelf Combine(TSelf left, TSelf right);
}

public readonly record struct MaxInt(int Value) : IMonoid<MaxInt>
{
    public static MaxInt Identity => throw new NotImplementedException();

    public static MaxInt Combine(MaxInt left, MaxInt right) => throw new NotImplementedException();
}

public readonly record struct Csv(string Value) : IMonoid<Csv>
{
    public static Csv Identity => throw new NotImplementedException();

    public static Csv Combine(Csv left, Csv right) => throw new NotImplementedException();
}
