---
title: Generic Math & Static Abstracts
level: 5
minutes: 40
summary: Write numeric algorithms once for every number type with INumberBase, IBinaryInteger and IFloatingPointIeee754, then define your own static abstract interface.
topics: [Generic math, Static abstract members, Operators, Interfaces]
keywords: [static abstract, where, operator, interface]
apis: ["INumberBase<T>", "IBinaryInteger<T>", "IFloatingPointIeee754<T>", "IAdditionOperators<TSelf,TOther,TResult>", "IAdditiveIdentity<TSelf,TResult>", CreateChecked]
docs:
  - title: Generic math
    url: https://learn.microsoft.com/dotnet/standard/generics/math
  - title: Static abstract interface members tutorial
    url: https://learn.microsoft.com/dotnet/csharp/whats-new/tutorials/static-virtual-interface-members
  - title: INumberBase<TSelf>
    url: https://learn.microsoft.com/dotnet/api/system.numerics.inumberbase-1
---

## Brief

Before .NET 7, a `Sum` method had to be written once per numeric type, or rely on `dynamic`. **Static abstract interface members** (C# 11) let interfaces require static methods and operators, and the BCL used them to describe arithmetic: `INumberBase<T>`, `IBinaryInteger<T>`, `IFloatingPointIeee754<T>`, and dozens of finer-grained operator interfaces. This kata writes algorithms against those interfaces and then defines an interface of its own.

## Tasks

1. **`GenericStats.Sum<T>(IEnumerable<T> values)`** works for any `T` with `+` and an additive identity (`IAdditionOperators<T, T, T>` and `IAdditiveIdentity<T, T>`): `int`, `double`, `decimal`, and the `Money` type below.
2. **`GenericStats.Average<T, TResult>(IEnumerable<T> values)`** accumulates in `TResult` (so `int` inputs cannot overflow) and returns `TResult`. Empty input throws `InvalidOperationException`.
3. **`GenericStats.StandardDeviation<T>(IReadOnlyCollection<T> values)`** returns the **population** standard deviation for any IEEE floating-point type (`float`, `double`, `Half`). Empty input throws `InvalidOperationException`.
4. **`GenericStats.Gcd<T>(T a, T b)`** computes the greatest common divisor for any binary integer (`int`, `long`, `byte`, `BigInteger`) and always returns a non-negative value.
5. **`Money`** implements `IAdditionOperators<Money, Money, Money>` and `IAdditiveIdentity<Money, Money>` so it works with `Sum`.
6. **`IMonoid<TSelf>`** declares a `static abstract` identity and combine operation. Implement it on `MaxInt` (identity `int.MinValue`, combine = max) and `Csv` (identity empty, combine joins non-empty values with `","`), then implement **`GenericStats.Fold<T>(IEnumerable<T> items)`** for any monoid.

## Hints

- Static members are called through the type parameter: `T.AdditiveIdentity`, `T.Zero`, `T.Sqrt(x)`, `TResult.CreateChecked(value)`.
- `CreateChecked` throws on overflow, `CreateSaturating` clamps, `CreateTruncating` wraps. All three convert between any two numeric types.
- Population variance: `Σ(x − mean)² / n`. Build `n` with `T.CreateChecked(values.Count)`.
- Euclid: `while (b != T.Zero) (a, b) = (b, a % b); return T.Abs(a);`
- `static abstract TSelf Identity { get; }` and `static abstract TSelf Combine(TSelf left, TSelf right);` in the interface; implementations are ordinary `public static` members.
- `items.Aggregate(T.Identity, T.Combine)` passes a static method group as the delegate.

## Notes

- **The self-referencing constraint** `where TSelf : IMonoid<TSelf>` is the "curiously recurring template pattern". It lets the interface talk about the implementing type in its signatures.
- **Static abstracts cannot be called through the interface** (`IMonoid<MaxInt>.Identity` is an error); they are only reachable through a type parameter or the concrete type. Since C# 11 you can also declare `static virtual` members with a default implementation.
- **Performance.** Generic math is resolved at JIT time for value types, so `Sum<int>` compiles to the same machine code as a hand-written `int` loop. There is no boxing or interface dispatch.
- **Where it shows up.** `Enumerable.Sum` and `Average` in .NET 8+, `Vector<T>`, `System.Numerics.Tensors`, and ML.NET all use these interfaces.
