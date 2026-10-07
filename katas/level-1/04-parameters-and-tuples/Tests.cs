using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L1.Parameters;

[TestClass]
public class ParametersAndTuplesTests
{
    [TestMethod]
    public void Swap_SwapsValueTypes()
    {
        int a = 1, b = 2;
        Args.Swap(ref a, ref b);
        Assert.AreEqual(2, a);
        Assert.AreEqual(1, b);
    }

    [TestMethod]
    public void Swap_SwapsReferences()
    {
        string left = "left", right = "right";
        Args.Swap(ref left, ref right);
        Assert.AreEqual("right", left);
        Assert.AreEqual("left", right);
    }

    [TestMethod]
    public void TryDivide_ReturnsQuotientAndRemainder()
    {
        Assert.IsTrue(Args.TryDivide(17, 5, out var q, out var r));
        Assert.AreEqual(3, q);
        Assert.AreEqual(2, r);

        Assert.IsTrue(Args.TryDivide(-17, 5, out q, out r));
        Assert.AreEqual(-3, q);
        Assert.AreEqual(-2, r, "C# remainder takes the sign of the dividend.");
    }

    [TestMethod]
    public void TryDivide_RejectsZeroDivisor()
    {
        Assert.IsFalse(Args.TryDivide(10, 0, out var q, out var r));
        Assert.AreEqual(0, q);
        Assert.AreEqual(0, r);
    }

    [TestMethod]
    public void Sum_AcceptsAnyNumberOfArguments()
    {
        int[] numbers = [10, 20, 30];

        Assert.AreEqual(0, Args.Sum());
        Assert.AreEqual(5, Args.Sum(5));
        Assert.AreEqual(6, Args.Sum(1, 2, 3));
        Assert.AreEqual(60, Args.Sum([.. numbers]));
    }

    [TestMethod]
    public void MinMax_ReturnsNamedTuple()
    {
        var result = Args.MinMax(4, -2, 9, 0);
        Assert.AreEqual(-2, result.Min);
        Assert.AreEqual(9, result.Max);

        var (min, max) = Args.MinMax(7);
        Assert.AreEqual(7, min);
        Assert.AreEqual(7, max);
    }

    [TestMethod]
    public void MinMax_RejectsEmptyInput() =>
        Assert.ThrowsExactly<ArgumentException>(() => Args.MinMax());

    [TestMethod]
    public void Greet_SupportsOptionalAndNamedArguments()
    {
        Assert.AreEqual("Hello, Ada!", Args.Greet("Ada"));
        Assert.AreEqual("Hello, Ada! (36)", Args.Greet("Ada", 36));
        Assert.AreEqual("Hi, Grace!", Args.Greet("Grace", greeting: "Hi"));
        Assert.AreEqual("Welcome, Linus! (56)", Args.Greet(greeting: "Welcome", age: 56, name: "Linus"));
    }

    [TestMethod]
    public void Distance_UsesAllThreeAxes()
    {
        var origin = new Point3(0, 0, 0);
        var point = new Point3(1, 2, 2);

        Assert.AreEqual(3.0, Args.Distance(origin, point), 1e-12);
        Assert.AreEqual(3.0, Args.Distance(in point, in origin), 1e-12);
        Assert.AreEqual(0.0, Args.Distance(point, point), 1e-12);
    }
}
