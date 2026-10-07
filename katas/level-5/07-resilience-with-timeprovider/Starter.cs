namespace Katas.L5.Resilience;

public sealed class TokenBucket(int capacity, double tokensPerSecond, TimeProvider timeProvider)
{
    public double AvailableTokens => throw new NotImplementedException();

    public bool TryAcquire(int tokens = 1) => throw new NotImplementedException();

    public TimeSpan RetryAfter(int tokens = 1) => throw new NotImplementedException();
}

public enum CircuitState
{
    Closed,
    Open,
    HalfOpen,
}

public sealed class CircuitOpenException() : Exception("The circuit is open; the call was not attempted.");

public sealed class CircuitBreaker(int failureThreshold, TimeSpan breakDuration, TimeProvider timeProvider)
{
    public CircuitState State => throw new NotImplementedException();

    public Task<T> ExecuteAsync<T>(Func<Task<T>> action) => throw new NotImplementedException();
}
