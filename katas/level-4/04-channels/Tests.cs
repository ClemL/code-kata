using System.Threading.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L4.Channels;

[TestClass]
public class ChannelsTests
{
    [TestMethod]
    public async Task WorkQueue_AppliesBackPressure()
    {
        var queue = ChannelKata.CreateWorkQueue<int>(2);

        Assert.IsTrue(queue.Writer.TryWrite(1));
        Assert.IsTrue(queue.Writer.TryWrite(2));
        Assert.IsFalse(queue.Writer.TryWrite(3), "A full Wait-mode channel rejects TryWrite.");

        Assert.AreEqual(1, await queue.Reader.ReadAsync());
        Assert.IsTrue(queue.Writer.TryWrite(3));
    }

    [TestMethod]
    public async Task WorkQueue_WriteAsyncWaitsForSpace()
    {
        var queue = ChannelKata.CreateWorkQueue<int>(1);
        await queue.Writer.WriteAsync(1);

        var pending = queue.Writer.WriteAsync(2).AsTask();
        Assert.IsFalse(pending.IsCompleted);

        await queue.Reader.ReadAsync();
        await pending.WaitAsync(TimeSpan.FromSeconds(5));
    }

    [TestMethod]
    public async Task LatestOnly_DropsOldest()
    {
        var channel = ChannelKata.CreateLatestOnly<int>(2);
        for (var i = 1; i <= 5; i++)
        {
            Assert.IsTrue(channel.Writer.TryWrite(i));
        }

        channel.Writer.Complete();

        var seen = new List<int>();
        await foreach (var item in channel.Reader.ReadAllAsync())
        {
            seen.Add(item);
        }

        CollectionAssert.AreEqual(new[] { 4, 5 }, seen);
    }

    [TestMethod]
    public async Task Process_HandlesEveryItemWithBoundedConcurrency()
    {
        var inFlight = 0;
        var maxInFlight = 0;

        var results = await ChannelKata.ProcessAsync(Enumerable.Range(1, 40), async (n, ct) =>
        {
            var now = Interlocked.Increment(ref inFlight);
            InterlockedMax(ref maxInFlight, now);
            await Task.Delay(5, ct);
            Interlocked.Decrement(ref inFlight);
            return n * n;
        }, workerCount: 4, capacity: 8);

        CollectionAssert.AreEquivalent(Enumerable.Range(1, 40).Select(n => n * n).ToArray(), results.ToArray());
        Assert.IsLessThanOrEqualTo(4, maxInFlight, "Never more handlers than workers.");
        Assert.IsGreaterThan(1, maxInFlight, "Workers should run concurrently.");
    }

    [TestMethod]
    public async Task Process_PropagatesHandlerFailures()
    {
        var processed = 0;

        var ex = await Assert.ThrowsExactlyAsync<InvalidDataException>(() => ChannelKata.ProcessAsync<int, int>(
            Enumerable.Range(1, 1_000),
            async (n, ct) =>
            {
                await Task.Delay(1, ct);
                Interlocked.Increment(ref processed);
                return n == 5 ? throw new InvalidDataException("bad record 5") : n;
            },
            workerCount: 2,
            capacity: 4));

        Assert.AreEqual("bad record 5", ex.Message);
        Assert.IsLessThan(1_000, processed, "Other workers should stop after a failure.");
    }

    [TestMethod]
    public async Task Merge_YieldsEverythingAndCompletes()
    {
        var a = Channel.CreateUnbounded<string>();
        var b = Channel.CreateUnbounded<string>();
        var merged = ChannelKata.Merge([a.Reader, b.Reader]);

        await a.Writer.WriteAsync("a1");
        await b.Writer.WriteAsync("b1");
        await a.Writer.WriteAsync("a2");
        a.Writer.Complete();
        b.Writer.Complete();

        var seen = new List<string>();
        await foreach (var item in merged.ReadAllAsync().WithCancellation(new CancellationTokenSource(TimeSpan.FromSeconds(5)).Token))
        {
            seen.Add(item);
        }

        CollectionAssert.AreEquivalent(new[] { "a1", "a2", "b1" }, seen);
        Assert.IsTrue(merged.Completion.IsCompletedSuccessfully);
    }

    [TestMethod]
    public async Task Merge_PropagatesInputFailure()
    {
        var good = Channel.CreateUnbounded<int>();
        var bad = Channel.CreateUnbounded<int>();
        var merged = ChannelKata.Merge([good.Reader, bad.Reader]);

        good.Writer.Complete();
        bad.Writer.Complete(new IOException("upstream failed"));

        var ex = await Assert.ThrowsExactlyAsync<IOException>(() => merged.Completion.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.AreEqual("upstream failed", ex.Message);
    }

    private static void InterlockedMax(ref int target, int value)
    {
        int current;
        while ((current = Volatile.Read(ref target)) < value
            && Interlocked.CompareExchange(ref target, value, current) != current)
        {
        }
    }
}
