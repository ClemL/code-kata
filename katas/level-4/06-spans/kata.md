---
title: "Span<T> & Zero-Allocation Parsing"
level: 4
minutes: 40
summary: Parse, slice, format, and transform text with Span<T> and ReadOnlySpan<char>, verified by tests that assert zero heap allocations.
topics: [Performance, Memory, Span, Parsing]
keywords: [stackalloc, ref struct, out, scoped]
apis: ["Span<T>", "ReadOnlySpan<T>", MemoryExtensions.Split, MemoryExtensions.Trim, int.Parse, ISpanFormattable.TryFormat, GC.GetAllocatedBytesForCurrentThread]
docs:
  - title: Memory and spans
    url: https://learn.microsoft.com/dotnet/standard/memory-and-spans/
  - title: Memory<T> and Span<T> usage guidelines
    url: https://learn.microsoft.com/dotnet/standard/memory-and-spans/memory-t-usage-guidelines
  - title: stackalloc expression
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/stackalloc
---

## Brief

`Span<T>` is a view over contiguous memory (an array, a slice of a string, a `stackalloc` buffer, or native memory) that you can slice without copying. Most of the BCL now accepts spans (`int.Parse`, `Encoding.GetBytes`, `TryFormat`), which makes it possible to parse and format text without allocating a single object. The tests in this kata measure allocations with `GC.GetAllocatedBytesForCurrentThread()` and require **zero bytes** for every method except `DigitsOnly`.

## Tasks

Implement the static methods on `SpanKata`:

1. **`SumCsv(ReadOnlySpan<char> line)`** sums comma-separated integers (surrounding whitespace allowed): `" 1, 2,3 "` → `6`. A blank line returns `0`; a bad number throws `FormatException`. **No allocations.**
2. **`ReverseInPlace(Span<int> values)`** reverses the elements in place. **No allocations.**
3. **`CountWords(ReadOnlySpan<char> text)`** counts runs of non-whitespace characters. **No allocations.**
4. **`TryFormatHex(uint value, Span<char> destination, out int charsWritten)`** writes `0x` followed by 8 upper-case hex digits (`0x0000BEEF`). Returns `false` (and `charsWritten = 0`) when `destination` is shorter than 10 characters. **No allocations.**
5. **`TryParseKeyValue(ReadOnlySpan<char> input, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)`** splits on the **first** `=`, trims both sides, and returns `false` when there is no `=` or the key is empty. `"  timeout = 30 "` gives `key = "timeout"`, `value = "30"`. **No allocations.**
6. **`DigitsOnly(ReadOnlySpan<char> input)`** returns a new string containing only the ASCII digits of `input` (`"(617) 555-0100"` → `"6175550100"`). Use a `stackalloc` buffer for inputs up to 256 characters; the only allocation should be the returned string.

## Hints

- .NET 9 added `MemoryExtensions.Split(ReadOnlySpan<char>, char)`, which enumerates `Range` values: `foreach (var range in line.Split(',')) total += int.Parse(line[range], ...)`.
- `int.Parse(ReadOnlySpan<char>, NumberStyles.Integer, CultureInfo.InvariantCulture)` accepts leading/trailing whitespace and a sign.
- `line.IsWhiteSpace()` handles the blank case.
- `values.Reverse()` exists as a span extension, but write the two-index swap once to see how little code it is.
- `"0x".CopyTo(destination)` then `value.TryFormat(destination[2..], out var written, "X8")`.
- `input.IndexOf('=')`, then `input[..index].Trim()` and `input[(index + 1)..].Trim()`.
- `Span<char> buffer = input.Length <= 256 ? stackalloc char[input.Length] : new char[input.Length];` then `new string(buffer[..count])`.

## Notes

- **Spans live on the stack.** `Span<T>` is a `ref struct`: it cannot be boxed, stored in a field of a class, captured by a lambda, or used across an `await`. Use `Memory<T>` when you need to keep a reference to the buffer.
- **`stackalloc` size.** The default stack is 1 MB on Windows (8 MB on Linux, configurable), shared with everything else on that thread. Cap `stackalloc` at a few hundred bytes to a few KB and fall back to the heap (or `ArrayPool`, next kata) above that.
- **C# 14 first-class span conversions.** Arrays and strings now convert implicitly to `Span<T>`/`ReadOnlySpan<T>` in more places, including as extension-method receivers. One side effect: `array.Reverse()` on an `int[]` may now bind to the in-place `MemoryExtensions.Reverse` instead of LINQ's `Enumerable.Reverse`.
- **Measuring allocations.** `GC.GetAllocatedBytesForCurrentThread()` is precise and cheap, so it is good for regression tests like these. For real benchmarking, use BenchmarkDotNet with `[MemoryDiagnoser]`.
