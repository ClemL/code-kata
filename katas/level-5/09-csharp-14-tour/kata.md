---
title: C# 14 Feature Tour
level: 5
minutes: 35
summary: Extension blocks with extension properties, static extension members and extension operators, null-conditional assignment, modifier-only lambda parameters, nameof on unbound generics, and user-defined compound assignment.
topics: [C# 14, Extension members, Operators, Language features]
keywords: [extension, field, "?.=", nameof, "operator +="]
apis: [DateOnly.DayNumber, "IEnumerable<T>"]
docs:
  - title: What's new in C# 14
    url: https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-14
  - title: Extension members
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
  - title: extension keyword
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/extension
---

## Brief

C# 14 shipped with .NET 10 (November 2025, LTS). Its headline feature is **extension members**: an `extension (receiver) { ... }` block inside a static class can declare extension *properties*, *static* members that appear to belong to the extended type, and *operators*. The release also added a set of smaller conveniences. The starter already contains the new declaration shapes so the tests compile; your job is to implement them and get comfortable reading the syntax. (The `field` keyword, also C# 14, is covered in Level 2's *Properties Deep Dive*.)

## Tasks

1. **Extension properties** on `IEnumerable<T>`: `IsEmpty` (without enumerating more than one element) and on `string`: `WordCount` (whitespace-separated).
2. **Instance extension properties on `DateOnly`**: `Quarter` (1-4) and `EndOfMonth`.
3. **Static extension members on `DateOnly`**: `DateOnly.FirstOfMonth(year, month)` and `DateOnly.ParseIso(text)` (exact `yyyy-MM-dd`, invariant culture), called as if they were defined on `DateOnly` itself.
4. **Extension operator**: `DateOnly - DateOnly` returns the number of days between two dates as an `int`.
5. **Null-conditional assignment**: `Accounts.Touch(customer, now)` sets `LastSeen` and `Accounts.AddPoints(customer, points)` adds to `LoyaltyPoints`, both doing nothing when `customer` is `null`. Use `customer?.LastSeen = now` and `customer?.LoyaltyPoints += points`.
6. **Modifier-only lambda parameters**: implement `Parsers.Int` and `Parsers.IsoDate` (both `TryParser<T>`, a delegate with an `out` parameter) as lambdas like `(text, out value) => ...` without writing parameter types.
7. **`nameof` on unbound generics**: `Names.Of()` returns the names of `List<>` and `Dictionary<,>` using `nameof`.
8. **User-defined compound assignment**: `RunningTotal` implements an instance `operator +=` so `total += 5m` updates the **same object** in place (no new instance), tracking `Total` and `Count`.

## Hints

- Inside `extension<T>(IEnumerable<T> source) { public bool IsEmpty => ...; }`, the receiver is in scope as `source`. `!source.Any()` stops at the first element.
- For static members, the receiver parameter has no name: `extension(DateOnly) { public static DateOnly FirstOfMonth(int year, int month) => new(year, month, 1); }`
- Extension operators are declared like normal operators inside a receiver-type-only extension block: `public static int operator -(DateOnly left, DateOnly right) => left.DayNumber - right.DayNumber;`
- `customer?.LastSeen = now;` evaluates the right side only when `customer` is not null.
- `(text, out value) => int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out value)`.
- Instance compound operators look like `public void operator +=(decimal amount) { ... }`. They return `void` and mutate `this`.

## Notes

- **Compatibility.** Extension methods declared in an extension block compile to the same static methods as classic `this`-parameter extensions, so you can migrate a static class one member at a time without breaking callers.
- **Lookup rules are unchanged.** Instance members still win over extensions, and operators defined on the type itself win over extension operators. If a future .NET release adds a real `DateOnly - DateOnly` operator, it will silently take over.
- **Why instance `+=`?** For mutable types (builders, accumulators, big-number buffers), `x += y` previously had to allocate a new object via `operator +`. The instance form updates in place.
- **Null-conditional assignment does not support `++` or `--`**, and the right-hand side is not evaluated at all when the receiver is null, which matters if it has side effects.
- **Also in C# 14:** implicit `Span<T>` conversions (Level 4), `partial` constructors and events, and `field`-backed properties (Level 2).
