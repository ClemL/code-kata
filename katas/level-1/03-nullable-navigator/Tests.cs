using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L1.Nullables;

[TestClass]
public class NullableNavigatorTests
{
    private static readonly Customer NoAddress = new("Ada", null, null);
    private static readonly Customer NoCity = new("Grace", new Address("1 Main St", null, "02139"), "grace@example.com");
    private static readonly Customer Full = new("Linus", new Address("2 Elm St", "Boston", null), null);

    [TestMethod]
    public void CityOrUnknown_ShortCircuitsTheChain()
    {
        Assert.AreEqual("Unknown", NullSafety.CityOrUnknown(null));
        Assert.AreEqual("Unknown", NullSafety.CityOrUnknown(NoAddress));
        Assert.AreEqual("Unknown", NullSafety.CityOrUnknown(NoCity));
        Assert.AreEqual("Boston", NullSafety.CityOrUnknown(Full));
    }

    [TestMethod]
    public void NormalizeOrNull_TrimsOrReturnsNull()
    {
        Assert.IsNull(NullSafety.NormalizeOrNull(null));
        Assert.IsNull(NullSafety.NormalizeOrNull(""));
        Assert.IsNull(NullSafety.NormalizeOrNull(" \t\n "));
        Assert.AreEqual("hello", NullSafety.NormalizeOrNull("  hello  "));
    }

    [TestMethod]
    public void TryGetPostalCode_ReportsPresence()
    {
        Assert.IsTrue(NullSafety.TryGetPostalCode(NoCity, out var code));
        Assert.AreEqual(5, code.Length); // No warning: [NotNullWhen(true)] proves code is not null here.

        Assert.IsFalse(NullSafety.TryGetPostalCode(Full, out var missing));
        Assert.IsNull(missing);

        Assert.IsFalse(NullSafety.TryGetPostalCode(null, out _));
    }

    [TestMethod]
    public void Require_ReturnsValidValue() =>
        Assert.AreEqual("secret", NullSafety.Require("secret"));

    [TestMethod]
    public void Require_ReportsCallerExpressionForNull()
    {
        string? apiKey = null;
        var ex = Assert.ThrowsExactly<ArgumentNullException>(() => NullSafety.Require(apiKey));
        Assert.AreEqual("apiKey", ex.ParamName);
    }

    [TestMethod]
    public void Require_ReportsCallerExpressionForWhitespace()
    {
        var settings = new { Region = "   " };
        var ex = Assert.ThrowsExactly<ArgumentException>(() => NullSafety.Require(settings.Region));
        Assert.AreEqual("settings.Region", ex.ParamName);
    }

    [TestMethod]
    public void SumIgnoringNulls_SkipsNulls()
    {
        Assert.AreEqual(6, NullSafety.SumIgnoringNulls([1, null, 2, null, 3]));
        Assert.AreEqual(0, NullSafety.SumIgnoringNulls([]));
        Assert.AreEqual(0, NullSafety.SumIgnoringNulls([null, null]));
    }

    [TestMethod]
    public void SumStrict_PropagatesNull()
    {
        Assert.AreEqual(6, NullSafety.SumStrict([1, 2, 3]));
        Assert.IsNull(NullSafety.SumStrict([1, null, 3]));
        Assert.AreEqual(0, NullSafety.SumStrict([]));
    }

    [TestMethod]
    public void ConfigCache_LoadsOnce()
    {
        var cache = new ConfigCache(() => "Server=.;Database=Katas");

        Assert.AreEqual(0, cache.LoadCount);
        Assert.AreEqual("Server=.;Database=Katas", cache.ConnectionString);
        Assert.AreEqual("Server=.;Database=Katas", cache.ConnectionString);
        Assert.AreEqual(1, cache.LoadCount);
    }
}
