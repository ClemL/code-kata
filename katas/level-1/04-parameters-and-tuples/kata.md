---
title: Parameters & Tuples
level: 1
minutes: 20
summary: ref, out, in, params collections, optional and named arguments, and named tuple returns.
topics: [Methods, Parameter passing, Tuples]
keywords: [ref, out, in, params, readonly, struct]
apis: ["ReadOnlySpan<T>", Math.DivRem, "ValueTuple"]
docs:
  - title: Method parameters and modifiers
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/method-parameters
  - title: Tuple types
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/value-tuples
  - title: Named and optional arguments
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/named-and-optional-arguments
---

## Brief

C# gives you more control over argument passing than most languages: by value, by reference (`ref`), output-only (`out`), read-only reference (`in`), and variadic (`params`). C# 13 extended `params` beyond arrays to any collection type, including `ReadOnlySpan<T>`, which lets callers pass a variable argument list without a heap allocation. Named tuples cover most of the "I need to return two things" cases that once required `out`.

## Tasks

Implement the static methods on `Args`:

1. **`Swap<T>(ref T a, ref T b)`** swaps two variables in the caller. One line, using tuple deconstruction.
2. **`TryDivide(int dividend, int divisor, out int quotient, out int remainder)`** returns `false` (with both outputs `0`) when `divisor` is `0`. Otherwise returns `true`.
3. **`Sum(params ReadOnlySpan<int> values)`** adds any number of integers, including none.
4. **`MinMax(params ReadOnlySpan<int> values)`** returns a named tuple `(int Min, int Max)` in a single pass. Throw `ArgumentException` for an empty input.
5. **`Greet(string name, int age = 0, string greeting = "Hello")`** returns `"{greeting}, {name}!"`, followed by `" ({age})"` when `age > 0`. The tests call it with named arguments in different orders.
6. **`Distance(in Point3 a, in Point3 b)`** returns the Euclidean distance between two `readonly record struct` points passed by read-only reference.

## Hints

- `(a, b) = (b, a);` swaps without a temporary variable you have to name.
- `Math.DivRem(dividend, divisor)` returns a `(Quotient, Remainder)` tuple (.NET 6+).
- A `params ReadOnlySpan<int>` parameter can be iterated with `foreach` like any collection.
- Start `MinMax` with `values[0]` as both min and max, then iterate `values[1..]`.
- `Math.Sqrt(dx * dx + dy * dy + dz * dz)`, or `double.Hypot` for two dimensions.

## Notes

- **`ref` vs `out` vs `in`.** `ref` requires the caller to initialize the variable and allows the callee to read and write it. `out` requires the callee to assign it before returning. `in` passes by reference but the callee cannot modify it, which avoids copying large structs.
- **`in` with mutable structs is a trap.** Calling a non-readonly method on an `in` parameter forces a *defensive copy*. Declaring the struct `readonly` (as `Point3` is) removes the copies.
- **`params` collections (C# 13).** `params ReadOnlySpan<T>` avoids the hidden array allocation of `params T[]`. Callers can also pass a collection expression: `Args.Sum([.. numbers])`.
- **Tuple element names are compile-time only.** At runtime it is `ValueTuple<int, int>` with `Item1`/`Item2`, so names do not survive reflection or serialization. Use a record for anything that crosses an API boundary.
