using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L2.Extensions;

[TestClass]
public class ExtensionMethodsTests
{
    [TestMethod]
    public void Truncate_ShortensLongStrings()
    {
        Assert.AreEqual("Hello, w...", "Hello, world!".Truncate(11));
        Assert.AreEqual("Hello, wo…", "Hello, world!".Truncate(10, "…"));
        Assert.AreEqual("short", "short".Truncate(10));
        Assert.AreEqual("exact", "exact".Truncate(5));
        Assert.AreEqual("...", "anything".Truncate(3));
    }

    [TestMethod]
    public void Truncate_RejectsTooSmallMaxLength() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => "anything".Truncate(2));

    [TestMethod]
    public void OrEmpty_WorksOnNullReceiver()
    {
        string? missing = null;
        Assert.AreEqual("", missing.OrEmpty());
        Assert.AreEqual("value", "value".OrEmpty());
    }

    [TestMethod]
    public void WhereNotNull_FiltersNulls()
    {
        string?[] values = ["a", null, "b", null];
        IEnumerable<string> notNull = values.WhereNotNull();
        CollectionAssert.AreEqual(new[] { "a", "b" }, notNull.ToArray());
    }

    [TestMethod]
    public void WhereNotNull_IsLazy()
    {
        static IEnumerable<string?> Explodes()
        {
            yield return "first";
            throw new InvalidOperationException("Enumerated too far.");
        }

        var query = Explodes().WhereNotNull();
        Assert.AreEqual("first", query.First());
    }

    [TestMethod]
    public void WithIndex_PairsItemsWithPositions()
    {
        var pairs = new[] { "zero", "one", "two" }.WithIndex().ToList();

        Assert.HasCount(3, pairs);
        Assert.AreEqual((0, "zero"), pairs[0]);
        Assert.AreEqual(2, pairs[2].Index);
        Assert.AreEqual("two", pairs[2].Item);
    }

    [TestMethod]
    public void GetOrAdd_RunsFactoryOnlyOnMiss()
    {
        var cache = new Dictionary<string, int>();
        var calls = 0;

        Assert.AreEqual(5, cache.GetOrAdd("hello", key => { calls++; return key.Length; }));
        Assert.AreEqual(5, cache.GetOrAdd("hello", key => { calls++; return -1; }));
        Assert.AreEqual(1, calls);
        Assert.AreEqual(5, cache["hello"]);
    }

    [TestMethod]
    public void IsBetween_IsInclusiveAndOrderInsensitive()
    {
        var start = new DateOnly(2026, 1, 1);
        var end = new DateOnly(2026, 3, 31);

        Assert.IsTrue(new DateOnly(2026, 2, 14).IsBetween(start, end));
        Assert.IsTrue(start.IsBetween(start, end));
        Assert.IsTrue(end.IsBetween(start, end));
        Assert.IsTrue(new DateOnly(2026, 2, 14).IsBetween(end, start));
        Assert.IsFalse(new DateOnly(2026, 4, 1).IsBetween(start, end));
    }
}
