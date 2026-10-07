---
title: Rate Limiting & Circuit Breaking
level: 5
minutes: 45
summary: Implement a token-bucket rate limiter and a circuit breaker against the .NET 8 TimeProvider abstraction, tested deterministically with a manual clock.
topics: [Resilience, Rate limiting, Testability, Time]
keywords: [async, await, enum, lock]
apis: [TimeProvider, TimeProvider.GetUtcNow, TimeProvider.GetElapsedTime, "Task<T>"]
docs:
  - title: What is TimeProvider?
    url: https://learn.microsoft.com/dotnet/standard/datetime/timeprovider-overview
  - title: Rate limiting middleware in ASP.NET Core
    url: https://learn.microsoft.com/aspnet/core/performance/rate-limit
  - title: Build resilient HTTP apps (Microsoft.Extensions.Http.Resilience)
    url: https://learn.microsoft.com/dotnet/core/resilience/http-resilience
  - title: Circuit breaker pattern
    url: https://learn.microsoft.com/azure/architecture/patterns/circuit-breaker
---

## Brief

Time-dependent logic is notoriously hard to test when it reads `DateTime.UtcNow` directly. .NET 8 introduced `TimeProvider`, an abstract clock that production code receives through its constructor and tests replace with a manual one. This kata uses it to build two resilience primitives you would normally take from `System.Threading.RateLimiting` or Polly, so the tests can move time forward instantly and exactly.

## Tasks

1. **`TokenBucket(capacity, tokensPerSecond, timeProvider)`** starts full and refills continuously (fractional tokens accrue) up to `capacity`.
   - `TryAcquire(int tokens = 1)` takes tokens if available and returns `true`, otherwise returns `false` without taking any. Requests above `capacity` throw `ArgumentOutOfRangeException`.
   - `AvailableTokens` reports the current (refilled) amount.
   - `RetryAfter(int tokens = 1)` returns how long until `tokens` would be available (`TimeSpan.Zero` if they are available now).
2. **`CircuitBreaker(failureThreshold, breakDuration, timeProvider)`** with `ExecuteAsync<T>(Func<Task<T>> action)`:
   - **Closed**: run the action. Exceptions propagate unchanged. After `failureThreshold` **consecutive** failures, the circuit opens. A success resets the count.
   - **Open**: throw `CircuitOpenException` **without running the action** until `breakDuration` has passed.
   - **HalfOpen** (after `breakDuration`): allow one trial call. Success closes the circuit; failure opens it again for another full `breakDuration`.
   - `State` reports the current state, including the transition from Open to HalfOpen as time passes.

## Hints

- Store `_tokens` (a `double`) and `_lastRefill` (a `DateTimeOffset`). On every call: `elapsed = now - _lastRefill`; `_tokens = Math.Min(capacity, _tokens + elapsed.TotalSeconds * tokensPerSecond)`; `_lastRefill = now`.
- `RetryAfter`: `TimeSpan.FromSeconds((tokens - _tokens) / tokensPerSecond)` when short.
- Read time only through `timeProvider.GetUtcNow()` (or `GetTimestamp()` + `GetElapsedTime()`), never `DateTime.UtcNow`.
- Compute `State` from the stored state plus the clock: an `Open` circuit whose `breakDuration` has elapsed reports `HalfOpen`.
- Wrap state changes in a `lock` if you want thread safety; the tests are single-threaded.

## Notes

- **`FakeTimeProvider`** in the `Microsoft.Extensions.TimeProvider.Testing` package is the production-quality version of the manual clock in these tests, including timers that fire when you call `Advance`.
- **`Task.Delay(TimeSpan, TimeProvider)`** and `CancellationTokenSource(TimeSpan, TimeProvider)` (.NET 8) make delays and timeouts controllable by the same fake clock.
- **Built-in limiters.** `System.Threading.RateLimiting` provides `TokenBucketRateLimiter`, `FixedWindowRateLimiter`, `SlidingWindowRateLimiter`, and `ConcurrencyLimiter`, and ASP.NET Core wires them up with `AddRateLimiter`. For outgoing HTTP calls, `Microsoft.Extensions.Http.Resilience` (built on Polly v8) adds retries, timeouts, and circuit breakers in one line.
- **Half-open under concurrency.** The reference solution lets every caller that arrives during the half-open window through. Production breakers admit exactly one trial and reject the rest until it completes; adding that is a good stretch goal.
- **Count consecutive failures carefully.** Real breakers usually use a failure *ratio* over a sampling window with a minimum throughput, so one bad request at 3 a.m. does not open the circuit.
