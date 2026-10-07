---
title: Ref Returns & Ref Structs
level: 4
minutes: 35
summary: Return references into arrays, update dictionary values in place with CollectionsMarshal, and build ref struct types including a foreach-able tokenizer and a C# 13 ref struct implementing an interface.
topics: [Performance, Ref semantics, Ref structs, C# 13]
keywords: [ref, "ref readonly", "ref struct", scoped, "allows ref struct", readonly]
apis: [CollectionsMarshal.GetValueRefOrAddDefault, "Span<T>", "ReadOnlySpan<T>"]
docs:
  - title: ref struct types
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/ref-struct
  - title: Reference variables (ref locals and ref returns)
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/declarations#reference-variables
  - title: CollectionsMarshal.GetValueRefOrAddDefault
    url: https://learn.microsoft.com/dotnet/api/system.runtime.interopservices.collectionsmarshal.getvaluereforadddefault
  - title: What's new in C# 13 (ref struct interfaces, allows ref struct)
    url: https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-13
---

## Brief

`ref` is not only a parameter modifier. Methods can **return** references, locals can **hold** references, and `ref struct` types (like `Span<T>`) are guaranteed to live only on the stack. C# 13 removed two long-standing limits: ref structs can now implement interfaces, and generic methods can accept them through the `allows ref struct` anti-constraint.

## Tasks

1. **`RefKata.Largest(Span<int> values)`** returns a **reference** to the largest element, so `RefKata.Largest(scores) = 0;` overwrites it in the caller's array. Empty input throws `ArgumentException`.
2. **`RefKata.CountChars(string text)`** returns a `Dictionary<char, int>` of character counts, using `CollectionsMarshal.GetValueRefOrAddDefault` so each character costs a single hash lookup.
3. **`Tokenizer`** is a `ref struct` over a `ReadOnlySpan<char>` and a separator. It supports `foreach` directly (no `IEnumerable`), yielding each non-empty token as a `ReadOnlySpan<char>`: `foreach (var token in new Tokenizer("a,,b", ','))` yields `a` and `b`.
4. **`FixedStringBuilder`** is a `ref struct` that appends into a caller-provided `Span<char>`:
   - `Append(char)` and `Append(ReadOnlySpan<char>)` return `false` (writing nothing) when the text would not fit;
   - `Length` and `ToString()` report what has been written.
5. **`SpanWindow`** is a `ref struct` that implements the `IMeasurable` interface (`Length`, `Sum()`) over a `ReadOnlySpan<int>`. **`RefKata.Describe<T>(T item)`** is generic over `T : IMeasurable, allows ref struct` and returns `"{Length} items, sum {Sum}"`.

## Hints

- Track the index of the maximum, then `return ref values[index];`.
- `ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(counts, c, out _); count++;` increments the value stored *inside* the dictionary.
- `foreach` only needs a `GetEnumerator()` method whose result has `MoveNext()` and `Current`. A ref struct can return itself as the enumerator: `public Tokenizer GetEnumerator() => this;`
- In `MoveNext`, skip separators, then slice up to the next separator with `IndexOf`.
- `FixedStringBuilder` needs a `Span<char>` field (allowed because the type is a ref struct) and an `int` position.
- `public ref struct SpanWindow(ReadOnlySpan<int> values) : IMeasurable` works with a primary constructor.

## Notes

- **What `ref struct` forbids.** Boxing, fields in classes or normal structs, capture by lambdas, and (before C# 13) use in iterators and async methods. Since C# 13 you may use them in iterators and async methods, as long as they are not alive across a `yield` or `await`.
- **`scoped`** (C# 11) tells the compiler a `ref` or span parameter will not escape the method, which relaxes the safety rules for callers.
- **`ref` fields** (C# 11) let a ref struct hold a `ref T` directly; that is how `Span<T>` itself is implemented now.
- **`CollectionsMarshal` is sharp.** The returned reference is invalid after any modification to the dictionary (an add can resize the backing array). Use it in tight loops, and never hold the reference across another insert.
- **Duck typing in C#.** `foreach` (`GetEnumerator`), `await` (`GetAwaiter`), collection initializers (`Add`), deconstruction (`Deconstruct`), and LINQ query syntax are all pattern-based rather than interface-based.
