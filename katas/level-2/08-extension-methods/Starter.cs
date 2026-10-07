namespace Katas.L2.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength, string ellipsis = "...") =>
        throw new NotImplementedException();

    public static string OrEmpty(this string? value) => throw new NotImplementedException();
}

public static class SequenceExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class =>
        throw new NotImplementedException();

    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source) =>
        throw new NotImplementedException();
}

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory) =>
        throw new NotImplementedException();
}

public static class DateExtensions
{
    public static bool IsBetween(this DateOnly date, DateOnly start, DateOnly end) => throw new NotImplementedException();
}
