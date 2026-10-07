using System.Collections;

namespace Katas.L3.Iterators;

public static class Sequences
{
    public static IEnumerable<long> Fibonacci() => throw new NotImplementedException();

    public static IEnumerable<int> Range(int start, int endExclusive, int step) => throw new NotImplementedException();

    public static IEnumerable<T[]> Window<T>(IEnumerable<T> source, int size) => throw new NotImplementedException();

    public static IEnumerable<T> Interleave<T>(IEnumerable<T> first, IEnumerable<T> second) =>
        throw new NotImplementedException();

    public static IEnumerable<T> DistinctUntilChanged<T>(IEnumerable<T> source) => throw new NotImplementedException();
}

// Implement without `yield`: write the enumerator class by hand.
public sealed class Countdown(int from) : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
