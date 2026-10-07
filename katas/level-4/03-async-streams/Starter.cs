using System.Runtime.CompilerServices;

namespace Katas.L4.AsyncStreams;

public static class AsyncStreams
{
    public static IAsyncEnumerable<string> ReadLinesAsync(
        TextReader reader,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public static IAsyncEnumerable<T> PaginateAsync<T>(
        Func<int, CancellationToken, Task<IReadOnlyList<T>>> fetchPage,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public static IAsyncEnumerable<IReadOnlyList<T>> BatchAsync<T>(
        IAsyncEnumerable<T> source,
        int size,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public static IAsyncEnumerable<TResult> SelectAsync<T, TResult>(
        IAsyncEnumerable<T> source,
        Func<T, CancellationToken, ValueTask<TResult>> selector,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public static Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
