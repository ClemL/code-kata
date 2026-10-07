namespace Katas.L3.Graphs;

public static class Algorithms
{
    public static IReadOnlyList<int> TopK(IEnumerable<int> values, int k) => throw new NotImplementedException();

    public static IEnumerable<int> MergeSorted(IEnumerable<IEnumerable<int>> sources) => throw new NotImplementedException();

    public static (int Distance, IReadOnlyList<string> Path)? ShortestPath(
        IReadOnlyDictionary<string, IReadOnlyList<(string To, int Weight)>> graph,
        string from,
        string to) =>
        throw new NotImplementedException();

    public static IReadOnlyList<string> BuildOrder(IReadOnlyDictionary<string, string[]> prerequisites) =>
        throw new NotImplementedException();
}
