---
title: "LINQ: Claims Analytics"
level: 3
minutes: 35
summary: Answer real reporting questions over pharmacy claims with Where, GroupBy, OrderBy/ThenBy, MinBy, ToDictionary, and .NET 9's CountBy and AggregateBy.
topics: [LINQ, Aggregation, Grouping, Data analysis]
keywords: [var, "=>", new]
apis: [Where, Select, GroupBy, OrderByDescending, ThenBy, Sum, Average, MinBy, ToDictionary, CountBy, AggregateBy]
docs:
  - title: Standard query operators overview
    url: https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/
  - title: Grouping data
    url: https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/grouping-data
  - title: What's new in .NET 9 libraries (CountBy, AggregateBy, Index)
    url: https://learn.microsoft.com/dotnet/core/whats-new/dotnet-9/libraries
---

## Brief

LINQ over objects is the same mental model as SQL `GROUP BY`, just composed in C#. This kata uses a small pharmacy claims data set (paid and reversed claims across three pharmacies) and asks the questions an analytics team asks every week. Reversed claims (`IsReversed == true`) are excluded from spend unless a task says otherwise.

## Tasks

Implement the static methods on `ClaimsReport`:

1. **`TotalPaid(claims)`**: total `PaidAmount` of non-reversed claims.
2. **`TopDrugsBySpend(claims, n)`**: the `n` drugs with the highest non-reversed spend, as `(DrugName, Spend)` tuples ordered by spend descending, then name ascending.
3. **`MonthlySpend(claims)`**: non-reversed spend per calendar month as `(DateOnly Month, decimal Spend)` where `Month` is the first day of the month, in chronological order.
4. **`PharmaciesWithReversalRateAbove(claims, threshold)`**: IDs of pharmacies whose share of reversed claims (over **all** their claims) is strictly greater than `threshold`, sorted ordinally.
5. **`AverageQuantityByNdc(claims)`**: a dictionary from NDC to average `Quantity` of non-reversed claims.
6. **`FirstFillPerDrug(claims)`**: a dictionary from drug name to the earliest non-reversed claim for that drug.
7. **`DuplicateClaimIds(claims)`**: claim IDs that appear more than once (a data-quality check), sorted.
8. **`ClaimCountByPharmacy(claims)`**: number of claims (all claims) per pharmacy, using `CountBy`.
9. **`SpendByPharmacy(claims)`**: non-reversed spend per pharmacy, using `AggregateBy`.

## Hints

- Factor out `claims.Where(c => !c.IsReversed)` into a private helper so every query starts from the same set.
- `GroupBy(c => c.DrugName).Select(g => (g.Key, g.Sum(c => c.PaidAmount)))`, then `OrderByDescending(...).ThenBy(...)`, then `Take(n)`.
- `new DateOnly(c.FillDate.Year, c.FillDate.Month, 1)` is a natural month key.
- `g.Count(c => c.IsReversed) / (double)g.Count()` computes a rate.
- `MinBy(c => c.FillDate)` returns the element itself, not just the date.
- `CountBy(c => c.PharmacyId)` and `AggregateBy(c => c.PharmacyId, 0m, (sum, c) => sum + c.PaidAmount)` both return `KeyValuePair` sequences, which have a `ToDictionary()` overload.

## Notes

- **Deferred execution.** Every query here is lazy until `ToList`, `ToDictionary`, `Sum`, or `foreach`. Enumerating an un-materialized query twice runs it twice, which matters when the source is a database or a network stream.
- **`GroupBy` vs `CountBy`/`AggregateBy`.** `GroupBy` buffers every element of every group. `CountBy` and `AggregateBy` (.NET 9) keep only the running aggregate per key, which uses far less memory on large inputs.
- **`IQueryable` is a different contract.** With EF Core, the same operators are translated to SQL. Methods with no SQL translation (some overloads of `MinBy` in older providers, custom C# methods) fail at runtime or silently switch to client evaluation.
- **Decimal for money, double for ratios.** `Average` over `int` returns `double`; over `decimal` it returns `decimal`.
