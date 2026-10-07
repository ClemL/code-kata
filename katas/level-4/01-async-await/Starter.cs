namespace Katas.L4.AsyncBasics;

public static class AsyncKata
{
    public static Task<TValue[]> FetchAllAsync<TKey, TValue>(IEnumerable<TKey> keys, Func<TKey, Task<TValue>> fetch) =>
        throw new NotImplementedException();

    public static Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout) => throw new NotImplementedException();

    public static Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts,
        TimeSpan baseDelay,
        Func<TimeSpan, Task> delay) =>
        throw new NotImplementedException();

    public static Task<IReadOnlyList<string>> CollectErrorsAsync(IEnumerable<Task> tasks) =>
        throw new NotImplementedException();

    public static Task<T> FirstSuccessfulAsync<T>(
        IReadOnlyList<Func<CancellationToken, Task<T>>> attempts,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}

public sealed class CachedLookup(Func<int, Task<string>> loader)
{
    public int LoaderCalls => throw new NotImplementedException();

    public ValueTask<string> GetAsync(int id) => throw new NotImplementedException();
}
