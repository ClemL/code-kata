---
title: Generics & Constraints
level: 3
minutes: 30
summary: Generic methods and types with class, struct, new(), notnull and interface constraints, static abstract IParsable, and in/out variance.
topics: [Generics, Constraints, Variance]
keywords: [where, class, struct, "new()", notnull, in, out, default, typeof]
apis: ["IComparable<T>", "IParsable<T>", "Dictionary<Type, object>"]
docs:
  - title: Generic type parameters and constraints
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/where-generic-type-constraint
  - title: Covariance and contravariance in generics
    url: https://learn.microsoft.com/dotnet/standard/generics/covariance-and-contravariance
  - title: Generics overview
    url: https://learn.microsoft.com/dotnet/csharp/fundamentals/types/generics
---

## Brief

Generics in .NET are reified: `List<int>` and `List<string>` are distinct runtime types, value-type instantiations get specialized code, and constraints are enforced by both the compiler and the runtime. This kata works through the constraint vocabulary and closes with variance, the feature that lets an `IEnumerable<string>` be used as an `IEnumerable<object>`.

## Tasks

1. **`GenericKata.Clamp<T>(T value, T min, T max)`** where `T : IComparable<T>`. Throw `ArgumentException` when `min > max`.
2. **`GenericKata.MaxOrDefault<T>(IEnumerable<T> source, T fallback)`** returns the maximum (by `IComparable<T>`) or `fallback` for an empty sequence. Single pass.
3. **`GenericKata.ParseOrNull<T>(string? text)`** where `T : struct, IParsable<T>` returns a `T?` parsed with the invariant culture, or `null` on failure. Call the parser through the type parameter: `T.TryParse(...)`.
4. **`Pool<T>`** where `T : class, new()`: `Rent()` returns a pooled instance or creates one with `new T()`; `Return(T item)` makes it available again; `Created` counts constructions.
5. **`TypeMap`** stores one value per type: `Set<T>(T value)` (with `T : notnull`), `Get<T>()` (throws `KeyNotFoundException` when missing), and `TryGet<T>(out T? value)`.
6. **Variance.** Make `IProducer<T>` covariant and `IConsumer<T>` contravariant, then implement `ConstantProducer<T>` and `ListConsumer<T>`, so that an `IProducer<string>` is usable as an `IProducer<object>` and an `IConsumer<object>` as an `IConsumer<string>`.

## Hints

- `value.CompareTo(min) < 0 ? min : ...`. `Math.Clamp` does the same for primitive types only.
- Use the enumerator directly for `MaxOrDefault`: `using var e = source.GetEnumerator(); if (!e.MoveNext()) return fallback;`
- `T.TryParse(text, CultureInfo.InvariantCulture, out var value)` works because `IParsable<T>` declares a static abstract `TryParse` (.NET 7).
- A `Stack<T>` is a perfectly good pool store.
- `typeof(T)` as a dictionary key, and a cast `(T)value` on the way out.
- `interface IProducer<out T>` can only *return* `T`; `interface IConsumer<in T>` can only *accept* `T`.

## Notes

- **Constraint cheat sheet.** `class` (reference type), `class?` (nullable reference type allowed), `struct` (non-nullable value type), `notnull`, `unmanaged` (no references anywhere inside), `new()` (public parameterless constructor), a base class, interfaces, `allows ref struct` (C# 13 anti-constraint for span-like types).
- **Why variance needs interfaces.** Variance applies only to interfaces and delegates, and only for reference-type arguments. `List<string>` is never a `List<object>`, because `List<T>` both accepts and returns `T`.
- **Arrays are covariant too, unsafely.** `object[] a = new string[1]; a[0] = 1;` compiles and throws `ArrayTypeMismatchException` at runtime. Generic variance was designed to avoid that hole.
- **`default(T)`** is `null` for reference types and zeroed memory for value types, which is why `TryGet` uses `[MaybeNullWhen(false)] out T value`.
