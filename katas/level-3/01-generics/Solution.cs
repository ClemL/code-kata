using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Katas.L3.Generics;

public static class GenericKata
{
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        if (min.CompareTo(max) > 0)
        {
            throw new ArgumentException($"{nameof(min)} must not be greater than {nameof(max)}.");
        }

        if (value.CompareTo(min) < 0) return min;
        if (value.CompareTo(max) > 0) return max;
        return value;
    }

    public static T MaxOrDefault<T>(IEnumerable<T> source, T fallback) where T : IComparable<T>
    {
        using var e = source.GetEnumerator();
        if (!e.MoveNext())
        {
            return fallback;
        }

        var max = e.Current;
        while (e.MoveNext())
        {
            if (e.Current.CompareTo(max) > 0)
            {
                max = e.Current;
            }
        }

        return max;
    }

    public static T? ParseOrNull<T>(string? text) where T : struct, IParsable<T> =>
        T.TryParse(text, CultureInfo.InvariantCulture, out var value) ? value : null;
}

public sealed class Pool<T> where T : class, new()
{
    private readonly Stack<T> _available = new();

    public int Created { get; private set; }

    public T Rent()
    {
        if (_available.TryPop(out var item))
        {
            return item;
        }

        Created++;
        return new T();
    }

    public void Return(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        _available.Push(item);
    }
}

public sealed class TypeMap
{
    private readonly Dictionary<Type, object> _values = [];

    public void Set<T>(T value) where T : notnull => _values[typeof(T)] = value;

    public T Get<T>() =>
        TryGet<T>(out var value)
            ? value
            : throw new KeyNotFoundException($"No value registered for {typeof(T).Name}.");

    public bool TryGet<T>([MaybeNullWhen(false)] out T value)
    {
        if (_values.TryGetValue(typeof(T), out var boxed))
        {
            value = (T)boxed;
            return true;
        }

        value = default;
        return false;
    }
}

public interface IProducer<out T>
{
    T Produce();
}

public interface IConsumer<in T>
{
    void Consume(T item);
}

public sealed class ConstantProducer<T>(T value) : IProducer<T>
{
    public T Produce() => value;
}

public sealed class ListConsumer<T> : IConsumer<T>
{
    private readonly List<T> _items = [];

    public IReadOnlyList<T> Items => _items;

    public void Consume(T item) => _items.Add(item);
}
