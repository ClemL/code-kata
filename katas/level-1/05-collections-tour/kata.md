---
title: Collections Tour
level: 1
minutes: 30
summary: Pick the right BCL collection - Dictionary, HashSet, Stack, Queue, SortedDictionary - and build them with C# 12 collection expressions.
topics: [Collections, Collection expressions, Algorithms]
keywords: [new, foreach, return, "[..]"]
apis: ["Dictionary<TKey,TValue>", "HashSet<T>", "Stack<T>", "Queue<T>", "SortedDictionary<TKey,TValue>", StringComparer.OrdinalIgnoreCase]
docs:
  - title: Collection expressions
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions
  - title: Selecting a collection class
    url: https://learn.microsoft.com/dotnet/standard/collections/selecting-a-collection-class
  - title: Commonly used collection types
    url: https://learn.microsoft.com/dotnet/standard/collections/commonly-used-collection-types
---

## Brief

Most performance problems in line-of-business code are collection problems: a `List.Contains` inside a loop, a dictionary with the wrong comparer, a queue implemented with `RemoveAt(0)`. This kata is a quick tour of the workhorse collections and the C# 12 collection expression syntax (`[a, b, .. rest]`).

## Tasks

Implement the static methods on `CollectionKata`:

1. **`WordFrequency(string text)`** returns a `Dictionary<string, int>` counting words. Words are separated by spaces and the punctuation `, . ! ? ; :`. Keys are **case-insensitive**: `result["THE"]` and `result["the"]` return the same count.
2. **`FirstDuplicate(IEnumerable<int> values)`** returns the first value that appears for a second time, or `null`. Single pass.
3. **`IsBalanced(string input)`** returns `true` when every `(`, `[`, and `{` is closed in the correct order. Ignore all other characters.
4. **`Frame(IEnumerable<int> middle, int edge)`** returns an `int[]` with `edge` at both ends and `middle` in between, written as a collection expression.
5. **`EliminationOrder(int count, int step)`** solves the Josephus problem with a `Queue<int>`: people `1..count` stand in a circle, and every `step`-th person is removed. Return the order of removal. For `(7, 3)` the answer is `3, 6, 2, 7, 5, 1, 4`.
6. **`GroupByLength(IEnumerable<string> words)`** returns a `SortedDictionary<int, List<string>>` mapping word length to the words of that length, in input order.

## Hints

- Pass `StringComparer.OrdinalIgnoreCase` to the `Dictionary` constructor. The comparer, not the key you store, decides case sensitivity.
- `counts[word] = counts.GetValueOrDefault(word) + 1;` is a compact upsert.
- `HashSet<T>.Add` returns `false` when the item is already present.
- For brackets, push the expected *closing* character, then compare on pop.
- Collection expressions spread any enumerable: `[edge, .. middle, edge]`.
- Josephus: dequeue and re-enqueue `step - 1` people, then dequeue the next one for good.

## Notes

- **Big-O cheat sheet.** `List<T>.Contains` is O(n); `HashSet<T>.Contains` and `Dictionary` lookups are O(1) on average; `SortedDictionary` is O(log n) and keeps keys ordered; `Queue`/`Stack` operations are O(1).
- **Comparers are part of the type's behavior.** A `Dictionary<string, T>` built with the default comparer is case-sensitive and ordinal. Culture-sensitive comparers (`StringComparer.CurrentCulture`) are rarely what you want for keys.
- **Collection expressions are target-typed.** The same `[1, 2, 3]` can become an array, `List<T>`, `Span<T>`, `ImmutableArray<T>`, or an interface such as `IReadOnlyList<T>`. The compiler picks an efficient construction for each.
- **`PriorityQueue<TElement, TPriority>`** (.NET 6) is the missing piece from this list; it gets its own kata in Level 3.
