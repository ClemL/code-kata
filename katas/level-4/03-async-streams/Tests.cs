using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L4.AsyncStreams;

[TestClass]
public class AsyncStreamsTests
{
    private static async IAsyncEnumerable<int> RangeAsync(
        int start,
        int count,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        for (var i = start; i < start + count; i++)
        {
            await Task.Yield();
            cancellationToken.ThrowIfCancellationRequested();
            yield return i;
        }
    }

    private static readonly IReadOnlyList<int>[] Pages = [[1, 2], [3, 4], [5], []];

    [TestMethod]
    public async Task ReadLines_StreamsEveryLine()
    {
        var lines = await AsyncStreams.CollectAsync(AsyncStreams.ReadLinesAsync(new StringReader("alpha\nbeta\r\ngamma")));
        CollectionAssert.AreEqual(new[] { "alpha", "beta", "gamma" }, lines);

        Assert.IsEmpty(await AsyncStreams.CollectAsync(AsyncStreams.ReadLinesAsync(new StringReader(""))));
    }

    [TestMethod]
    public async Task Paginate_YieldsEveryItemAndStopsAtEmptyPage()
    {
        var requested = new List<int>();
        var items = await AsyncStreams.CollectAsync(AsyncStreams.PaginateAsync<int>((page, _) =>
        {
            requested.Add(page);
            return Task.FromResult(Pages[page - 1]);
        }));

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, items);
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, requested);
    }

    [TestMethod]
    public async Task Paginate_IsLazy()
    {
        var requested = 0;
        var seen = new List<int>();

        await foreach (var item in AsyncStreams.PaginateAsync<int>((page, _) =>
        {
            requested++;
            return Task.FromResult(Pages[page - 1]);
        }))
        {
            seen.Add(item);
            if (seen.Count == 3)
            {
                break;
            }
        }

        Assert.AreEqual(2, requested, "Only the pages needed for three items should be fetched.");
    }

    [TestMethod]
    public async Task Paginate_ReceivesTheConsumersToken()
    {
        using var cts = new CancellationTokenSource();
        var observed = new List<CancellationToken>();

        var stream = AsyncStreams.PaginateAsync<int>((page, ct) =>
        {
            observed.Add(ct);
            return Task.FromResult(Pages[page - 1]);
        });

        await foreach (var _ in stream.WithCancellation(cts.Token))
        {
        }

        Assert.IsTrue(observed.All(ct => ct == cts.Token), "Mark the token parameter with [EnumeratorCancellation].");
    }

    [TestMethod]
    public async Task Paginate_StopsWhenCancelled()
    {
        using var cts = new CancellationTokenSource();
        var requested = 0;

        var stream = AsyncStreams.PaginateAsync<int>((page, _) =>
        {
            requested++;
            return Task.FromResult(Pages[page - 1]);
        });

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in stream.WithCancellation(cts.Token))
            {
                if (item == 2)
                {
                    await cts.CancelAsync();
                }
            }
        });

        Assert.AreEqual(1, requested);
    }

    [TestMethod]
    public async Task Batch_GroupsItems()
    {
        var batches = await AsyncStreams.CollectAsync(AsyncStreams.BatchAsync(RangeAsync(1, 7), 3));

        Assert.HasCount(3, batches);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, batches[0].ToArray());
        CollectionAssert.AreEqual(new[] { 4, 5, 6 }, batches[1].ToArray());
        CollectionAssert.AreEqual(new[] { 7 }, batches[2].ToArray());
    }

    [TestMethod]
    public async Task Batch_HandlesExactMultiplesAndEmptyInput()
    {
        Assert.HasCount(2, await AsyncStreams.CollectAsync(AsyncStreams.BatchAsync(RangeAsync(1, 4), 2)));
        Assert.IsEmpty(await AsyncStreams.CollectAsync(AsyncStreams.BatchAsync(RangeAsync(1, 0), 2)));
    }

    [TestMethod]
    public async Task Select_ProjectsSequentiallyInOrder()
    {
        var inFlight = 0;
        var maxInFlight = 0;

        var results = await AsyncStreams.CollectAsync(AsyncStreams.SelectAsync(RangeAsync(1, 5), async (n, ct) =>
        {
            maxInFlight = Math.Max(maxInFlight, Interlocked.Increment(ref inFlight));
            await Task.Delay(5 - n, ct);
            Interlocked.Decrement(ref inFlight);
            return n * 10;
        }));

        CollectionAssert.AreEqual(new[] { 10, 20, 30, 40, 50 }, results);
        Assert.AreEqual(1, maxInFlight);
    }

    [TestMethod]
    public async Task Collect_PropagatesCancellation()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => AsyncStreams.CollectAsync(RangeAsync(1, 3), cts.Token));
    }
}
