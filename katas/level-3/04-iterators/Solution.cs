using System.Collections;

namespace Katas.L3.Iterators;

public static class Sequences
{
    public static IEnumerable<long> Fibonacci()
    {
        long a = 0, b = 1;
        while (true)
        {
            yield return a;
            (a, b) = (b, a + b);
        }
    }

    public static IEnumerable<int> Range(int start, int endExclusive, int step)
    {
        if (step == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(step), "Step must not be zero.");
        }

        return Iterate();

        IEnumerable<int> Iterate()
        {
            for (var value = start; step > 0 ? value < endExclusive : value > endExclusive; value += step)
            {
                yield return value;
            }
        }
    }

    public static IEnumerable<T[]> Window<T>(IEnumerable<T> source, int size)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        return Iterate(source, size);

        static IEnumerable<T[]> Iterate(IEnumerable<T> source, int size)
        {
            var buffer = new Queue<T>(size);
            foreach (var item in source)
            {
                if (buffer.Count == size)
                {
                    buffer.Dequeue();
                }

                buffer.Enqueue(item);

                if (buffer.Count == size)
                {
                    yield return buffer.ToArray();
                }
            }
        }
    }

    public static IEnumerable<T> Interleave<T>(IEnumerable<T> first, IEnumerable<T> second)
    {
        using var a = first.GetEnumerator();
        using var b = second.GetEnumerator();

        bool hasA = true, hasB = true;
        while (hasA || hasB)
        {
            if (hasA && (hasA = a.MoveNext()))
            {
                yield return a.Current;
            }

            if (hasB && (hasB = b.MoveNext()))
            {
                yield return b.Current;
            }
        }
    }

    public static IEnumerable<T> DistinctUntilChanged<T>(IEnumerable<T> source)
    {
        var comparer = EqualityComparer<T>.Default;
        var hasPrevious = false;
        T previous = default!;

        foreach (var item in source)
        {
            if (hasPrevious && comparer.Equals(previous, item))
            {
                continue;
            }

            hasPrevious = true;
            previous = item;
            yield return item;
        }
    }
}

public sealed class Countdown(int from) : IEnumerable<int>
{
    public IEnumerator<int> GetEnumerator() => new Enumerator(from);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private sealed class Enumerator(int from) : IEnumerator<int>
    {
        private int _next = from;

        public int Current { get; private set; }

        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            if (_next <= 0)
            {
                return false;
            }

            Current = _next--;
            return true;
        }

        public void Reset() => throw new NotSupportedException();

        public void Dispose()
        {
        }
    }
}
