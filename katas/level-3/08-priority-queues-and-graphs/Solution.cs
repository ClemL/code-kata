namespace Katas.L3.Graphs;

public static class Algorithms
{
    public static IReadOnlyList<int> TopK(IEnumerable<int> values, int k)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(k);
        if (k == 0)
        {
            return [];
        }

        var heap = new PriorityQueue<int, int>(k + 1);
        foreach (var value in values)
        {
            if (heap.Count < k)
            {
                heap.Enqueue(value, value);
            }
            else if (value > heap.Peek())
            {
                heap.EnqueueDequeue(value, value);
            }
        }

        var result = new int[heap.Count];
        for (var i = result.Length - 1; i >= 0; i--)
        {
            result[i] = heap.Dequeue();
        }

        return result;
    }

    public static IEnumerable<int> MergeSorted(IEnumerable<IEnumerable<int>> sources)
    {
        var heap = new PriorityQueue<IEnumerator<int>, int>();
        var all = new List<IEnumerator<int>>();

        try
        {
            foreach (var source in sources)
            {
                var e = source.GetEnumerator();
                all.Add(e);
                if (e.MoveNext())
                {
                    heap.Enqueue(e, e.Current);
                }
            }

            while (heap.TryDequeue(out var e, out var value))
            {
                yield return value;
                if (e.MoveNext())
                {
                    heap.Enqueue(e, e.Current);
                }
            }
        }
        finally
        {
            foreach (var e in all)
            {
                e.Dispose();
            }
        }
    }

    public static (int Distance, IReadOnlyList<string> Path)? ShortestPath(
        IReadOnlyDictionary<string, IReadOnlyList<(string To, int Weight)>> graph,
        string from,
        string to)
    {
        if (graph.Values.SelectMany(edges => edges).Any(edge => edge.Weight < 0))
        {
            throw new ArgumentException("Dijkstra's algorithm requires non-negative weights.", nameof(graph));
        }

        var best = new Dictionary<string, int> { [from] = 0 };
        var previous = new Dictionary<string, string>();
        var frontier = new PriorityQueue<string, int>();
        frontier.Enqueue(from, 0);

        while (frontier.TryDequeue(out var node, out var distance))
        {
            if (distance > best[node])
            {
                continue; // Stale entry: a shorter route was already found.
            }

            if (node == to)
            {
                var path = new List<string> { to };
                while (previous.TryGetValue(path[^1], out var step))
                {
                    path.Add(step);
                }

                path.Reverse();
                return (distance, path);
            }

            if (!graph.TryGetValue(node, out var edges))
            {
                continue;
            }

            foreach (var (next, weight) in edges)
            {
                var candidate = distance + weight;
                if (!best.TryGetValue(next, out var known) || candidate < known)
                {
                    best[next] = candidate;
                    previous[next] = node;
                    frontier.Enqueue(next, candidate);
                }
            }
        }

        return null;
    }

    public static IReadOnlyList<string> BuildOrder(IReadOnlyDictionary<string, string[]> prerequisites)
    {
        var inDegree = new Dictionary<string, int>(StringComparer.Ordinal);
        var dependents = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var (node, requires) in prerequisites)
        {
            inDegree.TryAdd(node, 0);
            foreach (var prerequisite in requires)
            {
                inDegree.TryAdd(prerequisite, 0);
                inDegree[node]++;

                if (!dependents.TryGetValue(prerequisite, out var list))
                {
                    dependents[prerequisite] = list = [];
                }

                list.Add(node);
            }
        }

        var ready = new PriorityQueue<string, string>(StringComparer.Ordinal);
        foreach (var (node, degree) in inDegree)
        {
            if (degree == 0)
            {
                ready.Enqueue(node, node);
            }
        }

        var order = new List<string>(inDegree.Count);
        while (ready.TryDequeue(out var node, out _))
        {
            order.Add(node);
            foreach (var dependent in dependents.GetValueOrDefault(node) ?? [])
            {
                if (--inDegree[dependent] == 0)
                {
                    ready.Enqueue(dependent, dependent);
                }
            }
        }

        if (order.Count != inDegree.Count)
        {
            throw new InvalidOperationException("The dependency graph contains a cycle.");
        }

        return order;
    }
}
