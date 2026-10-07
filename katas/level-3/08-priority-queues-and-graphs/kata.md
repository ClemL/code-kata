---
title: Priority Queues & Graphs
level: 3
minutes: 45
summary: PriorityQueue<TElement, TPriority> for top-K and k-way merges, Dijkstra's shortest path, and a deterministic topological sort with cycle detection.
topics: [Algorithms, Graphs, Heaps]
keywords: [while, yield, out, "?"]
apis: ["PriorityQueue<TElement,TPriority>", "Queue<T>", "HashSet<T>", "Dictionary<TKey,TValue>"]
docs:
  - title: PriorityQueue<TElement, TPriority>
    url: https://learn.microsoft.com/dotnet/api/system.collections.generic.priorityqueue-2
  - title: Dijkstra's algorithm
    url: https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm
  - title: Topological sorting (Kahn's algorithm)
    url: https://en.wikipedia.org/wiki/Topological_sorting#Kahn's_algorithm
---

## Brief

.NET 6 shipped a binary-heap `PriorityQueue<TElement, TPriority>` after years of everyone writing their own. It is a min-heap (lowest priority dequeues first), it does not support decrease-key, and it does not guarantee FIFO order among equal priorities. Those three facts shape every algorithm in this kata, from top-K to Dijkstra to build ordering.

## Tasks

Implement the static methods on `Algorithms`:

1. **`TopK(IEnumerable<int> values, int k)`** returns the `k` largest values in descending order, keeping at most `k` items in a heap (O(n log k)). `k = 0` returns an empty list; `k` larger than the input returns everything.
2. **`MergeSorted(IEnumerable<IEnumerable<int>> sources)`** merges any number of ascending sequences into one ascending sequence, lazily. It must work on infinite inputs.
3. **`ShortestPath(graph, from, to)`** implements Dijkstra's algorithm on a weighted directed graph (`IReadOnlyDictionary<string, IReadOnlyList<(string To, int Weight)>>`). Return `(Distance, Path)` including both endpoints, or `null` when `to` is unreachable. Negative weights throw `ArgumentException`.
4. **`BuildOrder(IReadOnlyDictionary<string, string[]> prerequisites)`** returns a topological order where every node comes after its prerequisites. When several nodes are ready at once, take them **alphabetically** (ordinal) so the result is deterministic. Nodes that appear only as prerequisites are included. A cycle throws `InvalidOperationException`.

## Hints

- Top-K: a min-heap keyed by the value itself. After each `Enqueue`, if `Count > k`, `Dequeue()` the smallest. Drain and reverse at the end. `EnqueueDequeue` combines the two steps.
- K-way merge: enqueue each source's enumerator with its current value as the priority. Dequeue, yield, advance that enumerator, re-enqueue if it has more. Dispose enumerators in a `finally`.
- Dijkstra without decrease-key: enqueue duplicates and skip stale entries (`if (distance > best[node]) continue;`). Track `previous[node]` to rebuild the path.
- Kahn's algorithm: compute in-degrees, seed a `PriorityQueue<string, string>` with zero in-degree nodes (priority = name gives alphabetical order), and decrement dependents' in-degrees as you dequeue.
- A cycle exists when the output has fewer nodes than the graph.

## Notes

- **Not stable.** Equal priorities dequeue in an unspecified order. Make ties deterministic by using a tuple priority such as `(distance, name)` or an insertion counter.
- **No decrease-key, no contains.** The "lazy deletion" approach above costs extra heap entries but stays O((V + E) log V) and is how most production implementations use binary heaps.
- **`TryDequeue(out element, out priority)`** returns both, which saves a second lookup in Dijkstra.
- **Real build graphs.** MSBuild, NuGet restore, and EF Core migrations all perform a topological sort; cycle errors in those tools come from exactly this check.
