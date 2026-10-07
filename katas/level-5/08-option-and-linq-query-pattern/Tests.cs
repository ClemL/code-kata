using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.Monads;

[TestClass]
public class OptionAndLinqTests
{
    [TestMethod]
    public void SomeAndNone_BehaveAsValues()
    {
        Assert.IsTrue(Option.Some(5).IsSome);
        Assert.IsFalse(Option<int>.None.IsSome);
        Assert.IsFalse(default(Option<string>).IsSome, "default should be None.");

        Assert.IsTrue(Option.Some(5) == Option.Some(5));
        Assert.IsTrue(Option.Some(5) != Option.Some(6));
        Assert.IsTrue(Option<int>.None == default);
        Assert.AreEqual(Option.Some("a").GetHashCode(), Option.Some("a").GetHashCode());

        Assert.AreEqual("Some(5)", Option.Some(5).ToString());
        Assert.AreEqual("None", Option<int>.None.ToString());
    }

    [TestMethod]
    public void Some_RejectsNull() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => Option.Some<string>(null!));

    [TestMethod]
    public void Match_Map_Bind_GetValueOrDefault()
    {
        Assert.AreEqual("got 5", Option.Some(5).Match(v => $"got {v}", () => "nothing"));
        Assert.AreEqual("nothing", Option<int>.None.Match(v => $"got {v}", () => "nothing"));

        Assert.AreEqual(Option.Some(10), Option.Some(5).Map(v => v * 2));
        Assert.AreEqual(Option<int>.None, Option<int>.None.Map(v => v * 2));

        Assert.AreEqual(Option.Some(42), Option.Some("42").Bind(Parse.Int));
        Assert.AreEqual(Option<int>.None, Option.Some("forty-two").Bind(Parse.Int));

        Assert.AreEqual(7, Option<int>.None.GetValueOrDefault(7));
        Assert.AreEqual(3, Option.Some(3).GetValueOrDefault(7));
    }

    [TestMethod]
    public void Map_DoesNotRunOnNone()
    {
        var ran = false;
        Option<int>.None.Map(v => { ran = true; return v; });
        Assert.IsFalse(ran);
    }

    [TestMethod]
    public void Bind_ObeysMonadIdentityLaws()
    {
        Func<string, Option<int>> f = Parse.Int;

        Assert.AreEqual(f("12"), Option.Some("12").Bind(f), "Left identity: Some(x).Bind(f) == f(x).");

        var m = Option.Some(12);
        Assert.AreEqual(m, m.Bind(Option.Some), "Right identity: m.Bind(Some) == m.");
    }

    [TestMethod]
    public void QuerySyntax_ComposesOptions()
    {
        var product =
            from a in Parse.Int("6")
            from b in Parse.Int("7")
            select a * b;
        Assert.AreEqual(Option.Some(42), product);

        var failed =
            from a in Parse.Int("6")
            from b in Parse.Int("seven")
            select a * b;
        Assert.AreEqual(Option<int>.None, failed);

        var filtered =
            from a in Parse.Int("-3")
            where a > 0
            select a;
        Assert.AreEqual(Option<int>.None, filtered);
    }

    [TestMethod]
    public void Find_LooksUpKeys()
    {
        IReadOnlyDictionary<string, string> config = new Dictionary<string, string> { ["Host"] = "db.local" };

        Assert.AreEqual(Option.Some("db.local"), config.Find("Host"));
        Assert.AreEqual(Option<string>.None, config.Find("Port"));
    }

    [TestMethod]
    public void Endpoints_BuildsOnlyWhenEverythingIsValid()
    {
        Assert.AreEqual(Option.Some("db.local:5432"), Endpoints.Build(new Dictionary<string, string> { ["Host"] = "db.local", ["Port"] = "5432" }));
        Assert.AreEqual(Option<string>.None, Endpoints.Build(new Dictionary<string, string> { ["Host"] = "db.local" }));
        Assert.AreEqual(Option<string>.None, Endpoints.Build(new Dictionary<string, string> { ["Host"] = "db.local", ["Port"] = "abc" }));
        Assert.AreEqual(Option<string>.None, Endpoints.Build(new Dictionary<string, string> { ["Host"] = "db.local", ["Port"] = "70000" }));
        Assert.AreEqual(Option<string>.None, Endpoints.Build(new Dictionary<string, string> { ["Port"] = "5432" }));
    }
}
