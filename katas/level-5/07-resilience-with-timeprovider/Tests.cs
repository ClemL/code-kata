using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.Resilience;

/// <summary>A clock that only moves when the test says so.</summary>
public sealed class ManualTimeProvider(DateTimeOffset start) : TimeProvider
{
    private DateTimeOffset _now = start;

    public override DateTimeOffset GetUtcNow() => _now;

    public override long GetTimestamp() => _now.UtcTicks;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public void Advance(TimeSpan by) => _now += by;
}

[TestClass]
public class ResilienceTests
{
    private static ManualTimeProvider NewClock() => new(new DateTimeOffset(2026, 10, 7, 9, 0, 0, TimeSpan.Zero));

    [TestMethod]
    public void TokenBucket_StartsFullAndDrains()
    {
        var bucket = new TokenBucket(capacity: 5, tokensPerSecond: 1, NewClock());

        for (var i = 0; i < 5; i++)
        {
            Assert.IsTrue(bucket.TryAcquire(), $"Request {i + 1} should pass.");
        }

        Assert.IsFalse(bucket.TryAcquire());
        Assert.AreEqual(0, bucket.AvailableTokens, 1e-9);
    }

    [TestMethod]
    public void TokenBucket_RefillsOverTimeUpToCapacity()
    {
        var clock = NewClock();
        var bucket = new TokenBucket(capacity: 5, tokensPerSecond: 1, clock);
        while (bucket.TryAcquire())
        {
        }

        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.IsTrue(bucket.TryAcquire());
        Assert.IsFalse(bucket.TryAcquire());

        clock.Advance(TimeSpan.FromMinutes(10));
        Assert.AreEqual(5, bucket.AvailableTokens, 1e-9, "Tokens never exceed capacity.");
    }

    [TestMethod]
    public void TokenBucket_AccruesFractionalTokens()
    {
        var clock = NewClock();
        var bucket = new TokenBucket(capacity: 1, tokensPerSecond: 2, clock);
        Assert.IsTrue(bucket.TryAcquire());

        clock.Advance(TimeSpan.FromMilliseconds(250));
        Assert.AreEqual(0.5, bucket.AvailableTokens, 1e-9);
        Assert.IsFalse(bucket.TryAcquire());

        clock.Advance(TimeSpan.FromMilliseconds(250));
        Assert.IsTrue(bucket.TryAcquire());
    }

    [TestMethod]
    public void TokenBucket_ReportsRetryAfter()
    {
        var clock = NewClock();
        var bucket = new TokenBucket(capacity: 4, tokensPerSecond: 2, clock);
        Assert.IsTrue(bucket.TryAcquire(3));

        Assert.AreEqual(TimeSpan.Zero, bucket.RetryAfter(1));
        Assert.AreEqual(TimeSpan.FromSeconds(1), bucket.RetryAfter(3));

        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.IsTrue(bucket.TryAcquire(3));
    }

    [TestMethod]
    public void TokenBucket_RejectsImpossibleRequests()
    {
        var bucket = new TokenBucket(capacity: 2, tokensPerSecond: 1, NewClock());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bucket.TryAcquire(3));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => bucket.TryAcquire(0));
    }

    private static Task<int> Fail() => Task.FromException<int>(new TimeoutException("downstream timeout"));

    private static Task<int> Succeed() => Task.FromResult(42);

    [TestMethod]
    public async Task Breaker_OpensAfterConsecutiveFailures()
    {
        var breaker = new CircuitBreaker(failureThreshold: 3, breakDuration: TimeSpan.FromSeconds(30), NewClock());
        var calls = 0;
        Task<int> CountedFail() { calls++; return Fail(); }

        for (var i = 0; i < 3; i++)
        {
            await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(CountedFail));
        }

        Assert.AreEqual(CircuitState.Open, breaker.State);
        await Assert.ThrowsExactlyAsync<CircuitOpenException>(() => breaker.ExecuteAsync(CountedFail));
        Assert.AreEqual(3, calls, "An open circuit must not call the action.");
    }

    [TestMethod]
    public async Task Breaker_SuccessResetsTheFailureCount()
    {
        var breaker = new CircuitBreaker(failureThreshold: 3, breakDuration: TimeSpan.FromSeconds(30), NewClock());

        await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(Fail));
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(Fail));
        Assert.AreEqual(42, await breaker.ExecuteAsync(Succeed));
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(Fail));
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(Fail));

        Assert.AreEqual(CircuitState.Closed, breaker.State);
    }

    [TestMethod]
    public async Task Breaker_HalfOpenTrialSuccessCloses()
    {
        var clock = NewClock();
        var breaker = new CircuitBreaker(failureThreshold: 1, breakDuration: TimeSpan.FromSeconds(30), clock);
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(Fail));

        clock.Advance(TimeSpan.FromSeconds(29));
        Assert.AreEqual(CircuitState.Open, breaker.State);

        clock.Advance(TimeSpan.FromSeconds(1));
        Assert.AreEqual(CircuitState.HalfOpen, breaker.State);

        Assert.AreEqual(42, await breaker.ExecuteAsync(Succeed));
        Assert.AreEqual(CircuitState.Closed, breaker.State);
    }

    [TestMethod]
    public async Task Breaker_HalfOpenTrialFailureReopens()
    {
        var clock = NewClock();
        var breaker = new CircuitBreaker(failureThreshold: 2, breakDuration: TimeSpan.FromSeconds(30), clock);
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(Fail));
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(Fail));

        clock.Advance(TimeSpan.FromSeconds(30));
        await Assert.ThrowsExactlyAsync<TimeoutException>(() => breaker.ExecuteAsync(Fail));

        Assert.AreEqual(CircuitState.Open, breaker.State, "One failed trial is enough to reopen.");
        clock.Advance(TimeSpan.FromSeconds(29));
        await Assert.ThrowsExactlyAsync<CircuitOpenException>(() => breaker.ExecuteAsync(Succeed));
    }
}
