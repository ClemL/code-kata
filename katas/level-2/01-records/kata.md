---
title: Records & Value Equality
level: 2
minutes: 25
summary: Convert reference-equality classes into positional records, then use with-expressions, deconstruction, and record inheritance.
topics: [Records, Value equality, Immutability]
keywords: [record, with, init, sealed, "=="]
apis: [EqualityContract, Deconstruct, "HashSet<T>"]
docs:
  - title: Records (language reference)
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/record
  - title: Records tutorial
    url: https://learn.microsoft.com/dotnet/csharp/fundamentals/types/records
  - title: with expression
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/with-expression
---

## Brief

A `record` (C# 9) is a class (or, since C# 10, a struct) where the compiler writes value equality, `GetHashCode`, `ToString`, `Deconstruct`, and non-destructive mutation (`with`) for you. Records are the default choice for DTOs, messages, and value objects. The starter declares `Money`, `Person`, and `Employee` as ordinary classes, so two identical `Money` values are *not* equal. Convert them.

## Tasks

1. **Make `Money` a positional record** with `Amount` and `Currency`. Two `Money` values with the same amount and currency must be equal through `Equals`, `==`, and `GetHashCode`, and `ToString()` must print `Money { Amount = 10, Currency = USD }`. Deconstruction (`var (amount, currency) = money;`) must work.
2. **`Money.Add(Money other)`** returns a new `Money` with the summed amount. Mismatched currencies throw `InvalidOperationException`. Neither operand changes.
3. **`Money.Scale(decimal factor)`** returns a copy with `Amount * factor` rounded to 2 places (midpoint away from zero). Use a `with` expression.
4. **Make `Person` and `Employee` records**, with `Employee` deriving from `Person`. A `Person("Ada")` must **not** equal an `Employee("Ada", ...)`.
5. **`RecordKata.Promote(Employee e, string newTitle)`** returns a copy with the new title via `with`.
6. **`RecordKata.Total(IEnumerable<Money> amounts, string currency)`** sums amounts, starting from zero in `currency`.
7. **`RecordKata.DistinctAmounts(IEnumerable<Money> amounts)`** removes duplicates, keeping first-seen order. Value equality makes this one line.

## Hints

- `public sealed record Money(decimal Amount, string Currency) { ... }` generates the properties, constructor, and `Deconstruct`. Delete the hand-written members.
- `this with { Amount = Amount + other.Amount }` copies every property and overrides one.
- Record inheritance: `public record Employee(string Name, string Title) : Person(Name);`. `Person` cannot be `sealed` then.
- `Aggregate(new Money(0m, currency), (sum, next) => sum.Add(next))`.
- `Distinct()` uses `Equals`/`GetHashCode`, which records now implement.

## Notes

- **`EqualityContract`.** Every record has a protected virtual `EqualityContract` property returning its runtime type. Equality checks it, so a `Person` and an `Employee` with the same name are unequal, and equality stays symmetric.
- **Shallow immutability.** Positional properties are `init`-only, but a record holding a `List<T>` still exposes a mutable list. Equality on a collection property compares references, not contents.
- **`record struct`** (C# 10) gives value equality on a struct and generates *mutable* properties unless you write `readonly record struct`.
- **When not to use records.** Entities with identity (an EF Core entity keyed by `Id`) should not get value equality on every column.
