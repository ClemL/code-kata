namespace Katas.L2.Extensions;

public static class StringExtensions
{
    public static string Truncate(this string value, int maxLength, string ellipsis = "...")
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, ellipsis.Length);

        return value.Length <= maxLength
            ? value
            : string.Concat(value.AsSpan(0, maxLength - ellipsis.Length), ellipsis);
    }

    public static string OrEmpty(this string? value) => value ?? string.Empty;
}

public static class SequenceExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
    {
        foreach (var item in source)
        {
            if (item is not null)
            {
                yield return item;
            }
        }
    }

    public static IEnumerable<(int Index, T Item)> WithIndex<T>(this IEnumerable<T> source)
    {
        var index = 0;
        foreach (var item in source)
        {
            yield return (index++, item);
        }
    }
}

public static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory)
    {
        if (dictionary.TryGetValue(key, out var existing))
        {
            return existing;
        }

        var created = factory(key);
        dictionary.Add(key, created);
        return created;
    }
}

public static class DateExtensions
{
    public static bool IsBetween(this DateOnly date, DateOnly start, DateOnly end)
    {
        if (start > end)
        {
            (start, end) = (end, start);
        }

        return date >= start && date <= end;
    }
}
