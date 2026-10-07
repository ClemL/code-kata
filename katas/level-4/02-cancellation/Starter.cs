namespace Katas.L4.Cancellation;

public static class CancellationKata
{
    public static int Sum(IEnumerable<int> values, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public static Task<T> RunWithTimeoutAsync<T>(
        Func<CancellationToken, Task<T>> operation,
        TimeSpan timeout,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public static Task<int> PollUntilAsync(
        Func<CancellationToken, Task<bool>> condition,
        TimeSpan interval,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public static Task WhenCancelled(CancellationToken token) => throw new NotImplementedException();

    public static Task<(int Completed, bool WasCancelled)> ProcessAllAsync(
        IEnumerable<string> items,
        Func<string, CancellationToken, Task> handler,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
