using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Katas.L3.Generics;

public static class GenericKata
{
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> => throw new NotImplementedException();

    public static T MaxOrDefault<T>(IEnumerable<T> source, T fallback) where T : IComparable<T> =>
        throw new NotImplementedException();

    public static T? ParseOrNull<T>(string? text) where T : struct, IParsable<T> => throw new NotImplementedException();
}

public sealed class Pool<T> where T : class, new()
{
    public int Created => throw new NotImplementedException();

    public T Rent() => throw new NotImplementedException();

    public void Return(T item) => throw new NotImplementedException();
}

public sealed class TypeMap
{
    public void Set<T>(T value) where T : notnull => throw new NotImplementedException();

    public T Get<T>() => throw new NotImplementedException();

    public bool TryGet<T>([MaybeNullWhen(false)] out T value) => throw new NotImplementedException();
}

// TODO: Add variance annotations so the tests' conversions succeed.
public interface IProducer<T>
{
    T Produce();
}

public interface IConsumer<T>
{
    void Consume(T item);
}

public sealed class ConstantProducer<T>(T value) : IProducer<T>
{
    public T Produce() => throw new NotImplementedException();
}

public sealed class ListConsumer<T> : IConsumer<T>
{
    public IReadOnlyList<T> Items => throw new NotImplementedException();

    public void Consume(T item) => throw new NotImplementedException();
}
