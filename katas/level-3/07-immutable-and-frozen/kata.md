---
title: Immutable & Frozen Collections
level: 3
minutes: 25
summary: Read-only wrappers vs true immutability, ImmutableList/Dictionary with builders, and .NET 8 FrozenDictionary/FrozenSet for read-heavy lookups.
topics: [Immutability, Collections, Performance]
keywords: [readonly, init, record]
apis: ["ImmutableList<T>", "ImmutableArray<T>", "ImmutableDictionary<TKey,TValue>", "FrozenDictionary<TKey,TValue>", "FrozenSet<T>", AsReadOnly]
docs:
  - title: System.Collections.Immutable
    url: https://learn.microsoft.com/dotnet/api/system.collections.immutable
  - title: System.Collections.Frozen
    url: https://learn.microsoft.com/dotnet/api/system.collections.frozen
  - title: ImmutableArray<T> vs ImmutableList<T>
    url: https://learn.microsoft.com/dotnet/api/system.collections.immutable.immutablearray-1
---

## Brief

`IReadOnlyList<T>` only promises that *you* won't modify the collection; the owner still can. Immutable collections promise that *nobody* can, and frozen collections (.NET 8) trade a slower one-time build for the fastest possible lookups afterwards. Picking the right one is a design decision about ownership and access patterns.

## Tasks

1. **`Snapshots.ReadOnlyView(List<int> source)`** returns a read-only wrapper that reflects later changes to `source`.
2. **`Snapshots.Snapshot(List<int> source)`** returns an `ImmutableArray<int>` that does **not** change when `source` changes.
3. **`Snapshots.Squares(int count)`** builds an `ImmutableArray<int>` of `0², 1², ...` using a builder with the exact capacity and `MoveToImmutable()`.
4. **`AuditTrail`** is an immutable log:
   - `AuditTrail.Empty` is a shared empty instance;
   - `Append(string entry)` returns a **new** trail; the original is unchanged;
   - `Entries` exposes the entries in order.
5. **`Inventory`** wraps an `ImmutableDictionary<string, int>` of SKU → quantity:
   - `Restock(sku, quantity)` returns a new inventory with the quantity added;
   - `Remove(sku, quantity)` returns a new inventory, removing the SKU entirely at zero, and throws `InvalidOperationException` when stock is insufficient;
   - `QuantityOf(sku)` returns `0` for unknown SKUs.
6. **`CodeTables.BuildLookup(rows)`** returns a case-insensitive `FrozenDictionary<string, string>` from code to description. Duplicate codes (case-insensitively) throw `ArgumentException`.
7. **`CodeTables.BuildAllowList(codes)`** returns a case-insensitive `FrozenSet<string>`.

## Hints

- `source.AsReadOnly()` returns a `ReadOnlyCollection<T>` wrapper (no copy).
- `ImmutableArray.CreateRange(source)` or `[.. source]` with an `ImmutableArray<int>` target type copies.
- `var builder = ImmutableArray.CreateBuilder<int>(count);` then `builder.MoveToImmutable()` (throws if `Count != Capacity`, which guarantees no wasted space).
- `ImmutableList<T>.Add` returns a new list sharing most of its structure with the old one.
- `ImmutableDictionary.SetItem` and `Remove` return new dictionaries. `GetValueOrDefault(sku)` returns `0` for a missing `int`.
- `rows.ToFrozenDictionary(r => r.Code, r => r.Description, StringComparer.OrdinalIgnoreCase)` throws on duplicate keys.

## Notes

- **`ImmutableArray<T>` vs `ImmutableList<T>`.** `ImmutableArray<T>` is a struct wrapper over a plain array: fastest reads, but every `Add` copies the whole array. `ImmutableList<T>` is a balanced tree: O(log n) reads and writes with structural sharing. Build once and read often → array; modify incrementally → list.
- **`default(ImmutableArray<T>)` is not empty.** It wraps a null array and throws on most operations; check `IsDefault`, or use `ImmutableArray<T>.Empty`.
- **Frozen collections** analyze their keys at construction (string length buckets, perfect hashing for small sets), so they suit configuration and lookup tables built at startup and read on every request.
- **Thread safety.** Immutable and frozen collections are safe to share across threads without locks, because nothing can change after construction.
