using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L4.AsyncBasics;

[TestClass]
public class AsyncAwaitTests
{
    private static readonly TimeSpan Guard = TimeSpan.FromSeconds(5);

    [TestMethod]
    public async Task FetchAll_RunsConcurrently()
    {
        var started = 0;
        var allStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        async Task<string> Fetch(int id)
        {
            if (Interlocked.Increment(ref started) == 3)
            {
                allStarted.SetResult();
            }

            // Sequential code never reaches 3 concurrent fetches, so this would time out.
            await allStarted.Task.WaitAsync(Guard);
            return $"item {id}";
        }

        var results = await AsyncKata.FetchAllAsync([1, 2, 3], Fetch);

        CollectionAssert.AreEqual(new[] { "item 1", "item 2", "item 3" }, results);
    }

    [TestMethod]
    public async Task FetchAll_PreservesKeyOrder()
    {
        var results = await AsyncKata.FetchAllAsync([30, 0, 10], async delay =>
        {
            await Task.Delay(delay);
            return delay;
        });

        CollectionAssert.AreEqual(new[] { 30, 0, 10 }, results);
    }

    [TestMethod]
    public async Task WithTimeout_ReturnsResultWhenFastEnough() =>
        Assert.AreEqual(42, await AsyncKata.WithTimeoutAsync(Task.FromResult(42), TimeSpan.FromSeconds(1)));

    [TestMethod]
    public async Task WithTimeout_ThrowsTimeoutException()
    {
        var never = new TaskCompletionSource<int>().Task;
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => AsyncKata.WithTimeoutAsync(never, TimeSpan.FromMilliseconds(20)));
    }

    [TestMethod]
    public async Task Retry_BacksOffExponentially()
    {
        var delays = new List<TimeSpan>();
        var calls = 0;

        var result = await AsyncKata.RetryAsync(
            () => ++calls < 4 ? Task.FromException<string>(new IOException($"fail {calls}")) : Task.FromResult("done"),
            maxAttempts: 5,
            baseDelay: TimeSpan.FromMilliseconds(100),
            delay: d => { delays.Add(d); return Task.CompletedTask; });

        Assert.AreEqual("done", result);
        Assert.AreEqual(4, calls);
        CollectionAssert.AreEqual(
            new[] { TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400) },
            delays);
    }

    [TestMethod]
    public async Task Retry_PropagatesLastFailure()
    {
        var delays = 0;
        var calls = 0;

        var ex = await Assert.ThrowsExactlyAsync<IOException>(() => AsyncKata.RetryAsync<int>(
            () => Task.FromException<int>(new IOException($"fail {++calls}")),
            maxAttempts: 3,
            baseDelay: TimeSpan.FromSeconds(1),
            delay: _ => { delays++; return Task.CompletedTask; }));

        Assert.AreEqual("fail 3", ex.Message);
        Assert.AreEqual(2, delays, "No delay after the final attempt.");
    }

    [TestMethod]
    public async Task CollectErrors_ReturnsEveryFailure()
    {
        var errors = await AsyncKata.CollectErrorsAsync(
        [
            Task.CompletedTask,
            Task.FromException(new InvalidOperationException("first")),
            Task.Run(async () => { await Task.Delay(10); throw new ArgumentException("second"); }),
        ]);

        CollectionAssert.AreEquivalent(new[] { "first", "second" }, errors.ToArray());
    }

    [TestMethod]
    public async Task CollectErrors_IsEmptyWhenAllSucceed() =>
        Assert.IsEmpty(await AsyncKata.CollectErrorsAsync([Task.CompletedTask, Task.Delay(1)]));

    [TestMethod]
    public async Task FirstSuccessful_ReturnsFastestSuccessAndCancelsTheRest()
    {
        CancellationToken slowToken = default;

        var result = await AsyncKata.FirstSuccessfulAsync<string>(
        [
            async ct => { slowToken = ct; await Task.Delay(Timeout.Infinite, ct); return "slow"; },
            _ => Task.FromException<string>(new HttpRequestException("replica down")),
            async ct => { await Task.Delay(20, ct); return "fast"; },
        ]);

        Assert.AreEqual("fast", result);
        Assert.IsTrue(slowToken.IsCancellationRequested, "Losing attempts should be cancelled.");
    }

    [TestMethod]
    public async Task FirstSuccessful_AggregatesWhenEverythingFails()
    {
        var ex = await Assert.ThrowsExactlyAsync<AggregateException>(() => AsyncKata.FirstSuccessfulAsync<int>(
        [
            _ => Task.FromException<int>(new TimeoutException("a")),
            async _ => { await Task.Yield(); throw new IOException("b"); },
        ]));

        Assert.HasCount(2, ex.InnerExceptions);
    }

    [TestMethod]
    public async Task CachedLookup_CompletesHitsSynchronously()
    {
        var lookup = new CachedLookup(async id => { await Task.Yield(); return $"value {id}"; });

        Assert.AreEqual("value 7", await lookup.GetAsync(7));

        var hit = lookup.GetAsync(7);
        Assert.IsTrue(hit.IsCompletedSuccessfully, "A cache hit should not need to await anything.");
        Assert.AreEqual("value 7", await hit);
        Assert.AreEqual(1, lookup.LoaderCalls);

        Assert.AreEqual("value 8", await lookup.GetAsync(8));
        Assert.AreEqual(2, lookup.LoaderCalls);
    }
}
