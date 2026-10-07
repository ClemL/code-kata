namespace Katas.L5.Resilience;

public sealed class TokenBucket
{
    private readonly Lock _gate = new();
    private readonly int _capacity;
    private readonly double _tokensPerSecond;
    private readonly TimeProvider _timeProvider;
    private double _tokens;
    private DateTimeOffset _lastRefill;

    public TokenBucket(int capacity, double tokensPerSecond, TimeProvider timeProvider)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokensPerSecond);

        _capacity = capacity;
        _tokensPerSecond = tokensPerSecond;
        _timeProvider = timeProvider;
        _tokens = capacity;
        _lastRefill = timeProvider.GetUtcNow();
    }

    public double AvailableTokens
    {
        get
        {
            lock (_gate)
            {
                Refill();
                return _tokens;
            }
        }
    }

    public bool TryAcquire(int tokens = 1)
    {
        ValidateRequest(tokens);

        lock (_gate)
        {
            Refill();
            if (_tokens < tokens)
            {
                return false;
            }

            _tokens -= tokens;
            return true;
        }
    }

    public TimeSpan RetryAfter(int tokens = 1)
    {
        ValidateRequest(tokens);

        lock (_gate)
        {
            Refill();
            var missing = tokens - _tokens;
            return missing <= 0 ? TimeSpan.Zero : TimeSpan.FromSeconds(missing / _tokensPerSecond);
        }
    }

    private void ValidateRequest(int tokens)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokens);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(tokens, _capacity);
    }

    private void Refill()
    {
        var now = _timeProvider.GetUtcNow();
        var elapsed = now - _lastRefill;
        if (elapsed > TimeSpan.Zero)
        {
            _tokens = Math.Min(_capacity, _tokens + elapsed.TotalSeconds * _tokensPerSecond);
            _lastRefill = now;
        }
    }
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
    private readonly Lock _gate = new();
    private CircuitState _state = CircuitState.Closed;
    private int _consecutiveFailures;
    private DateTimeOffset _openedAt;

    public CircuitState State
    {
        get
        {
            lock (_gate)
            {
                return CurrentState();
            }
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        lock (_gate)
        {
            var state = CurrentState();
            if (state == CircuitState.Open)
            {
                throw new CircuitOpenException();
            }

            _state = state;
        }

        try
        {
            var result = await action();
            lock (_gate)
            {
                _state = CircuitState.Closed;
                _consecutiveFailures = 0;
            }

            return result;
        }
        catch (Exception)
        {
            lock (_gate)
            {
                _consecutiveFailures++;
                if (_state == CircuitState.HalfOpen || _consecutiveFailures >= failureThreshold)
                {
                    _state = CircuitState.Open;
                    _openedAt = timeProvider.GetUtcNow();
                }
            }

            throw;
        }
    }

    private CircuitState CurrentState() =>
        _state == CircuitState.Open && timeProvider.GetUtcNow() - _openedAt >= breakDuration
            ? CircuitState.HalfOpen
            : _state;
}
