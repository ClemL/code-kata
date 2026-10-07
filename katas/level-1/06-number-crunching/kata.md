---
title: Number Crunching
level: 1
minutes: 25
summary: Overflow with checked/unchecked, decimal rounding modes, floating-point comparison, bit tricks, and BigInteger.
topics: [Numeric types, Overflow, Rounding, Bit manipulation]
keywords: [checked, unchecked, decimal, double, long, uint, "&", "<<"]
apis: [Math.Round, MidpointRounding, BigInteger, BitOperations.PopCount, double.Epsilon]
docs:
  - title: checked and unchecked statements
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/checked-and-unchecked
  - title: Floating-point numeric types
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/floating-point-numeric-types
  - title: Math.Round
    url: https://learn.microsoft.com/dotnet/api/system.math.round
  - title: BigInteger
    url: https://learn.microsoft.com/dotnet/api/system.numerics.biginteger
---

## Brief

Integer arithmetic in C# wraps silently by default, `Math.Round` uses banker's rounding unless told otherwise, and `0.1 + 0.2 != 0.3`. None of that is news to a senior engineer, but the exact APIs and defaults are easy to forget. This kata makes each default explicit, which matters in data and money code.

## Tasks

Implement the static methods on `Numbers`:

1. **`CheckedAdd(int a, int b)`** throws `OverflowException` on overflow.
2. **`WrappingAdd(int a, int b)`** wraps around on overflow, regardless of project settings.
3. **`RoundMoney(decimal amount)`** rounds to two places with midpoints going *away from zero* (`2.345 → 2.35`, `-2.345 → -2.35`).
4. **`RoundBankers(decimal amount)`** rounds to two places with midpoints going to the *even* digit (`2.345 → 2.34`, `2.355 → 2.36`).
5. **`NearlyEqual(double a, double b, double relativeTolerance = 1e-9)`** compares with a *relative* tolerance: `|a - b| <= tolerance * max(|a|, |b|)`. Exact equality (including both zero) is always `true`; `NaN` is never equal.
6. **`IsPowerOfTwo(long n)`** uses a bit trick, not a loop or `Math.Log`.
7. **`CountSetBits(uint value)`** returns the number of `1` bits.
8. **`Factorial(int n)`** returns a `BigInteger`; `25!` does not fit in a `long`. Throw `ArgumentOutOfRangeException` for negative `n`.

## Hints

- `checked(a + b)` and `unchecked(a + b)` are expressions. The `<CheckForOverflowUnderflow>` MSBuild property flips the project default, so being explicit matters.
- `Math.Round(amount, 2, MidpointRounding.AwayFromZero)`. The overload without a mode uses `ToEven`.
- `n > 0 && (n & (n - 1)) == 0`: a power of two has exactly one bit set, and subtracting one flips it and every bit below it.
- `System.Numerics.BitOperations.PopCount` compiles down to a single CPU instruction where available.
- `BigInteger` supports `*=` with an `int` on the right through implicit conversion.

## Notes

- **`decimal` for money, `double` for measurement.** `decimal` is a base-10 128-bit type: slower, but `0.1m + 0.2m == 0.3m`. `double` is IEEE 754 base-2 and cannot represent 0.1 exactly.
- **Absolute tolerances fail at scale.** `1e-9` is generous for values near 1 and meaningless for values near `1e15`. A relative tolerance scales with the operands.
- **Generic math (.NET 7+)** gives every primitive static helpers such as `long.IsPow2(n)` and `int.PopCount(x)`. Level 5's *Generic Math* kata builds on this.
- **Digit separators.** `1_000_000` and `0b_1010_0001` are valid literals and make tests easier to read.
