using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L4.Synchronization;

[TestClass]
public class SynchronizationTests
{
    [TestMethod]
    public void HitCounter_IsSafeUnderContention()
    {
        var counter = new HitCounter();
        Parallel.For(0, 200_000, _ => counter.Increment());
        Assert.AreEqual(200_000L, counter.Count);
    }

    [TestMethod]
    public void Account_DepositAndWithdraw()
    {
        var account = new Account(1, 100m);
        account.Deposit(50m);
        account.Withdraw(30m);

        Assert.AreEqual(120m, account.Balance);
        Assert.ThrowsExactly<InvalidOperationException>(() => account.Withdraw(500m));
        Assert.AreEqual(120m, account.Balance, "A failed withdrawal must not change the balance.");
    }

    [TestMethod]
    public void Account_ParallelDepositsAreNotLost()
    {
        var account = new Account(1, 0m);
        Parallel.For(0, 10_000, _ => account.Deposit(1m));
        Assert.AreEqual(10_000m, account.Balance);
    }

    [TestMethod]
    public void Transfer_RejectsSameAccount()
    {
        var account = new Account(1, 10m);
        Assert.ThrowsExactly<ArgumentException>(() => Account.Transfer(account, account, 1m));
    }

    [TestMethod]
    public async Task Transfer_DoesNotDeadlockAndConservesMoney()
    {
        var a = new Account(1, 10_000m);
        var b = new Account(2, 10_000m);

        var work = Task.Run(() => Parallel.For(0, 20_000, i =>
        {
            if (i % 2 == 0)
            {
                Account.Transfer(a, b, 1m);
            }
            else
            {
                Account.Transfer(b, a, 1m);
            }
        }));

        await work.WaitAsync(TimeSpan.FromSeconds(10));

        Assert.AreEqual(20_000m, a.Balance + b.Balance);
        Assert.AreEqual(10_000m, a.Balance);
    }

    [TestMethod]
    public async Task AsyncLock_ProvidesMutualExclusionAcrossAwaits()
    {
        var gate = new AsyncLock();
        var inside = 0;
        var maxInside = 0;

        var tasks = Enumerable.Range(0, 50).Select(async _ =>
        {
            using (await gate.LockAsync())
            {
                maxInside = Math.Max(maxInside, Interlocked.Increment(ref inside));
                await Task.Delay(1);
                Interlocked.Decrement(ref inside);
            }
        });

        await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(1, maxInside);
    }

    [TestMethod]
    public async Task AsyncLock_HonorsCancellationAndDoubleDispose()
    {
        var gate = new AsyncLock();
        var held = await gate.LockAsync();

        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(30));
        await Assert.ThrowsAsync<OperationCanceledException>(() => gate.LockAsync(cts.Token));

        held.Dispose();
        held.Dispose();

        using (await gate.LockAsync().WaitAsync(TimeSpan.FromSeconds(5)))
        {
        }

        var second = gate.LockAsync();
        Assert.IsTrue(second.IsCompleted, "A double Dispose must not leave the lock in a bad state.");
        (await second).Dispose();
    }

    [TestMethod]
    public async Task SingleFlight_RunsFactoryOncePerKey()
    {
        var calls = 0;
        var cache = new SingleFlightCache<string, int>(async key =>
        {
            Interlocked.Increment(ref calls);
            await Task.Delay(20);
            return key.Length;
        });

        var results = await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => Task.Run(() => cache.GetAsync("hello"))));

        Assert.IsTrue(results.All(r => r == 5));
        Assert.AreEqual(1, calls);

        Assert.AreEqual(3, await cache.GetAsync("abc"));
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public async Task SingleFlight_EvictsFailures()
    {
        var calls = 0;
        var cache = new SingleFlightCache<int, string>(async key =>
        {
            await Task.Yield();
            return ++calls == 1 ? throw new HttpRequestException("transient") : $"value {key}";
        });

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => cache.GetAsync(1));
        Assert.AreEqual("value 1", await cache.GetAsync(1));
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public void WordCounter_CountsAcrossDocumentsInParallel()
    {
        var documents = Enumerable.Range(0, 1_000).Select(i => i % 2 == 0 ? "the cat  sat" : "The dog sat").ToList();

        var counts = WordCounter.Count(documents);

        Assert.AreEqual(1_000, counts["the"]);
        Assert.AreEqual(1_000, counts["SAT"]);
        Assert.AreEqual(500, counts["cat"]);
        Assert.AreEqual(500, counts["dog"]);
    }
}
