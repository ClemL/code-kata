using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L3.Graphs;

[TestClass]
public class PriorityQueuesAndGraphsTests
{
    private static readonly Dictionary<string, IReadOnlyList<(string To, int Weight)>> Graph = new()
    {
        ["A"] = [("B", 4), ("C", 2)],
        ["C"] = [("B", 1), ("D", 8)],
        ["B"] = [("D", 5)],
        ["D"] = [("E", 3)],
        ["F"] = [],
    };

    [TestMethod]
    public void TopK_ReturnsLargestDescending()
    {
        CollectionAssert.AreEqual(new[] { 9, 9, 7 }, Algorithms.TopK([5, 1, 9, 3, 7, 9, 2], 3).ToArray());
        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, Algorithms.TopK([1, 2, 3], 10).ToArray());
        Assert.IsEmpty(Algorithms.TopK([1, 2, 3], 0));
        Assert.IsEmpty(Algorithms.TopK([], 3));
    }

    [TestMethod]
    public void TopK_HandlesLargeInput()
    {
        var values = Enumerable.Range(0, 100_000).Select(i => (i * 7919) % 100_003);
        var top = Algorithms.TopK(values, 5);
        CollectionAssert.AreEqual(values.Order().TakeLast(5).Reverse().ToArray(), top.ToArray());
    }

    [TestMethod]
    public void MergeSorted_MergesFiniteSequences() =>
        CollectionAssert.AreEqual(
            Enumerable.Range(0, 10).ToArray(),
            Algorithms.MergeSorted([[1, 4, 7], [2, 5, 8], [0, 3, 6, 9], []]).ToArray());

    [TestMethod]
    public void MergeSorted_IsLazyOnInfiniteInputs()
    {
        static IEnumerable<int> Step(int start)
        {
            for (var i = start; ; i += 2)
            {
                yield return i;
            }
        }

        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 4, 5 }, Algorithms.MergeSorted([Step(0), Step(1)]).Take(6).ToArray());
    }

    [TestMethod]
    public void MergeSorted_KeepsDuplicates() =>
        CollectionAssert.AreEqual(new[] { 1, 1, 2, 2 }, Algorithms.MergeSorted([[1, 2], [1, 2]]).ToArray());

    [TestMethod]
    public void ShortestPath_FindsCheapestRoute()
    {
        var result = Algorithms.ShortestPath(Graph, "A", "D");

        Assert.IsNotNull(result);
        Assert.AreEqual(8, result.Value.Distance);
        CollectionAssert.AreEqual(new[] { "A", "C", "B", "D" }, result.Value.Path.ToArray());
    }

    [TestMethod]
    public void ShortestPath_HandlesLongerAndTrivialRoutes()
    {
        var toE = Algorithms.ShortestPath(Graph, "A", "E");
        Assert.AreEqual(11, toE?.Distance);
        CollectionAssert.AreEqual(new[] { "A", "C", "B", "D", "E" }, toE?.Path.ToArray());

        var self = Algorithms.ShortestPath(Graph, "A", "A");
        Assert.AreEqual(0, self?.Distance);
        CollectionAssert.AreEqual(new[] { "A" }, self?.Path.ToArray());
    }

    [TestMethod]
    public void ShortestPath_ReturnsNullWhenUnreachable()
    {
        Assert.IsNull(Algorithms.ShortestPath(Graph, "A", "F"));
        Assert.IsNull(Algorithms.ShortestPath(Graph, "E", "A"));
    }

    [TestMethod]
    public void ShortestPath_RejectsNegativeWeights()
    {
        var bad = new Dictionary<string, IReadOnlyList<(string To, int Weight)>> { ["A"] = [("B", -1)] };
        Assert.ThrowsExactly<ArgumentException>(() => Algorithms.ShortestPath(bad, "A", "B"));
    }

    [TestMethod]
    public void BuildOrder_RespectsPrerequisitesAndIsDeterministic()
    {
        var order = Algorithms.BuildOrder(new Dictionary<string, string[]>
        {
            ["app"] = ["core", "data"],
            ["data"] = ["core"],
            ["core"] = [],
            ["tests"] = ["app"],
            ["docs"] = [],
        });

        CollectionAssert.AreEqual(new[] { "core", "data", "app", "docs", "tests" }, order.ToArray());
    }

    [TestMethod]
    public void BuildOrder_IncludesNodesOnlyListedAsPrerequisites()
    {
        var order = Algorithms.BuildOrder(new Dictionary<string, string[]> { ["api"] = ["logging", "auth"] });
        CollectionAssert.AreEqual(new[] { "auth", "logging", "api" }, order.ToArray());
    }

    [TestMethod]
    public void BuildOrder_DetectsCycles() =>
        Assert.ThrowsExactly<InvalidOperationException>(() => Algorithms.BuildOrder(new Dictionary<string, string[]>
        {
            ["a"] = ["b"],
            ["b"] = ["c"],
            ["c"] = ["a"],
            ["d"] = [],
        }));
}
