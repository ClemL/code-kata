---
title: Expression Trees
level: 5
minutes: 40
summary: Build predicates at runtime, combine them with an ExpressionVisitor, extract member paths from lambdas, and compile fast property getters.
topics: [Expression trees, Metaprogramming, LINQ providers]
keywords: [Expression, "=>", typeof]
apis: ["Expression<TDelegate>", Expression.Property, Expression.Equal, Expression.Lambda, ExpressionVisitor, IQueryable]
docs:
  - title: Expression trees
    url: https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/
  - title: Build expression trees
    url: https://learn.microsoft.com/dotnet/csharp/advanced-topics/expression-trees/expression-trees-building
  - title: ExpressionVisitor
    url: https://learn.microsoft.com/dotnet/api/system.linq.expressions.expressionvisitor
---

## Brief

When a lambda is assigned to `Expression<Func<T, bool>>` instead of `Func<T, bool>`, the compiler produces a **data structure** describing the code instead of IL. EF Core translates that structure to SQL; mocking libraries read it to know which member you configured; validation libraries use it to get property names without magic strings. This kata builds, rewrites, inspects, and compiles expression trees.

## Tasks

Implement the static methods on `ExpressionKata`:

1. **`PropertyEquals<T>(string propertyName, object? value)`** builds `x => x.<propertyName> == value` as an `Expression<Func<T, bool>>`. The value is converted to the property's type (so `PropertyEquals<Patient>("Age", 42L)` works for an `int` property). Unknown properties throw `ArgumentException`.
2. **`AndAlso<T>(left, right)`** combines two predicates into one lambda with a **single** parameter. The two inputs have different parameter objects, so rewrite one body with an `ExpressionVisitor`. The result must work with `IQueryable.Where`.
3. **`MemberPath<T>(Expression<Func<T, object?>> selector)`** returns the dotted member path: `p => p.Name` → `"Name"`, `p => p.Address.City` → `"Address.City"`, and `p => p.Age` → `"Age"` (value types arrive wrapped in a `Convert` node). Anything else throws `ArgumentException`.
4. **`CompileGetter<T, TValue>(string propertyName)`** returns a compiled `Func<T, TValue>` that reads the property without reflection on each call. Unknown properties throw `ArgumentException`.

## Hints

- `var x = Expression.Parameter(typeof(T), "x"); var member = Expression.Property(x, propertyName);` throws `ArgumentException` for unknown names.
- Convert the value first: `Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type, CultureInfo.InvariantCulture)`, then `Expression.Constant(converted, member.Type)`.
- `Expression.Lambda<Func<T, bool>>(Expression.Equal(member, constant), x)`.
- A parameter-replacing visitor overrides `VisitParameter` and returns the replacement when `node == _from`.
- `Expression.AndAlso(left.Body, rewrittenRightBody)` (short-circuit `&&`), not `Expression.And` (bitwise `&`).
- For `MemberPath`, unwrap `UnaryExpression { NodeType: ExpressionType.Convert }`, then walk `MemberExpression.Expression` until you reach the `ParameterExpression`.

## Notes

- **`Compile()` is expensive** (it emits IL and JIT-compiles it, on the order of tens to hundreds of microseconds). Cache compiled delegates; the payoff is that each call afterwards is as fast as hand-written code.
- **Why not just `Func<T, bool>`?** `IQueryable.Where(Func<...>)` binds to `Enumerable.Where` and pulls every row into memory. Only `Expression<Func<...>>` can be translated to SQL.
- **Closures in expressions.** Capturing a local in an expression lambda produces a `MemberExpression` over a compiler-generated closure object. EF Core turns those into SQL parameters, which is why captured variables don't cause query-plan cache pollution but inlined constants can.
- **Limits.** Expression trees cannot represent statements such as loops or assignments inside an expression lambda (CS0834), nor newer syntax such as `?.`, `is` patterns, or `switch` expressions, although the `Expression` API itself has `Block`, `Loop`, and `Assign` nodes for hand-built trees.
