---
title: Option<T> & the LINQ Query Pattern
level: 5
minutes: 40
summary: Build an Option<T> type with Map, Bind and Match, then implement Select, SelectMany and Where so it works with LINQ query syntax.
topics: [Functional programming, LINQ, Monads, API design]
keywords: [from, select, where, readonly struct, "IEquatable<T>"]
apis: [SelectMany, "Func<T,TResult>", "IEquatable<T>"]
docs:
  - title: Language Integrated Query (LINQ) overview
    url: https://learn.microsoft.com/dotnet/csharp/linq/
  - title: C# language specification - expressions (see "Query expressions")
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/language-specification/expressions
---

## Brief

LINQ query syntax is not tied to `IEnumerable<T>`. The compiler rewrites `from x in a from y in b where p select r` into calls named `SelectMany`, `Where`, and `Select`, and any type that has methods with those names and compatible shapes can be queried. This kata builds an `Option<T>` (a value that is either `Some(value)` or `None`) and makes it queryable, which turns chains of "try this, then that" into a flat, readable query.

## Tasks

1. **`Option<T>`** is a `readonly struct` with:
   - `Option<T>.None` and `Option.Some(value)` (a `null` value passed to `Some` throws `ArgumentNullException`);
   - `IsSome`;
   - `Match(some, none)` to extract a result;
   - `Map(f)` to transform a present value;
   - `Bind(f)` to chain an operation that itself returns an `Option`;
   - `GetValueOrDefault(fallback)`;
   - value equality (`IEquatable<Option<T>>`, `==`, `!=`) and `ToString()` returning `"Some(5)"` or `"None"`.
2. **Query pattern.** Implement `Select`, `SelectMany` (the two-function overload the compiler needs), and `Where`, so that this compiles and works:
   ```csharp
   from host in config.Find("Host")
   from rawPort in config.Find("Port")
   from port in Parse.Int(rawPort)
   where port is > 0 and < 65536
   select $"{host}:{port}"
   ```
3. **`Parse.Int(string? text)`** returns `Some` for a valid invariant-culture integer, otherwise `None`.
4. **`OptionExtensions.Find(dictionary, key)`** looks up a key in an `IReadOnlyDictionary<TKey, TValue>` and returns `Some` or `None`.
5. **`Endpoints.Build(IReadOnlyDictionary<string, string> config)`** returns `"host:port"` using the query above, or `None` if anything is missing or invalid.

## Hints

- Store `_value` and `_hasValue` fields; `default(Option<T>)` is then `None` automatically.
- `Match<TResult>(Func<T, TResult> some, Func<TResult> none) => _hasValue ? some(_value) : none();` Everything else can be written with `Match`.
- `Bind` has signature `Option<TResult> Bind<TResult>(Func<T, Option<TResult>> binder)`.
- The compiler translates `from a in x from b in f(a) select g(a, b)` into `x.SelectMany(a => f(a), (a, b) => g(a, b))`, so you need `SelectMany<TMiddle, TResult>(Func<T, Option<TMiddle>> binder, Func<T, TMiddle, TResult> projector)`.
- `Where` keeps the value only when the predicate is true.
- `Select` is just `Map` with the name LINQ expects.

## Notes

- **Duck typing again.** Query syntax requires methods, not interfaces, so the methods can be instance methods or extension methods. Parsers (Sprache, Pidgin), reactive streams (`IObservable<T>` in Rx.NET), and `Task` (in some libraries) all support LINQ syntax this way.
- **Nullable reference types vs `Option<T>`.** NRT annotations give compile-time warnings with zero runtime cost and are the idiomatic C# answer for "may be missing". `Option<T>` is worth it when absence must be handled explicitly or composed, as in parsing pipelines.
- **The monad laws** (left identity, right identity, associativity of `Bind`) are what make chained queries behave predictably. The tests check two of them.
- **`Result<T, TError>`** is the natural next step: like `Option<T>`, but `None` carries an error. Several libraries (`OneOf`, `ErrorOr`, `FluentResults`) provide one.
