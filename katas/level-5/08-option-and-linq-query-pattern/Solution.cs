using System.Globalization;

namespace Katas.L5.Monads;

public readonly struct Option<T> : IEquatable<Option<T>>
{
    private readonly T _value;
    private readonly bool _hasValue;

    internal Option(T value)
    {
        _value = value;
        _hasValue = true;
    }

    public static Option<T> None => default;

    public bool IsSome => _hasValue;

    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none) =>
        _hasValue ? some(_value) : none();

    public Option<TResult> Map<TResult>(Func<T, TResult> mapper) =>
        _hasValue ? Option.Some(mapper(_value)) : Option<TResult>.None;

    public Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder) =>
        _hasValue ? binder(_value) : Option<TResult>.None;

    public T GetValueOrDefault(T fallback) => _hasValue ? _value : fallback;

    public Option<TResult> Select<TResult>(Func<T, TResult> selector) => Map(selector);

    public Option<TResult> SelectMany<TMiddle, TResult>(
        Func<T, Option<TMiddle>> binder,
        Func<T, TMiddle, TResult> projector)
    {
        var value = _value;
        return Bind(binder).Map(middle => projector(value, middle));
    }

    public Option<T> Where(Func<T, bool> predicate) =>
        _hasValue && predicate(_value) ? this : None;

    public bool Equals(Option<T> other) =>
        _hasValue == other._hasValue
        && (!_hasValue || EqualityComparer<T>.Default.Equals(_value, other._value));

    public override bool Equals(object? obj) => obj is Option<T> other && Equals(other);

    public override int GetHashCode() => _hasValue ? HashCode.Combine(true, _value) : 0;

    public override string ToString() => _hasValue ? $"Some({_value})" : "None";

    public static bool operator ==(Option<T> left, Option<T> right) => left.Equals(right);

    public static bool operator !=(Option<T> left, Option<T> right) => !left.Equals(right);
}

public static class Option
{
    public static Option<T> Some<T>(T value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Option<T>(value);
    }
}

public static class Parse
{
    public static Option<int> Int(string? text) =>
        int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? Option.Some(value)
            : Option<int>.None;
}

public static class OptionExtensions
{
    public static Option<TValue> Find<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key) =>
        dictionary.TryGetValue(key, out var value) && value is not null
            ? Option.Some(value)
            : Option<TValue>.None;
}

public static class Endpoints
{
    public static Option<string> Build(IReadOnlyDictionary<string, string> config) =>
        from host in config.Find("Host")
        from rawPort in config.Find("Port")
        from port in Parse.Int(rawPort)
        where port is > 0 and < 65536
        select $"{host}:{port}";
}
