using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L2.Equality;

[TestClass]
public class EqualityAndComparisonTests
{
    [TestMethod]
    public void Parse_ReadsAllParts()
    {
        var v = SemVer.Parse("2.10.3-rc.1");
        Assert.AreEqual(2, v.Major);
        Assert.AreEqual(10, v.Minor);
        Assert.AreEqual(3, v.Patch);
        Assert.AreEqual("rc.1", v.PreRelease);

        Assert.IsNull(SemVer.Parse("1.0.0").PreRelease);
    }

    [TestMethod]
    [DataRow("1.0")]
    [DataRow("1.0.0.0")]
    [DataRow("a.b.c")]
    [DataRow("1.-1.0")]
    [DataRow("1.0.0-")]
    [DataRow("1.0.0-alpha..1")]
    [DataRow(" 1.0.0")]
    [DataRow("")]
    public void Parse_RejectsInvalidText(string text)
    {
        Assert.ThrowsExactly<FormatException>(() => SemVer.Parse(text));
        Assert.IsFalse(SemVer.TryParse(text, out _));
    }

    [TestMethod]
    public void ToString_RoundTrips()
    {
        Assert.AreEqual("1.2.3", SemVer.Parse("1.2.3").ToString());
        Assert.AreEqual("2.0.0-rc.1", SemVer.Parse("2.0.0-rc.1").ToString());
    }

    [TestMethod]
    public void Equality_IsValueBased()
    {
        var a = SemVer.Parse("1.2.3-beta");
        var b = new SemVer(1, 2, 3, "beta");

        Assert.IsTrue(a.Equals(b));
        Assert.IsTrue(a.Equals((object)b));
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        Assert.IsFalse(a == SemVer.Parse("1.2.3"));
        Assert.IsFalse(a.Equals("1.2.3-beta"));
    }

    [TestMethod]
    public void Equality_WorksAsDictionaryKey()
    {
        var releases = new Dictionary<SemVer, string> { [SemVer.Parse("1.0.0")] = "first" };
        Assert.AreEqual("first", releases[new SemVer(1, 0, 0)]);
    }

    [TestMethod]
    [DataRow("1.0.0", "2.0.0")]
    [DataRow("2.0.0", "2.1.0")]
    [DataRow("2.1.0", "2.1.1")]
    [DataRow("1.0.0-alpha", "1.0.0")]
    [DataRow("1.0.0-alpha", "1.0.0-alpha.1")]
    [DataRow("1.0.0-alpha.1", "1.0.0-alpha.beta")]
    [DataRow("1.0.0-alpha.beta", "1.0.0-beta")]
    [DataRow("1.0.0-beta.2", "1.0.0-beta.11")]
    [DataRow("1.0.0-beta.11", "1.0.0-rc.1")]
    [DataRow("1.9.0", "1.10.0")]
    public void Ordering_FollowsSemVerPrecedence(string lower, string higher)
    {
        var low = SemVer.Parse(lower);
        var high = SemVer.Parse(higher);

        Assert.IsLessThan(0, low.CompareTo(high));
        Assert.IsGreaterThan(0, high.CompareTo(low));
        Assert.IsTrue(low < high);
        Assert.IsTrue(high > low);
        Assert.IsTrue(low <= high);
        Assert.IsTrue(high >= low);
        Assert.IsFalse(low > high);
    }

    [TestMethod]
    public void Ordering_SortsAList()
    {
        var versions = new[] { "1.0.0", "1.0.0-rc.1", "0.9.9", "1.0.0-alpha", "1.10.0", "1.2.0" }
            .Select(SemVer.Parse)
            .ToList();

        versions.Sort();

        CollectionAssert.AreEqual(
            new[] { "0.9.9", "1.0.0-alpha", "1.0.0-rc.1", "1.0.0", "1.2.0", "1.10.0" },
            versions.Select(v => v.ToString()).ToArray());
        Assert.AreEqual("1.10.0", versions.Max().ToString());
    }

    [TestMethod]
    public void SkuComparer_IgnoresCaseAndOtherFields()
    {
        var a = new Product("ABC-1", "Widget", 9.99m);
        var b = new Product("abc-1", "Widget v2", 12.00m);
        var c = new Product("XYZ-9", "Gadget", 1m);

        Assert.IsTrue(SkuComparer.Instance.Equals(a, b));
        Assert.IsFalse(SkuComparer.Instance.Equals(a, c));
        Assert.IsFalse(SkuComparer.Instance.Equals(a, null));
        Assert.IsTrue(SkuComparer.Instance.Equals(null, null));
        Assert.AreEqual(SkuComparer.Instance.GetHashCode(a), SkuComparer.Instance.GetHashCode(b));
    }

    [TestMethod]
    public void SkuComparer_PlugsIntoLinqAndHashSet()
    {
        Product[] products = [new("ABC-1", "Widget", 9.99m), new("abc-1", "Widget v2", 12m), new("XYZ-9", "Gadget", 1m)];

        Assert.AreEqual(2, products.Distinct(SkuComparer.Instance).Count());
        Assert.HasCount(2, new HashSet<Product>(products, SkuComparer.Instance));
        Assert.HasCount(3, new HashSet<Product>(products), "Record equality compares every field.");
    }
}
