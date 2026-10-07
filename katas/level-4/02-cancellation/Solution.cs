namespace Katas.L4.Cancellation;

public static class CancellationKata
{
    public static int Sum(IEnumerable<int> values, CancellationToken cancellationToken)
    {
        var total = 0;
        foreach (var value in values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            total += value;
        }

        return total;
    }

    public static async Task<T> RunWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            return await operation(cts.Token);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && cts.IsCancellationRequested)
        {
            throw new TimeoutException($"The operation did not complete within {timeout}.", ex);
        }
    }

    public static async Task<int> PollUntilAsync(
        Func<CancellationToken, Task<bool>> condition,
        TimeSpan interval,
        CancellationToken cancellationToken)
    {
        var checks = 1;
        if (await condition(cancellationToken))
        {
            return checks;
        }

        using var timer = new PeriodicTimer(interval);
        while (await timer.WaitForNextTickAsync(cancellationToken))
        {
            checks++;
            if (await condition(cancellationToken))
            {
                return checks;
            }
        }

        throw new InvalidOperationException("The timer stopped unexpectedly.");
    }

    public static Task WhenCancelled(CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        token.Register(static state => ((TaskCompletionSource)state!).TrySetResult(), tcs);
        return tcs.Task;
    }

    public static async Task<(int Completed, bool WasCancelled)> ProcessAllAsync(
        IEnumerable<string> items,
        Func<string, CancellationToken, Task> handler,
        CancellationToken cancellationToken)
    {
        var completed = 0;
        try
        {
            foreach (var item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await handler(item, cancellationToken);
                completed++;
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return (completed, true);
        }

        return (completed, false);
    }
}
