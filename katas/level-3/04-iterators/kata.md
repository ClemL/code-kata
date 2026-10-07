---
title: Iterators & Lazy Sequences
level: 3
minutes: 30
summary: yield return and yield break, infinite sequences, eager argument validation, sliding windows, and a hand-written IEnumerator.
topics: [Iterators, Lazy evaluation, IEnumerable]
keywords: [yield, "yield break", using, foreach, IEnumerator]
apis: ["IEnumerable<T>", "IEnumerator<T>", "EqualityComparer<T>.Default", "Queue<T>"]
docs:
  - title: yield statement
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/yield
  - title: Iterators
    url: https://learn.microsoft.com/dotnet/csharp/iterators
  - title: Local functions
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/local-functions
---

## Brief

An iterator method (`yield return`) compiles into a state machine class that implements `IEnumerable<T>` and `IEnumerator<T>`. Nothing in its body runs until the first `MoveNext()`, which is powerful (infinite sequences, streaming) and a classic source of bugs (argument validation that fires late, double enumeration, enumerators that never get disposed).

## Tasks

Implement the members of `Sequences` and `Countdown`:

1. **`Fibonacci()`** yields `0, 1, 1, 2, 3, 5, ...` forever, as `long`.
2. **`Range(int start, int endExclusive, int step)`** yields `start, start + step, ...` while below `endExclusive` (or above it for a negative step). A `step` of `0` must throw `ArgumentOutOfRangeException` **when `Range` is called**, not when the result is enumerated.
3. **`Window<T>(source, int size)`** yields sliding windows of `size` consecutive elements as independent arrays: `[1,2,3,4]` with size 3 gives `[1,2,3]`, `[2,3,4]`. Lazy; validates `size > 0` eagerly.
4. **`Interleave<T>(first, second)`** alternates elements until both are exhausted: `[1,2,3]` and `[a,b]` give `1,a,2,b,3`. Both enumerators must be disposed.
5. **`DistinctUntilChanged<T>(source)`** drops consecutive duplicates: `1,1,2,2,2,1,3,3` gives `1,2,1,3`.
6. **`Countdown(int from)`** is an `IEnumerable<int>` yielding `from, from - 1, ..., 1`, **written without `yield`**: implement a nested `IEnumerator<int>` class by hand (`Current`, `MoveNext`, `Reset`, `Dispose`).

## Hints

- An infinite iterator is just `while (true) { yield return a; (a, b) = (b, a + b); }`.
- Eager validation: make the public method a normal method that validates and then returns a call to a private (or local) iterator function.
- A `Queue<T>` of capacity `size` works as the sliding buffer; `yield return queue.ToArray()` when it is full.
- `using var a = first.GetEnumerator(); using var b = second.GetEnumerator();` then loop while either `MoveNext()` succeeds.
- Track `hasPrevious` and `previous`, and compare with `EqualityComparer<T>.Default.Equals`.
- For the manual enumerator, start `Current` in a "before first" state; `MoveNext` decrements and returns `false` once it reaches zero.

## Notes

- **Iterator state machines are disposable.** `foreach` calls `Dispose()` on the enumerator, which runs any pending `finally` blocks inside the iterator. Breaking out of a `foreach` early is therefore safe; abandoning a manually obtained enumerator is not.
- **Multiple enumeration.** Each `foreach` over an iterator restarts it from scratch. Analyzers flag "possible multiple enumeration" for this reason; materialize with `ToList()` when you need two passes.
- **`yield` restrictions.** Iterators cannot have `ref`/`out` parameters. C# 13 relaxed the rule against `ref struct` locals and `unsafe` code in iterators, as long as they are not used across a `yield`.
- **Allocation.** Each call to an iterator method allocates a state-machine object. LINQ's built-in operators avoid some of that with specialized iterator types; your own code rarely needs to.
