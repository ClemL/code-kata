using System.Globalization;

namespace Katas.L5.Monads;

public readonly struct Option<T> : IEquatable<Option<T>>
{
    public static Option<T> None => throw new NotImplementedException();

    public bool IsSome => throw new NotImplementedException();

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) => throw new NotImplementedException();

    public Option<TResult> Map<TResult>(Func<T, TResult> mapper) => throw new NotImplementedException();

    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) => throw new NotImplementedException();

    public T GetValueOrDefault(T fallback) => throw new NotImplementedException();

    public Option<TResult> Select<TResult>(Func<T, TResult> selector) => throw new NotImplementedException();

    public Option<TResult> SelectMany<TMiddle, TResult>(
        Func<T, Option<TMiddle>> binder,
        Func<T, TMiddle, TResult> projector) =>
        throw new NotImplementedException();

    public Option<T> Where(Func<T, bool> predicate) => throw new NotImplementedException();

    public bool Equals(Option<T> other) => throw new NotImplementedException();

    public override bool Equals(object? obj) => throw new NotImplementedException();

    public override int GetHashCode() => throw new NotImplementedException();

    public override string ToString() => throw new NotImplementedException();

    public static bool operator ==(Option<T> left, Option<T> right) => throw new NotImplementedException();

    public static bool operator !=(Option<T> left, Option<T> right) => throw new NotImplementedException();
}

public static class Option
{
    public static Option<T> Some<T>(T value) => throw new NotImplementedException();
}

public static class Parse
{
    public static Option<int> Int(string? text) => throw new NotImplementedException();
}

public static class OptionExtensions
{
    public static Option<TValue> Find<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) =>
        throw new NotImplementedException();
}

public static class Endpoints
{
    public static Option<string> Build(IReadOnlyDictionary<string, string> config) => throw new NotImplementedException();
}
