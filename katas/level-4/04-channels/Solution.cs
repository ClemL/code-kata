using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Katas.L4.Channels;

public static class ChannelKata
{
    public static Channel<T> CreateWorkQueue<T>(int capacity) =>
        Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });

    public static Channel<T> CreateLatestOnly<T>(int capacity) =>
        Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
        });

    public static async Task<IReadOnlyList<TOut>> ProcessAsync<TIn, TOut>(
        IEnumerable<TIn> inputs,
        Func<TIn, CancellationToken, Task<TOut>> handler,
        int workerCount,
        int capacity,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workerCount);

        var channel = Channel.CreateBounded<TIn>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = true,
        });
        var results = new ConcurrentBag<TOut>();
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var producer = Task.Run(async () =>
        {
            try
            {
                foreach (var input in inputs)
                {
                    await channel.Writer.WriteAsync(input, cts.Token);
                }

                channel.Writer.Complete();
            }
            catch (Exception ex)
            {
                channel.Writer.TryComplete(ex);
            }
        });

        var workers = Enumerable.Range(0, workerCount).Select(_ => Task.Run(async () =>
        {
            await foreach (var item in channel.Reader.ReadAllAsync(cts.Token))
            {
                try
                {
                    results.Add(await handler(item, cts.Token));
                }
                catch
                {
                    await cts.CancelAsync();
                    throw;
                }
            }
        }));

        await Task.WhenAll([producer, .. workers]);
        return [.. results];
    }

    public static ChannelReader<T> Merge<T>(IReadOnlyList<ChannelReader<T>> readers, CancellationToken cancellationToken = default)
    {
        var output = Channel.CreateUnbounded<T>();

        async Task Pump(ChannelReader<T> reader)
        {
            await foreach (var item in reader.ReadAllAsync(cancellationToken))
            {
                await output.Writer.WriteAsync(item, cancellationToken);
            }
        }

        var pumps = readers.Select(Pump).ToArray();
        _ = Task.WhenAll(pumps).ContinueWith(
            t => output.Writer.TryComplete(t.Exception?.InnerException),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        return output.Reader;
    }
}
