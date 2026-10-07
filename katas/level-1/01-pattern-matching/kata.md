---
title: Pattern Matching 101
level: 1
minutes: 20
summary: Replace if/else ladders with switch expressions, tuple patterns, and relational/logical patterns.
topics: [Pattern matching, Switch expressions, Control flow]
keywords: [switch, when, is, and, or, not, "null", throw, var]
apis: [ArgumentOutOfRangeException.ThrowIfNegativeOrZero, DateOnly, DayOfWeek]
docs:
  - title: Pattern matching overview
    url: https://learn.microsoft.com/dotnet/csharp/fundamentals/functional/pattern-matching
  - title: Patterns (language reference)
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns
  - title: switch expression
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/switch-expression
---

## Brief

Since C# 8 the `switch` keyword doubles as an **expression**, and C# 9 added relational (`< 0`) and logical (`and`, `or`, `not`) patterns. Together they replace most if/else ladders with a declarative table the compiler can check for gaps and dead arms. This kata is the warm-up: every method should be one `switch` or `is` expression.

## Tasks

Implement the static methods on `Patterns`:

1. **`FizzBuzz(int n)`** returns `"FizzBuzz"` when `n` is divisible by 3 and 5, `"Fizz"` for 3, `"Buzz"` for 5, otherwise `n` as a string. Throw `ArgumentOutOfRangeException` when `n <= 0`. Switch over the tuple `(n % 3, n % 5)`.
2. **`Grade(int score)`** maps `90-100 → "A"`, `80-89 → "B"`, `70-79 → "C"`, `60-69 → "D"`, `0-59 → "F"`. Scores outside `0..100` throw `ArgumentOutOfRangeException`. Use relational patterns, not `if`.
3. **`Quadrant(int x, int y)`** returns `"origin"`, `"x-axis"`, `"y-axis"`, `"Q1"`, `"Q2"`, `"Q3"`, or `"Q4"` using positional (tuple) patterns.
4. **`Describe(object? value)`** classifies an arbitrary value:
   | Input | Result |
   | --- | --- |
   | `null` | `"null"` |
   | `0` (int) | `"zero"` |
   | negative `int` | `"negative int"` |
   | positive `int` | `"positive int"` |
   | `""` | `"empty string"` |
   | other `string` | `"string of length N"` |
   | `bool` | `"yes"` / `"no"` |
   | `double.NaN` | `"not a number"` |
   | other `double` | `"double"` |
   | anything else | `"unknown: TypeName"` (use `GetType().Name`) |
5. **`IsWeekend(DateOnly date)`** returns `true` for Saturday and Sunday, using `is ... or ...`.

## Constraints

- No `if` statements except argument guards (`ArgumentOutOfRangeException.ThrowIfNegativeOrZero` counts as a guard).
- Each method body should be a single expression where possible.

## Hints

- Tuple switch: `(n % 3, n % 5) switch { (0, 0) => "FizzBuzz", (0, _) => "Fizz", ... }`.
- Arms are evaluated top to bottom. If an arm can never match because an earlier arm covers it, the compiler reports error CS8510.
- `throw` is an expression, so an arm can be `< 0 or > 100 => throw new ArgumentOutOfRangeException(...)`.
- A constant pattern against `object` checks both type and value: `0 => "zero"` only matches a boxed `int` 0.
- Combine a type pattern with a relational one: `int and < 0 => "negative int"`.

## Notes

- **Exhaustiveness.** The compiler warns (CS8509) when a switch expression does not cover every input. At runtime an unmatched input throws `SwitchExpressionException`, so the discard arm `_` is how you state the default explicitly.
- **Order matters for overlapping arms.** In `Grade`, once `< 0 or > 100` is handled first, the remaining arms can be plain `>= 90`, `>= 80`, and so on.
- **`is` vs `==`.** `x is null` cannot be hijacked by an overloaded `==` operator, which is why it is the idiomatic null check.
- **Property patterns** (`string { Length: 0 }`) and **list patterns** appear in Level 3's *Pattern Matching Pro*.
