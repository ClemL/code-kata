---
title: Equality & Comparison
level: 2
minutes: 35
summary: Implement IEquatable, IComparable, comparison operators, and HashCode correctly on a SemVer struct, plus a custom IEqualityComparer.
topics: [Equality, Comparison, Sorting, Hashing]
keywords: [operator, struct, readonly, override, "=="]
apis: ["IEquatable<T>", "IComparable<T>", "IEqualityComparer<T>", HashCode.Combine, string.CompareOrdinal]
docs:
  - title: How to define value equality for a type
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type
  - title: HashCode struct
    url: https://learn.microsoft.com/dotnet/api/system.hashcode
  - title: IComparable<T>
    url: https://learn.microsoft.com/dotnet/api/system.icomparable-1
  - title: Semantic Versioning 2.0.0 (precedence rules)
    url: https://semver.org/#spec-item-11
---

## Brief

Records give you equality for free, but ordering, custom precedence rules, and comparers remain hand-written. This kata implements a `SemVer` value type with full equality and ordering semantics, then a separate `IEqualityComparer<T>` for when equality depends on context rather than on the type.

## Tasks

1. **`SemVer.Parse(string text)`** accepts `MAJOR.MINOR.PATCH` with an optional `-prerelease` suffix (`1.4.0`, `2.0.0-rc.1`). Components are non-negative integers. Anything else throws `FormatException`. `TryParse` returns `false` instead of throwing.
2. **Equality.** Implement `Equals(SemVer)`, `Equals(object?)`, `GetHashCode`, `==`, and `!=`. Two versions are equal when all four parts are equal (prerelease compared ordinally).
3. **Ordering.** Implement `CompareTo` and the `<`, `>`, `<=`, `>=` operators using [SemVer precedence](https://semver.org/#spec-item-11):
   - Compare major, minor, patch numerically.
   - A version **with** a prerelease is lower than the same version without one (`1.0.0-alpha < 1.0.0`).
   - Prerelease identifiers (split on `.`) are compared left to right: numeric identifiers numerically, others ordinally; numeric identifiers sort before alphanumeric ones; when all shared identifiers are equal, the shorter list is lower.
4. **`ToString()`** returns the canonical text (`"2.0.0-rc.1"`).
5. **`SkuComparer`** is an `IEqualityComparer<Product>` that treats two products as equal when their `Sku` matches **case-insensitively**, ignoring `Name` and `Price`. `GetHashCode` must be consistent with that.

## Hints

- Split on the first `-` to separate the prerelease. `int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var n)` rejects signs and whitespace.
- `HashCode.Combine(Major, Minor, Patch, PreRelease)` handles null and mixes bits well.
- Chain comparisons: `var c = Major.CompareTo(other.Major); if (c != 0) return c;`
- Implement the four relational operators in terms of `CompareTo`, and `==` in terms of `Equals`.
- `StringComparer.OrdinalIgnoreCase.GetHashCode(sku)` pairs with `StringComparer.OrdinalIgnoreCase.Equals`.

## Notes

- **The hash contract.** If `a.Equals(b)` then `a.GetHashCode() == b.GetHashCode()`. The reverse is not required. Breaking this rule makes `Dictionary` and `HashSet` silently lose items.
- **Struct equality default is slow.** `ValueType.Equals` falls back to reflection when a struct contains reference fields. Implement `IEquatable<T>` on every struct used as a key.
- **Equality vs comparison.** `CompareTo` returning `0` should imply `Equals`. SemVer build metadata (`+build.5`) deliberately breaks this rule, which is why this kata leaves it out.
- **Comparers decouple policy from type.** The same `Product` can be deduplicated by SKU in one place and by full value in another without changing the type.
- **C# 11 generic math** adds `IComparisonOperators<TSelf, TOther, TResult>` so generic code can use `<` directly on `T`.
