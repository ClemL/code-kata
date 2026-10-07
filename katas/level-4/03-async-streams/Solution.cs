using System.Runtime.CompilerServices;

namespace Katas.L4.AsyncStreams;

public static class AsyncStreams
{
    public static async IAsyncEnumerable<string> ReadLinesAsync(
        TextReader reader,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            yield return line;
        }
    }

    public static async IAsyncEnumerable<T> PaginateAsync<T>(
        Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var page = 1; ; page++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var items = await fetchPage(page, cancellationToken);
            if (items.Count == 0)
            {
                yield break;
            }

            foreach (var item in items)
            {
                yield return item;
            }
        }
    }

    public static async IAsyncEnumerable<IReadOnlyList<T>> BatchAsync<T>(
        IAsyncEnumerable<T> source,
        int size,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        var batch = new List<T>(size);
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new List<T>(size);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    public static async IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
        IAsyncEnumerable<T> source,
        Func<T, CancellationToken, ValueTask<TResult>> selector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            yield return await selector(item, cancellationToken);
        }
    }

    public static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken = default)
    {
        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }

        return list;
    }
}
