---
title: Async Streams
level: 4
minutes: 30
summary: IAsyncEnumerable<T> with await foreach, async iterators, [EnumeratorCancellation], lazy pagination over an API, and streaming batch/projection operators.
topics: [Async streams, Iterators, Pagination, Cancellation]
keywords: [async, await, "await foreach", yield]
apis: ["IAsyncEnumerable<T>", EnumeratorCancellationAttribute, WithCancellation, TextReader.ReadLineAsync]
docs:
  - title: Generate and consume async streams
    url: https://learn.microsoft.com/dotnet/csharp/asynchronous-programming/generate-consume-asynchronous-stream
  - title: IAsyncEnumerable<T> interface
    url: https://learn.microsoft.com/dotnet/api/system.collections.generic.iasyncenumerable-1
  - title: EnumeratorCancellationAttribute
    url: https://learn.microsoft.com/dotnet/api/system.runtime.compilerservices.enumeratorcancellationattribute
---

## Brief

`IAsyncEnumerable<T>` (C# 8) combines iterators with `await`: a method can `await` an HTTP call and `yield return` each item as it arrives, and the consumer pulls with `await foreach`. EF Core's `AsAsyncEnumerable`, ASP.NET Core streaming responses, gRPC server streams, and Azure SDK pageable results all expose this shape.

## Tasks

Implement the static methods on `AsyncStreams` (plain static methods, not extension methods, so they never clash with .NET 10's built-in `System.Linq.AsyncEnumerable`):

1. **`ReadLinesAsync(TextReader reader, CancellationToken cancellationToken = default)`** yields each line until the end of the reader.
2. **`PaginateAsync(fetchPage, cancellationToken = default)`** calls `fetchPage(1, token)`, `fetchPage(2, token)`, ... and yields every item, stopping at the first empty page. It must be lazy (page 2 is not requested until page 1 has been consumed) and must check for cancellation before each request.
3. **`BatchAsync(source, size, cancellationToken = default)`** groups a stream into lists of `size` items; the last batch may be smaller.
4. **`SelectAsync(source, selector, cancellationToken = default)`** projects each item with an async selector, one at a time, preserving order.
5. **`CollectAsync(source, cancellationToken = default)`** materializes a stream into a `List<T>`.

The token passed to `WithCancellation(...)` by a consumer must reach every async iterator, so annotate the token parameters with `[EnumeratorCancellation]`.

## Hints

- An async iterator is an `async` method returning `IAsyncEnumerable<T>` that contains `yield return`.
- `while (await reader.ReadLineAsync(cancellationToken) is { } line) yield return line;`
- For pagination: `for (var page = 1; ; page++) { ct.ThrowIfCancellationRequested(); var items = await fetchPage(page, ct); if (items.Count == 0) yield break; foreach (...) yield return item; }`
- Pass the token through when consuming another stream: `await foreach (var item in source.WithCancellation(cancellationToken))`.
- Add `.ConfigureAwait(false)` after `WithCancellation` in library code if you want to avoid capturing a synchronization context.

## Notes

- **`[EnumeratorCancellation]` wiring.** `stream.WithCancellation(token)` passes `token` to `GetAsyncEnumerator`. The compiler-generated enumerator routes it into the parameter marked `[EnumeratorCancellation]`; if the method was *also* called with a token, it links the two.
- **Back-pressure is built in.** The producer does not run ahead of the consumer: each `yield return` suspends until the next `MoveNextAsync`. That is the opposite of `Channel<T>`, where a producer can run ahead up to the channel's capacity.
- **Disposal.** `await foreach` calls `DisposeAsync` on the enumerator, running `finally` blocks in the iterator, even when the consumer breaks out early.
- **.NET 10** ships `System.Linq.AsyncEnumerable` in the base library (previously the `System.Linq.Async` package), so in production code prefer its `Select`, `Where`, `Chunk`, and `ToListAsync`.
