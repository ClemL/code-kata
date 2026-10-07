---
title: Channels & Pipelines
level: 4
minutes: 40
summary: Bounded and dropping channels for back-pressure, a multi-worker processing pipeline with error propagation, and fan-in merging of channel readers.
topics: [Channels, Producer/consumer, Back-pressure, Concurrency]
keywords: [async, await, "await foreach", using]
apis: ["Channel<T>", BoundedChannelOptions, BoundedChannelFullMode, "ChannelReader<T>.ReadAllAsync", "ChannelWriter<T>.TryComplete"]
docs:
  - title: System.Threading.Channels
    url: https://learn.microsoft.com/dotnet/core/extensions/channels
  - title: BoundedChannelFullMode
    url: https://learn.microsoft.com/dotnet/api/system.threading.channels.boundedchannelfullmode
---

## Brief

`System.Threading.Channels` is the in-process message queue of modern .NET: Kestrel, SignalR, and many background-processing designs are built on it. A channel decouples producers from consumers, and a **bounded** channel adds back-pressure: when consumers fall behind, producers wait (or the channel drops data) instead of growing memory without limit.

## Tasks

Implement the static methods on `ChannelKata`:

1. **`CreateWorkQueue<T>(int capacity)`** returns a bounded channel that **waits** when full, configured for a single reader. `TryWrite` must fail once `capacity` items are buffered and succeed again after a read.
2. **`CreateLatestOnly<T>(int capacity)`** returns a bounded channel that **drops the oldest** item when full, so readers always see the most recent `capacity` items.
3. **`ProcessAsync(inputs, handler, workerCount, capacity, cancellationToken)`** runs a pipeline:
   - one producer writes `inputs` into a bounded channel of `capacity`;
   - `workerCount` consumers read concurrently and call `handler`;
   - returns every result (order not guaranteed);
   - never runs more than `workerCount` handlers at once;
   - if any handler throws, stops the other workers and rethrows that exception.
4. **`Merge<T>(readers, cancellationToken)`** fans multiple `ChannelReader<T>`s into one reader that yields every item and **completes when all inputs have completed**. If an input completes with an error, the merged reader completes with that error.

## Hints

- `Channel.CreateBounded<T>(new BoundedChannelOptions(capacity) { FullMode = BoundedChannelFullMode.Wait, SingleReader = true })`.
- The producer must call `writer.Complete()` (or `TryComplete(exception)`) so `ReadAllAsync` loops end.
- Start workers with `Enumerable.Range(0, workerCount).Select(_ => Task.Run(...))`. Each one does `await foreach (var item in reader.ReadAllAsync(token))`.
- Use a linked `CancellationTokenSource`; a failing worker cancels it so the others stop promptly. `await Task.WhenAll(...)` rethrows the original fault, because cancelled tasks do not contribute exceptions.
- `ConcurrentBag<T>` or a `lock`-protected list collects results safely.
- For `Merge`, start one pump task per input, then `_ = Task.WhenAll(pumps).ContinueWith(t => output.Writer.TryComplete(t.Exception?.InnerException))`.

## Notes

- **Channels vs `BlockingCollection<T>`.** `BlockingCollection` blocks threads; channels are async all the way down, which is what server code needs.
- **Unbounded channels are a memory leak waiting to happen** if producers can outpace consumers indefinitely. Default to bounded and choose the full mode deliberately.
- **`SingleReader`/`SingleWriter`** enable lock-free fast paths; set them whenever they are true.
- **`ReadAllAsync` vs `WaitToReadAsync` + `TryRead`.** The latter loop drains bursts without an allocation per item and is the pattern in performance-sensitive code.
- **Dataflow alternative.** `System.Threading.Tasks.Dataflow` (TPL Dataflow) offers linked blocks with built-in parallelism limits; channels are lower-level and lighter.
