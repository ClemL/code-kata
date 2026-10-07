using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Katas.L4.Channels;

public static class ChannelKata
{
    public static Channel<T> CreateWorkQueue<T>(int capacity) => throw new NotImplementedException();

    public static Channel<T> CreateLatestOnly<T>(int capacity) => throw new NotImplementedException();

    public static Task<IReadOnlyList<TOut>> ProcessAsync<TIn, TOut>(
        IEnumerable<TIn> inputs,
        Func<TIn, CancellationToken, Task<TOut>> handler,
        int workerCount,
        int capacity,
        CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public static ChannelReader<T> Merge<T>(IReadOnlyList<ChannelReader<T>> readers, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
