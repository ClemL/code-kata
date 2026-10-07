using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L4.Cancellation;

[TestClass]
public class CancellationTests
{
    [TestMethod]
    public void Sum_CompletesWhenNotCancelled() =>
        Assert.AreEqual(15, CancellationKata.Sum([1, 2, 3, 4, 5], CancellationToken.None));

    [TestMethod]
    public void Sum_StopsAsSoonAsCancelled()
    {
        using var cts = new CancellationTokenSource();
        var pulled = 0;

        IEnumerable<int> Source()
        {
            while (true)
            {
                if (++pulled == 3)
                {
                    cts.Cancel();
                }

                yield return 1;
            }
        }

        Assert.ThrowsExactly<OperationCanceledException>(() => CancellationKata.Sum(Source(), cts.Token));
        Assert.AreEqual(3, pulled);
    }

    [TestMethod]
    public async Task RunWithTimeout_ReturnsResult() =>
        Assert.AreEqual("ok", await CancellationKata.RunWithTimeoutAsync(
            async ct => { await Task.Delay(1, ct); return "ok"; },
            TimeSpan.FromSeconds(5),
            CancellationToken.None));

    [TestMethod]
    public async Task RunWithTimeout_ReportsTimeout() =>
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => CancellationKata.RunWithTimeoutAsync(
            async ct => { await Task.Delay(Timeout.Infinite, ct); return 0; },
            TimeSpan.FromMilliseconds(30),
            CancellationToken.None));

    [TestMethod]
    public async Task RunWithTimeout_ReportsCallerCancellation()
    {
        using var caller = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));

        await Assert.ThrowsAsync<OperationCanceledException>(() => CancellationKata.RunWithTimeoutAsync(
            async ct => { await Task.Delay(Timeout.Infinite, ct); return 0; },
            TimeSpan.FromSeconds(30),
            caller.Token));
    }

    [TestMethod]
    public async Task RunWithTimeout_PassesTheLinkedTokenToTheOperation()
    {
        var observed = CancellationToken.None;
        await CancellationKata.RunWithTimeoutAsync(
            ct => { observed = ct; return Task.FromResult(1); },
            TimeSpan.FromSeconds(5),
            CancellationToken.None);

        Assert.IsTrue(observed.CanBeCanceled, "The operation must receive a token that the timeout can cancel.");
    }

    [TestMethod]
    public async Task PollUntil_CountsChecks()
    {
        var calls = 0;
        var checks = await CancellationKata.PollUntilAsync(
            _ => Task.FromResult(++calls == 3),
            TimeSpan.FromMilliseconds(5),
            CancellationToken.None);

        Assert.AreEqual(3, checks);
    }

    [TestMethod]
    public async Task PollUntil_ChecksImmediately()
    {
        var checks = await CancellationKata.PollUntilAsync(
            _ => Task.FromResult(true),
            TimeSpan.FromHours(1),
            CancellationToken.None);

        Assert.AreEqual(1, checks);
    }

    [TestMethod]
    public async Task PollUntil_HonorsCancellation()
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));
        await Assert.ThrowsAsync<OperationCanceledException>(() => CancellationKata.PollUntilAsync(
            _ => Task.FromResult(false),
            TimeSpan.FromMilliseconds(5),
            cts.Token));
    }

    [TestMethod]
    public async Task WhenCancelled_CompletesOnCancel()
    {
        using var cts = new CancellationTokenSource();
        var task = CancellationKata.WhenCancelled(cts.Token);

        Assert.IsFalse(task.IsCompleted);
        await cts.CancelAsync();
        await task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.IsTrue(task.IsCompletedSuccessfully);
    }

    [TestMethod]
    public void WhenCancelled_IsImmediateForCancelledTokens() =>
        Assert.IsTrue(CancellationKata.WhenCancelled(new CancellationToken(canceled: true)).IsCompletedSuccessfully);

    [TestMethod]
    public async Task ProcessAll_CompletesNormally()
    {
        var result = await CancellationKata.ProcessAllAsync(["a", "b"], (_, _) => Task.CompletedTask, CancellationToken.None);
        Assert.AreEqual((2, false), result);
    }

    [TestMethod]
    public async Task ProcessAll_StopsBetweenItems()
    {
        using var cts = new CancellationTokenSource();
        var seen = new List<string>();

        var result = await CancellationKata.ProcessAllAsync(["a", "b", "c", "d"], (item, _) =>
        {
            seen.Add(item);
            if (item == "b")
            {
                cts.Cancel();
            }

            return Task.CompletedTask;
        }, cts.Token);

        Assert.AreEqual((2, true), result);
        CollectionAssert.AreEqual(new[] { "a", "b" }, seen);
    }

    [TestMethod]
    public async Task ProcessAll_StopsInsideAHandler()
    {
        using var cts = new CancellationTokenSource();

        var result = await CancellationKata.ProcessAllAsync(["a", "b", "c"], async (item, ct) =>
        {
            if (item == "c")
            {
                cts.CancelAfter(20);
                await Task.Delay(Timeout.Infinite, ct);
            }
        }, cts.Token);

        Assert.AreEqual((2, true), result);
    }

    [TestMethod]
    public async Task ProcessAll_PropagatesUnrelatedCancellation()
    {
        using var unrelated = new CancellationTokenSource();
        await unrelated.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => CancellationKata.ProcessAllAsync(
            ["a"],
            (_, _) => Task.FromCanceled(unrelated.Token),
            CancellationToken.None));
    }
}
