---
title: Properties Deep Dive
level: 2
minutes: 25
summary: Validated setters with the C# 14 field keyword, computed properties, required/init members, indexers, and cached values with invalidation.
topics: [Properties, Encapsulation, Indexers, C# 14]
keywords: [field, get, set, init, required, value, this]
apis: [ArgumentOutOfRangeException.ThrowIfLessThan, ArgumentException.ThrowIfNullOrWhiteSpace]
docs:
  - title: Properties
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/classes-and-structs/properties
  - title: The field keyword (C# 14)
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/field
  - title: required modifier
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/required
  - title: Indexers
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/indexers/
---

## Brief

C# 14 removed the most common reason to write a manual backing field: inside an accessor, the contextual keyword `field` refers to the compiler-generated backing field. You can add validation or normalization to one accessor and keep the other automatic. Combined with `init`, `required`, and computed properties, most classes need far fewer private fields than they used to.

## Tasks

1. **`Temperature`**
   - `Celsius` has a getter and setter. Setting a value below absolute zero (`-273.15`) throws `ArgumentOutOfRangeException`. Use `field`; no explicit private field.
   - `Fahrenheit` is computed from `Celsius` on get, and on set converts back and stores Celsius (so validation still applies).
   - `Kelvin` is get-only and computed.
2. **`UserProfile`**
   - `required string Email { get; init; }` trims and lower-cases the value as it is initialized. Blank values throw `ArgumentException`.
   - `DisplayName` is optional and init-only. When it was never set, the getter returns `Email`.
3. **`Grid`** is a fixed-size grid of integers with `Rows`, `Columns`, and a two-dimensional indexer `grid[row, column]` for get and set. Out-of-range coordinates throw `ArgumentOutOfRangeException`.
4. **`Document`**
   - `Text` has a getter and setter (default `""`).
   - `WordCount` is computed lazily (words separated by whitespace) and cached until `Text` changes.
   - `Computations` reports how many times the word count was actually calculated.

## Hints

- `public double Celsius { get; set => field = value < AbsoluteZero ? throw ... : value; }`. The getter stays automatic.
- `init => field = value.Trim().ToLowerInvariant();` normalizes on initialization while keeping the property immutable afterwards.
- `get => field ?? Email; init;` works when the backing field can be null; the compiler infers its nullability from the getter.
- Store the grid in a flat `int[]` and index with `row * Columns + column`.
- A cache with invalidation: set a nullable `int? _wordCount` to `null` in the `Text` setter, and fill it with `??=` in the getter.

## Notes

- **`field` and existing members.** If your class already has a member called `field`, use `@field` or `this.field` to refer to it. The compiler warns when the keyword could be ambiguous.
- **`required` is enforced at compile time.** Callers must set the member in an object initializer, or call a constructor marked `[SetsRequiredMembers]`. It is a compile-time contract, not a runtime null check.
- **`init` vs `set`.** `init` accessors run only during object initialization (constructors, object initializers, `with` expressions). They make immutable types usable with initializer syntax.
- **Indexer design.** Indexers can take any parameter types (`this[string key]`, `this[Index i]`, `this[int row, int column]`). Throw `ArgumentOutOfRangeException` from methods you write; `IndexOutOfRangeException` is reserved for the runtime's own array checks.
