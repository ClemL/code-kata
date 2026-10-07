using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L1.Collections;

[TestClass]
public class CollectionsTourTests
{
    [TestMethod]
    public void WordFrequency_CountsCaseInsensitively()
    {
        var counts = CollectionKata.WordFrequency("The cat; the HAT. The bat!");

        Assert.AreEqual(3, counts["the"]);
        Assert.AreEqual(3, counts["THE"]);
        Assert.AreEqual(1, counts["cat"]);
        Assert.AreEqual(1, counts["hat"]);
        Assert.HasCount(4, counts);
    }

    [TestMethod]
    public void WordFrequency_HandlesEmptyInput() =>
        Assert.IsEmpty(CollectionKata.WordFrequency("  ,. "));

    [TestMethod]
    public void FirstDuplicate_FindsEarliestRepeat()
    {
        Assert.AreEqual(3, CollectionKata.FirstDuplicate([2, 3, 4, 3, 2]));
        Assert.AreEqual(1, CollectionKata.FirstDuplicate([1, 1]));
        Assert.IsNull(CollectionKata.FirstDuplicate([1, 2, 3]));
        Assert.IsNull(CollectionKata.FirstDuplicate([]));
    }

    [TestMethod]
    public void FirstDuplicate_IsSinglePass()
    {
        static IEnumerable<int> OnlyOnce()
        {
            yield return 5;
            yield return 5;
            throw new InvalidOperationException("Enumerated past the first duplicate.");
        }

        Assert.AreEqual(5, CollectionKata.FirstDuplicate(OnlyOnce()));
    }

    [TestMethod]
    [DataRow("", true)]
    [DataRow("([]{})", true)]
    [DataRow("fn(a[0], { b: c })", true)]
    [DataRow("([)]", false)]
    [DataRow("((", false)]
    [DataRow("())", false)]
    [DataRow("}", false)]
    public void IsBalanced_ChecksNesting(string input, bool expected) =>
        Assert.AreEqual(expected, CollectionKata.IsBalanced(input));

    [TestMethod]
    public void Frame_WrapsTheMiddle()
    {
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3, 0 }, CollectionKata.Frame([1, 2, 3], 0));
        CollectionAssert.AreEqual(new[] { 9, 9 }, CollectionKata.Frame([], 9));
    }

    [TestMethod]
    public void EliminationOrder_SolvesJosephus()
    {
        CollectionAssert.AreEqual(new[] { 3, 6, 2, 7, 5, 1, 4 }, CollectionKata.EliminationOrder(7, 3));
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, CollectionKata.EliminationOrder(3, 1));
        Assert.IsEmpty(CollectionKata.EliminationOrder(0, 2));
    }

    [TestMethod]
    public void GroupByLength_IsSortedByKeyAndKeepsInputOrder()
    {
        var groups = CollectionKata.GroupByLength(["kiwi", "fig", "plum", "apple", "pea"]);

        CollectionAssert.AreEqual(new[] { 3, 4, 5 }, groups.Keys.ToArray());
        CollectionAssert.AreEqual(new[] { "fig", "pea" }, groups[3]);
        CollectionAssert.AreEqual(new[] { "kiwi", "plum" }, groups[4]);
        CollectionAssert.AreEqual(new[] { "apple" }, groups[5]);
    }
}
