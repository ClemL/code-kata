namespace Katas.L4.AsyncBasics;

public static class AsyncKata
{
    public static Task<TValue[]> FetchAllAsync<TKey, TValue>(IEnumerable<TKey> keys, Func<TKey, Task<TValue>> fetch)
    {
        var tasks = keys.Select(fetch).ToList();
        return Task.WhenAll(tasks);
    }

    public static Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout) => task.WaitAsync(timeout);

    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxAttempts,
        TimeSpan baseDelay,
        Func<TimeSpan, Task> delay)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxAttempts);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception) when (attempt < maxAttempts)
            {
                await delay(baseDelay * Math.Pow(2, attempt - 1));
            }
        }
    }

    public static async Task<IReadOnlyList<string>> CollectErrorsAsync(IEnumerable<Task> tasks)
    {
        var all = Task.WhenAll(tasks);
        await all.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

        return all.Exception?.InnerExceptions.Select(e => e.Message).ToList() ?? [];
    }

    public static async Task<T> FirstSuccessfulAsync<T>(
        IReadOnlyList<Func<CancellationToken, Task<T>>> attempts,
        CancellationToken cancellationToken = default)
    {
        if (attempts.Count == 0)
        {
            throw new ArgumentException("At least one attempt is required.", nameof(attempts));
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var tasks = attempts.Select(attempt => attempt(cts.Token)).ToList();
        var failures = new List<Exception>();

        await foreach (var task in Task.WhenEach(tasks))
        {
            if (task.IsCompletedSuccessfully)
            {
                await cts.CancelAsync();
                return task.Result;
            }

            failures.Add(task.Exception?.InnerException ?? new TaskCanceledException(task));
        }

        throw new AggregateException("Every attempt failed.", failures);
    }
}

public sealed class CachedLookup(Func<int, Task<string>> loader)
{
    private readonly Dictionary<int, string> _cache = [];

    public int LoaderCalls { get; private set; }

    public ValueTask<string> GetAsync(int id) =>
        _cache.TryGetValue(id, out var cached)
            ? ValueTask.FromResult(cached)
            : new ValueTask<string>(LoadAsync(id));

    private async Task<string> LoadAsync(int id)
    {
        LoaderCalls++;
        var value = await loader(id);
        _cache[id] = value;
        return value;
    }
}
