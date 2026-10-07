---
title: Operators & Conversions
level: 2
minutes: 30
summary: Build an immutable Fraction struct with arithmetic operators, implicit/explicit conversions, and a safe default value.
topics: [Operator overloading, Conversions, Value types]
keywords: [operator, implicit, explicit, checked, readonly, struct, default]
apis: [Math.Abs, "IEquatable<T>", "IComparable<T>"]
docs:
  - title: Operator overloading
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/operator-overloading
  - title: User-defined conversion operators
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/user-defined-conversion-operators
  - title: Structure types
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/struct
---

## Brief

Operator overloading is easy to write and easy to get subtly wrong: normalization, sign handling, overflow, and the `default` value of a struct (which never runs your constructor). `Fraction` is small enough to finish in one sitting and touches all of those.

## Tasks

Implement the `Fraction` readonly struct:

1. **Constructor `Fraction(long numerator, long denominator)`** stores the value in lowest terms with a positive denominator: `new Fraction(2, -4)` becomes `-1/2`. A zero denominator throws `DivideByZeroException`. Zero is stored as `0/1`.
2. **`default(Fraction)`** must behave as zero (`0/1`) even though no constructor ran. `Denominator` must never return `0`.
3. **Arithmetic operators:** unary `-`, binary `+`, `-`, `*`, `/`. Dividing by a zero fraction throws `DivideByZeroException`. Results are normalized. Intermediate overflow throws `OverflowException` rather than silently wrapping.
4. **Comparison and equality:** `==`, `!=`, `<`, `>`, `IEquatable<Fraction>`, `IComparable<Fraction>`, `Equals(object?)`, `GetHashCode`.
5. **Conversions:**
   - `implicit` from `int` (lossless: `Fraction f = 3;`).
   - `explicit` to `double` (lossy: `(double)f`).
6. **`ToString()`** returns `"3/4"`, `"-1/2"`, or `"2"` when the denominator is `1`.
7. **`Parse(string text)`** accepts `"3/4"`, `"-3/4"`, and whole numbers like `"5"`. Invalid text throws `FormatException`.

## Hints

- Greatest common divisor: `while (b != 0) (a, b) = (b, a % b); return Math.Abs(a);`
- Wrap arithmetic in `checked(...)` so overflow throws instead of wrapping.
- For `default`, store the denominator in a private field and expose `Denominator => _denominator == 0 ? 1 : _denominator;`
- Compare without division: `a/b < c/d` is equivalent to `a*d < c*b` when `b` and `d` are positive.
- Because values are always normalized, equality is a field-by-field comparison.

## Notes

- **Implicit vs explicit.** Make a conversion implicit only when it cannot fail and cannot lose information. `int → Fraction` qualifies; `Fraction → double` does not.
- **Operators come in pairs.** Defining `==` without `!=` is a compile error, as is `<` without `>`. Defining `==` without overriding `Equals` and `GetHashCode` produces warnings.
- **Struct defaults.** Arrays, uninitialized fields, and `default(T)` produce zeroed structs without calling any constructor. Since C# 10 you can declare a parameterless struct constructor, but `default` still bypasses it.
- **Generic math.** Implementing `IAdditionOperators<Fraction, Fraction, Fraction>` and related interfaces (Level 5) would let `Fraction` participate in generic algorithms like `Sum<T>()`.
- **C# 14 compound assignment operators.** Mutable types can now define `operator +=` directly to update in place. Immutable value types like this one should keep the `+` operator, which the compiler uses for `+=` automatically.
