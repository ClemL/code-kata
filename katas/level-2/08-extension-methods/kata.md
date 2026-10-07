---
title: Extension Methods
level: 2
minutes: 20
summary: Write classic this-parameter extension methods on strings, sequences, dictionaries, and nullable receivers, and learn how they bind.
topics: [Extension methods, Static classes, API design]
keywords: [this, static, where, class, yield]
apis: ["IDictionary<TKey,TValue>", "IEnumerable<T>", DateOnly]
docs:
  - title: Extension methods
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
  - title: Static classes and static class members
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members
  - title: Extension members (C# 14)
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/extension
---

## Brief

Extension methods are how LINQ attaches `Where` and `Select` to every `IEnumerable<T>`. They are static methods in a non-generic static class whose first parameter carries the `this` modifier, and the compiler rewrites `x.Foo()` into `Extensions.Foo(x)`. This kata uses the classic syntax. Level 5 revisits the topic with C# 14 extension *blocks*, which add extension properties and static extension members.

## Tasks

Implement the methods in `StringExtensions`, `SequenceExtensions`, `DictionaryExtensions`, and `DateExtensions`:

1. **`string.Truncate(int maxLength, string ellipsis = "...")`** returns the string unchanged when it fits; otherwise returns a string of exactly `maxLength` characters ending in `ellipsis`. Throw `ArgumentOutOfRangeException` when `maxLength` is shorter than the ellipsis.
2. **`string?.OrEmpty()`** returns `""` for a null receiver. Calling it on a null reference must not throw.
3. **`IEnumerable<T?>.WhereNotNull()`** (for reference types) filters out nulls and returns `IEnumerable<T>` with non-nullable elements. Lazy.
4. **`IEnumerable<T>.WithIndex()`** yields `(int Index, T Item)` pairs. Lazy.
5. **`IDictionary<TKey, TValue>.GetOrAdd(TKey key, Func<TKey, TValue> factory)`** returns the existing value or adds and returns the factory's value. The factory runs only on a miss.
6. **`DateOnly.IsBetween(DateOnly start, DateOnly end)`** is inclusive on both ends and works regardless of argument order.

## Hints

- Extension methods live in a `public static class` that is not nested and not generic.
- `this string? value` lets the method run with a null receiver, because the call is really a static call: `StringExtensions.OrEmpty(null)`.
- `where T : class` plus `IEnumerable<T?>` in and `IEnumerable<T>` out; `foreach` + `if (item is not null) yield return item;`
- .NET 9 added `Enumerable.Index()`, which does what `WithIndex` does. Name collisions with BCL methods are resolved in favor of the BCL *instance* method first, then the closest namespace.
- `if (start > end) (start, end) = (end, start);`

## Notes

- **Binding rules.** Instance methods always win over extension methods with the same signature, so an extension can never override existing behavior. That is also why adding an instance method to a type can silently change which code runs at existing call sites.
- **Discoverability.** Extensions only appear when their namespace is imported. Put general-purpose ones in a namespace your project imports globally (`global using`), and keep domain-specific ones close to the domain.
- **Don't extend `object`.** Extensions on `object` or unconstrained `T` pollute IntelliSense for every type in scope.
- **C# 14 extension blocks** (`extension(string s) { ... }`) support properties, operators, and static members, and can coexist with the classic syntax shown here. The two forms compile to the same metadata for methods.
