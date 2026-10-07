---
title: Delegates, Lambdas & Events
level: 3
minutes: 30
summary: Function composition, memoization with closures, the for-loop capture trap, multicast delegates, and the standard event pattern.
topics: [Delegates, Lambdas, Closures, Events]
keywords: [delegate, event, "=>", "+=", "-=", static]
apis: ["Func<T,TResult>", "Action<T>", "EventHandler<TEventArgs>", Delegate.GetInvocationList]
docs:
  - title: Lambda expressions
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions
  - title: Delegates
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/delegates/
  - title: Events
    url: https://learn.microsoft.com/dotnet/standard/events/
---

## Brief

Delegates are typed function references; lambdas are the syntax for creating them; closures are what happens when a lambda captures a local variable; events are delegates with restricted access (outsiders can only subscribe and unsubscribe). Most of LINQ, ASP.NET Core middleware, and every UI framework sits on these four ideas.

## Tasks

1. **`Functional.Compose(f, g)`** returns a function equivalent to `x => g(f(x))`.
2. **`Functional.Memoize(f)`** returns a function that calls `f` at most once per distinct argument and caches results.
3. **`Functional.MakeCounter()`** returns a `Func<int>` that yields `1, 2, 3, ...` on successive calls. Each counter is independent.
4. **`Functional.CreateMultipliers(int count)`** returns `count` functions where function `i` multiplies its input by `i`. Use a `for` loop and make sure each lambda sees its own `i`.
5. **`Functional.Pipeline(params Action<List<string>>[] steps)`** combines steps into one multicast `Action<List<string>>` that runs them in order.
6. **`StockTicker`** raises `PriceChanged` (an `EventHandler<PriceChangedEventArgs>`) from `Update(symbol, price)`:
   - the first price for a symbol always raises the event (`OldPrice` is `null`);
   - later prices raise it only when they differ from the **last reported** price by at least `thresholdPercent`;
   - unreported prices do not move the baseline.

## Hints

- `return x => g(f(x));` The compiler infers the lambda's delegate type from the return type.
- Memoize: create a `Dictionary<TIn, TOut>` *outside* the returned lambda so every call shares it.
- The counter's state lives in a captured local: `var count = 0; return () => ++count;`
- In a `for` loop, `i` is a single variable shared across iterations. Copy it: `var factor = i;`. (`foreach` creates a fresh variable per iteration since C# 5.)
- `Action<List<string>>? combined = null; foreach (var step in steps) combined += step;`
- Raise events with `PriceChanged?.Invoke(this, args)`; the null-conditional call is thread-safe because it reads the field once.

## Notes

- **Closures allocate.** A capturing lambda compiles to a hidden class holding the captured variables. On hot paths, `static` lambdas (C# 9) forbid capture and guarantee no closure allocation.
- **Multicast return values.** When a multicast delegate returns a value, only the last invocation's result is returned. That is why events use `void` delegates.
- **Event memory leaks.** A publisher holds strong references to its subscribers. Long-lived publishers (static events, singletons) keep short-lived subscribers alive until they unsubscribe.
- **`event` vs public delegate field.** Without `event`, any caller could invoke the delegate or overwrite it with `=`. The `event` keyword limits outsiders to `+=` and `-=`.
