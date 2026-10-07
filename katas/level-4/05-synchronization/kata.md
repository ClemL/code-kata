---
title: Thread Safety & Synchronization
level: 4
minutes: 40
summary: Interlocked counters, the C# 13 Lock type with deadlock-free lock ordering, an async lock built on SemaphoreSlim, single-flight caching with Lazy<Task<T>>, and ConcurrentDictionary.
topics: [Concurrency, Locks, Thread safety, Caching]
keywords: [lock, volatile, async, await, using]
apis: [Interlocked, System.Threading.Lock, SemaphoreSlim, "ConcurrentDictionary<TKey,TValue>", "Lazy<T>", Parallel.ForEach]
docs:
  - title: lock statement (and System.Threading.Lock)
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/lock
  - title: Overview of synchronization primitives
    url: https://learn.microsoft.com/dotnet/standard/threading/overview-of-synchronization-primitives
  - title: Thread-safe collections
    url: https://learn.microsoft.com/dotnet/standard/collections/thread-safe/
---

## Brief

Shared mutable state is where concurrency bugs live. .NET offers a ladder of tools: `Interlocked` for single values, `lock` (backed by the dedicated `System.Threading.Lock` type since C# 13 / .NET 9) for short critical sections, `SemaphoreSlim` when you need to `await` inside the critical section, and concurrent collections when you would otherwise lock a dictionary. Use the lowest rung that solves the problem.

## Tasks

1. **`HitCounter`**: `Increment()` and `Count` are safe under heavy parallel use, without `lock`.
2. **`Account`**: `Deposit`, `Withdraw` (throws `InvalidOperationException` on insufficient funds), and `Balance` are thread-safe, using a `System.Threading.Lock` field. The static **`Account.Transfer(from, to, amount)`** moves money atomically and **cannot deadlock** when transfers run in both directions at once. Transferring to the same account throws `ArgumentException`.
3. **`AsyncLock`**: `LockAsync(cancellationToken)` returns an `IDisposable` that releases the lock when disposed, so callers can write `using (await gate.LockAsync()) { await ...; }`. Disposing twice releases once. Waiting honors cancellation.
4. **`SingleFlightCache<TKey, TValue>`**: `GetAsync(key)` runs the factory **at most once per key** even when many callers ask concurrently; they all await the same task. If the factory fails, the entry is removed so a later call can retry.
5. **`WordCounter.Count(documents)`**: counts words (split on spaces, case-insensitive) across documents with `Parallel.ForEach` and a `ConcurrentDictionary`.

## Hints

- `Interlocked.Increment(ref _count)` and `Interlocked.Read(ref _count)` for a `long`.
- `private readonly Lock _gate = new();` then `lock (_gate) { ... }`. The compiler emits `Lock.EnterScope()` for this type instead of `Monitor.Enter`.
- Deadlock-free transfers: always acquire the two locks in a global order (for example, lower `Id` first).
- `SemaphoreSlim(1, 1)` is a mutex you can `await`. Wrap `Release()` in a small private `IDisposable` with an `Interlocked.Exchange` guard.
- `ConcurrentDictionary.GetOrAdd(key, k => new Lazy<Task<TValue>>(() => factory(k)))`: `GetOrAdd` may call the value factory more than once under a race, but only one `Lazy` wins, and `Lazy` guarantees one execution.
- `TryRemove(KeyValuePair.Create(key, lazy))` removes the entry only if it still holds that exact `Lazy`.
- `counts.AddOrUpdate(word, 1, (_, current) => current + 1)`.

## Notes

- **Never `await` inside `lock`.** The compiler forbids it (CS1996), because a `Monitor` is thread-affine and the continuation may resume on another thread. `SemaphoreSlim` has no thread affinity.
- **Lock on a private object.** `lock (this)` or `lock (typeof(X))` lets outside code take your lock and deadlock you.
- **`ConcurrentDictionary` callbacks are not atomic.** The `valueFactory` of `GetOrAdd` and the `updateValueFactory` of `AddOrUpdate` run outside the internal lock and can run more than once. Keep them pure, or use the `Lazy<T>` trick.
- **Single-flight in production.** `HybridCache` (.NET 9, `Microsoft.Extensions.Caching.Hybrid`) provides stampede protection out of the box.
