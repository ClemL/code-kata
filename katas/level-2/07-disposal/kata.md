---
title: Disposal & Resources
level: 2
minutes: 35
summary: IDisposable and IAsyncDisposable, using declarations, reverse-order cleanup with aggregated errors, and the full dispose pattern with a finalizer.
topics: [Resource management, IDisposable, Async disposal, Finalizers]
keywords: [using, "await using", finally, "~", IDisposable]
apis: [IAsyncDisposable, ValueTask, ObjectDisposedException.ThrowIf, GC.SuppressFinalize, Marshal.AllocHGlobal, AggregateException]
docs:
  - title: Implement a Dispose method
    url: https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-dispose
  - title: Implement a DisposeAsync method
    url: https://learn.microsoft.com/dotnet/standard/garbage-collection/implementing-disposeasync
  - title: using statement
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/using
---

## Brief

Deterministic cleanup is one of the places where C# differs most from garbage-collected peers. `using` declarations (C# 8) removed the nesting, `await using` handles asynchronous cleanup, and the classic dispose pattern with a finalizer still matters whenever you hold an unmanaged resource directly.

## Tasks

1. **`Scope.Begin(string name, List<string> log)`** logs `"enter {name}"` immediately and returns an `IDisposable` that logs `"exit {name}"` when disposed. Disposing twice logs only once.
2. **`CompositeDisposable`** collects disposables:
   - `Add(IDisposable item)` registers an item. Adding to an already-disposed composite disposes the item immediately.
   - `Dispose()` disposes items in **reverse order of addition**, exactly once.
   - If any items throw, every remaining item is still disposed, then an `AggregateException` containing all failures is thrown.
   - `Count` and `IsDisposed` report state.
3. **`BufferedLineWriter(List<string> destination)`** implements `IAsyncDisposable`:
   - `Write(string line)` buffers in memory and throws `ObjectDisposedException` after disposal;
   - `FlushAsync()` moves buffered lines to `destination` (yield once with `await Task.Yield()` to simulate I/O);
   - `DisposeAsync()` flushes and is idempotent.
4. **`NativeBuffer(int size)`** owns unmanaged memory from `Marshal.AllocHGlobal`:
   - `Read(offset)` / `Write(offset, value)` with bounds checks (`ArgumentOutOfRangeException`) and `ObjectDisposedException` after disposal;
   - implement the full dispose pattern: `Dispose()`, `protected virtual`-style `Dispose(bool disposing)` (private is fine for a sealed class), a **finalizer**, and `GC.SuppressFinalize(this)`;
   - the static `LiveCount` tracks how many buffers currently hold memory.

## Hints

- A private nested `sealed class` that implements `IDisposable` keeps `Scope.Begin` tidy.
- Reverse iteration: `for (var i = items.Count - 1; i >= 0; i--)`, with `try`/`catch` around each `Dispose()` call to collect failures.
- `ObjectDisposedException.ThrowIf(_disposed, this);` (.NET 7) is a one-line guard.
- `DisposeAsync` returns `ValueTask`; mark it `async` and `await FlushAsync()` before setting the disposed flag.
- `Interlocked.Increment(ref s_liveCount)` keeps the counter correct across threads.
- `Marshal.WriteByte(handle, offset, value)` and `Marshal.ReadByte(handle, offset)` avoid `unsafe` code.

## Notes

- **Finalizers are a last resort.** They run on a dedicated thread, at an unpredictable time, and delay collection. Prefer wrapping unmanaged handles in a `SafeHandle` subclass, which carries its own critical finalizer.
- **Never let a finalizer throw.** An unhandled exception on the finalizer thread terminates the process. That is why the starter for this kata contains no finalizer: its constructor throws, and a throwing finalizer on a half-built object would crash the test host.
- **`using` declarations** dispose at the end of the enclosing scope, in reverse declaration order, which is exactly what the `Scope` test relies on.
- **`IAsyncDisposable` + `IDisposable`.** Types that implement both should make `Dispose()` do the synchronous subset and `DisposeAsync()` the full cleanup. `Stream`, `DbConnection`, and `Utf8JsonWriter` follow this pattern.
