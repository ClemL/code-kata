---
title: Cancellation & Timeouts
level: 4
minutes: 30
summary: Cooperative cancellation, linked token sources, telling timeouts apart from caller cancellation, PeriodicTimer polling, token callbacks, and graceful shutdown.
topics: [Cancellation, Timeouts, Async/await, Graceful shutdown]
keywords: [async, await, when, using]
apis: [CancellationToken, CancellationTokenSource.CreateLinkedTokenSource, CancelAfter, OperationCanceledException, PeriodicTimer, CancellationToken.Register]
docs:
  - title: Cancellation in managed threads
    url: https://learn.microsoft.com/dotnet/standard/threading/cancellation-in-managed-threads
  - title: Cancel a list of tasks
    url: https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/cancel-an-async-task-or-a-list-of-tasks
  - title: PeriodicTimer
    url: https://learn.microsoft.com/dotnet/api/system.threading.periodictimer
---

## Brief

Cancellation in .NET is cooperative: a `CancellationToken` is a request, and your code decides where to honor it. Getting it right means passing tokens all the way down, combining them with linked sources, distinguishing "the caller gave up" from "we timed out", and shutting down cleanly instead of crashing with an `OperationCanceledException`.

## Tasks

Implement the static methods on `CancellationKata`:

1. **`Sum(IEnumerable<int> values, CancellationToken cancellationToken)`** adds values and stops with `OperationCanceledException` as soon as cancellation is requested (check on every item).
2. **`RunWithTimeoutAsync(operation, timeout, cancellationToken)`** runs `operation` with a token that is cancelled by **either** the caller or the timeout:
   - caller cancellation → `OperationCanceledException` (or a subclass);
   - timeout → `TimeoutException`.
3. **`PollUntilAsync(condition, interval, cancellationToken)`** checks `condition` immediately and then once per `interval` (use `PeriodicTimer`) until it returns `true`. Return how many times it was checked. Cancellation throws `OperationCanceledException`.
4. **`WhenCancelled(CancellationToken token)`** returns a `Task` that completes successfully when `token` is cancelled (immediately if it already is).
5. **`ProcessAllAsync(items, handler, cancellationToken)`** processes items sequentially and supports **graceful shutdown**: when cancellation is requested (between items or inside the handler), stop and return `(Completed, WasCancelled: true)` instead of throwing. Exceptions that are *not* caused by this token still propagate.

## Hints

- `cancellationToken.ThrowIfCancellationRequested()` inside the loop.
- `using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken); cts.CancelAfter(timeout);`
- `catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)` means the linked source fired because of the timeout.
- `using var timer = new PeriodicTimer(interval); while (await timer.WaitForNextTickAsync(cancellationToken)) { ... }`
- `var tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); token.Register(() => tcs.TrySetResult());`
- In `ProcessAllAsync`, catch `OperationCanceledException` only `when (cancellationToken.IsCancellationRequested)`.

## Notes

- **`TaskCanceledException` derives from `OperationCanceledException`.** Catch the base type.
- **Linked sources must be disposed.** A linked `CancellationTokenSource` registers a callback on its parents; forgetting `using` leaks those registrations for as long as the parent token lives (often the application lifetime).
- **`RunContinuationsAsynchronously`.** Without it, `TrySetResult` runs awaiting continuations inline on the thread that called `Cancel()`, which can cause surprising re-entrancy and deadlocks.
- **`BackgroundService`** in ASP.NET Core passes a `stoppingToken` to `ExecuteAsync`; the graceful pattern in task 5 is exactly how a well-behaved worker should exit.
- **HttpClient timeouts** surface as `TaskCanceledException` whose `InnerException` is a `TimeoutException` (.NET 5+), the same distinction this kata makes explicit.
