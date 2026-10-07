---
title: Nullable Navigator
level: 1
minutes: 25
summary: Nullable reference types, null-conditional and null-coalescing operators, flow attributes, and lifted nullable arithmetic.
topics: [Nullable reference types, Null safety, Guard clauses]
keywords: ["?.", "??", "??=", "is null", "is not null", "!", out, "null"]
apis: [ArgumentNullException.ThrowIfNull, ArgumentException.ThrowIfNullOrWhiteSpace, NotNullWhenAttribute, CallerArgumentExpressionAttribute, "Nullable<T>"]
docs:
  - title: Nullable reference types
    url: https://learn.microsoft.com/dotnet/csharp/nullable-references
  - title: "?? and ??= operators"
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/null-coalescing-operator
  - title: Attributes for null-state static analysis
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/attributes/nullable-analysis
  - title: Nullable value types
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/nullable-value-types
---

## Brief

With `<Nullable>enable</Nullable>` (the default in every new project template since .NET 6), `string` means "never null" and `string?` means "might be null". The compiler tracks null-state through your code and warns before you dereference something that could be null. This kata exercises the operators and attributes that keep that analysis accurate without sprinkling `!` everywhere.

The `Address` and `Customer` records are provided. Records are covered properly in Level 2.

## Tasks

Implement the members of `NullSafety` and `ConfigCache`:

1. **`CityOrUnknown(Customer? customer)`** returns the customer's city, or `"Unknown"` when the customer, address, or city is null. One expression.
2. **`NormalizeOrNull(string? input)`** trims the input and returns `null` when it is null, empty, or whitespace.
3. **`TryGetPostalCode(Customer? customer, out string? postalCode)`** returns `true` and the postal code when one exists. The signature carries `[NotNullWhen(true)]` so callers can use `postalCode` without a null check after a `true` result.
4. **`Require(string? value, string? expression = null)`** returns `value` when it has non-whitespace content. Otherwise it throws `ArgumentNullException` (null) or `ArgumentException` (empty or whitespace) whose `ParamName` is the *caller's argument expression*: `Require(apiKey)` reports `"apiKey"`.
5. **`SumIgnoringNulls(IEnumerable<int?> values)`** adds the non-null values (empty input returns `0`).
6. **`SumStrict(IEnumerable<int?> values)`** returns `null` if *any* value is null. Use lifted `+` rather than an explicit check.
7. **`ConfigCache.ConnectionString`** loads its value through the injected loader on first access only (use `??=`), and `LoadCount` reports how many times the loader ran.

## Hints

- `customer?.Address?.City ?? "Unknown"` short-circuits the whole chain on the first null.
- `string.IsNullOrWhiteSpace` is annotated with `[NotNullWhen(false)]`, so after a `false` result the compiler knows the argument is not null.
- `[CallerArgumentExpression(nameof(value))]` asks the compiler to pass the source text of the argument. `ArgumentNullException.ThrowIfNull(value, paramName)` accepts it as the parameter name.
- `Enumerable.Sum` over `int?` already skips nulls.
- Lifted operators: `(int?)null + 5` is `null`. `Aggregate((int?)0, (acc, v) => acc + v)` propagates it.

## Notes

- **`!` is a promise, not a check.** The null-forgiving operator silences the compiler and does nothing at runtime. Each one in production code should have a comment or a test explaining why it is safe.
- **`is null` / `is not null`** cannot be overridden by a custom `==` operator, which makes them the most reliable null checks.
- **Value vs reference nullability.** `int?` is `Nullable<int>`, a real struct with `HasValue`/`Value`. `string?` is the same `string` type with an annotation; there is no runtime difference.
- **Guard helpers.** `ArgumentNullException.ThrowIfNull` (.NET 6), `ArgumentException.ThrowIfNullOrEmpty` (.NET 7), and `ThrowIfNullOrWhiteSpace` (.NET 8) all use `CallerArgumentExpression`, so `nameof(...)` is no longer needed.
