---
title: Async/Await Essentials
level: 4
minutes: 40
summary: Concurrent fan-out with WhenAll, first-success with WhenEach, timeouts with WaitAsync, testable retries, ValueTask caching, and collecting every failure.
topics: [Async/await, Tasks, Concurrency, Testability]
keywords: [async, await, Task, ValueTask]
apis: [Task.WhenAll, Task.WhenEach, Task.WaitAsync, TaskCompletionSource, ConfigureAwaitOptions.SuppressThrowing, CancellationTokenSource.CreateLinkedTokenSource]
docs:
  - title: Asynchronous programming with async and await
    url: https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/
  - title: Process asynchronous tasks as they complete
    url: https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/start-multiple-async-tasks-and-process-them-as-they-complete
  - title: Task-based asynchronous pattern (TAP)
    url: https://learn.microsoft.com/dotnet/standard/asynchronous-programming-patterns/task-based-asynchronous-pattern-tap
---

## Brief

`async`/`await` is easy to use and easy to misuse: sequential awaits where concurrency was intended, lost exceptions from `WhenAll`, untestable `Task.Delay` calls, and `ValueTask`s awaited twice. This kata covers the patterns a service developer uses weekly, plus three newer APIs: `Task.WaitAsync` (.NET 6), `ConfigureAwaitOptions` (.NET 8), and `Task.WhenEach` (.NET 9).

## Tasks

Implement the members of `AsyncKata` and `CachedLookup`:

1. **`FetchAllAsync(keys, fetch)`** starts every fetch **concurrently** and returns the results in key order.
2. **`WithTimeoutAsync(task, timeout)`** returns the task's result, or throws `TimeoutException` if it takes longer than `timeout`.
3. **`RetryAsync(operation, maxAttempts, baseDelay, delay)`** retries a failing operation with exponential backoff: waits `baseDelay`, then `2 × baseDelay`, then `4 × baseDelay`, and so on. Waiting goes through the injected `delay` function (tests pass a fake that records durations). The last failure propagates unchanged.
4. **`CollectErrorsAsync(tasks)`** waits for **all** tasks and returns the messages of every failure (empty when all succeed). It must not throw.
5. **`FirstSuccessfulAsync(attempts, cancellationToken)`** starts every attempt, returns the first **successful** result, and cancels the others through the token passed to them. If every attempt fails, throw an `AggregateException` containing all failures.
6. **`CachedLookup.GetAsync(id)`** returns a `ValueTask<string>`. Cache hits complete **synchronously** without allocating a `Task`; misses call the loader once and cache the result. `LoaderCalls` counts loader invocations.

## Hints

- Create all tasks first (`keys.Select(fetch).ToList()`), then `await Task.WhenAll(tasks)`. Awaiting inside a `foreach` serializes the work.
- `task.WaitAsync(timeout)` throws `TimeoutException` and leaves the original task running.
- `baseDelay * Math.Pow(2, attempt - 1)` uses the `TimeSpan * double` operator. Put the retry decision in an exception filter: `catch (Exception) when (attempt < maxAttempts)`.
- `await whenAll.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);` waits without throwing; afterwards `whenAll.Exception?.InnerExceptions` holds every failure.
- `await foreach (var task in Task.WhenEach(tasks))` yields tasks in completion order. Link a `CancellationTokenSource` to the caller's token and cancel it once you have a winner.
- `ValueTask.FromResult(value)` (or `new ValueTask<string>(value)`) for hits; `new ValueTask<string>(LoadAsync(id))` for misses.

## Notes

- **`await` rethrows only the first exception** of a `WhenAll`. The returned task's `Exception` property is an `AggregateException` with all of them.
- **Never block on async code** with `.Result` or `.Wait()` in code that may run on a synchronization context (legacy ASP.NET, WinForms, WPF). It deadlocks. ASP.NET Core has no synchronization context, which hides the problem until the code is reused elsewhere.
- **`ConfigureAwait(false)`** matters in libraries that might run under a synchronization context; application code in ASP.NET Core does not need it.
- **ValueTask rules.** Await a `ValueTask` exactly once, never concurrently, and never call `.Result` before it completes. Call `.AsTask()` if you need any of those.
- **Inject time.** The `delay` parameter here is a lightweight stand-in for `TimeProvider` (.NET 8), which Level 5's rate-limiter kata uses.
