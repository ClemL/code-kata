---
title: Pattern Matching Pro
level: 3
minutes: 40
summary: Extended property patterns, list and slice patterns, and recursive positional patterns over a record-based expression tree.
topics: [Pattern matching, Records, Recursion, Parsing]
keywords: [switch, is, and, or, not, var, "..", when]
apis: [abstract record, string.Join, CultureInfo.InvariantCulture]
docs:
  - title: Patterns (property, positional, list)
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns
  - title: List patterns
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns#list-patterns
  - title: Tutorial - use pattern matching to build type-driven algorithms
    url: https://learn.microsoft.com/dotnet/csharp/fundamentals/tutorials/pattern-matching
---

## Brief

Level 1 used switch expressions on primitives. This kata uses the rest of the pattern language: **extended property patterns** (`{ Customer.Address.Country: "US" }`, C# 10), **list patterns** with slices (`[first, .., last]`, C# 11), and **recursive positional patterns** that deconstruct records inside records. The expression simplifier at the end is the kind of code where pattern matching is far clearer than the visitor pattern.

## Tasks

1. **`Shipping.Cost(Order order)`** returns the first matching rule:
   | Rule | Cost |
   | --- | --- |
   | Premium customer | `0` |
   | Subtotal of at least `100` | `0` |
   | Shipping outside `"US"` | `25` |
   | US state `"AK"` or `"HI"` | `15` |
   | More than 10 items | `12` |
   | Everything else | `7` |
2. **`Lists.Describe(int[] values)`**:
   - `[]` → `"empty"`
   - one element → `"one: 5"`
   - two elements → `"pair: 1, 2"`
   - starts with `0` → `"starts with zero"`
   - last element negative → `"ends negative"`
   - otherwise → `"1..9 (4 items)"` (first, last, and length)
3. **`Commands.Parse(string input)`** splits on spaces and matches with list patterns:
   - `help` → `"help"`
   - `go <north|south|east|west>` → `"move north"`
   - `take <item> [item ...]` → `"take apple, rope"`
   - `say <words...>` → `"say: hello there"` (at least one word)
   - anything else → `"unknown"`
4. **`ExprTools.Evaluate(Expr e, IReadOnlyDictionary<string, double> variables)`** evaluates `Num`, `Var`, `Add`, `Mul`, and `Neg`. Unknown variables throw `KeyNotFoundException`.
5. **`ExprTools.Simplify(Expr e)`** simplifies children first, then applies:
   - `x + 0` and `0 + x` → `x`; `x * 1` and `1 * x` → `x`; `x * 0` and `0 * x` → `0`
   - `-(-x)` → `x`
   - constant folding: `Num + Num`, `Num * Num`, `-Num` → `Num`
6. **`ExprTools.Print(Expr e)`**: `Num` uses invariant `ToString()`, `Var` its name, `Add` → `"(a + b)"`, `Mul` → `"(a * b)"`, `Neg` → `"-a"`.

## Hints

- `order switch { { Customer.IsPremium: true } => 0m, { Subtotal: >= 100 } => 0m, ... }`
- `{ Customer.Address: { Country: "US", State: "AK" or "HI" } }` combines nested and extended property patterns.
- List patterns: `[var only]`, `[var a, var b]`, `[0, ..]`, `[.., < 0]`, `[var first, .., var last]`.
- Slice capture: `["take", .. var items] when items.Length > 0`. Slices on arrays produce arrays.
- Capture and constrain in one element: `["go", var direction and ("north" or "south" or "east" or "west")]`.
- Positional patterns deconstruct records: `Add(Num(0), var x) => x`. Records generate the `Deconstruct` method this needs.
- Simplify bottom-up: `var simplified = e switch { Add(var l, var r) => new Add(Simplify(l), Simplify(r)), ... };` then apply the rules to `simplified`.

## Notes

- **Pattern order is a design decision.** Both the shipping rules and the simplifier depend on order; the compiler flags arms that are fully subsumed by earlier ones (CS8510), but not arms that are merely shadowed for some inputs.
- **No variables inside `or`.** `Add(Num(0), var x) or Add(var x, Num(0))` is error CS8780, because the compiler cannot guarantee `x` is assigned. Write two arms, or declare the variable on the outside of an `and`: `var direction and ("north" or "south")`.
- **`and` binds tighter than `or`.** `"a" or "b" and var x` parses as `"a" or ("b" and var x)`. Parenthesize combinators when mixing them.
- **Double constants in patterns** compare with `double.Equals` semantics, so `Num(0)` also matches `-0.0` and `NaN` matches `double.NaN`.
- **List patterns work on any type** with an indexer and a `Length` or `Count` property; slice patterns additionally need a range indexer or `Slice` method. `List<T>` and spans both qualify.
- **Visitor vs pattern matching.** The visitor pattern makes adding *operations* hard and adding *node types* easy; switch expressions over a closed hierarchy make the opposite trade. C# does not yet have closed hierarchies (discriminated unions are a long-running language proposal), so the compiler cannot prove exhaustiveness here and the discard arm stays.
