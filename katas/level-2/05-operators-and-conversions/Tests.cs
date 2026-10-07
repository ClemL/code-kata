using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L2.Operators;

[TestClass]
public class OperatorsAndConversionsTests
{
    [TestMethod]
    public void Constructor_NormalizesSignAndTerms()
    {
        var f = new Fraction(2, -4);
        Assert.AreEqual(-1, f.Numerator);
        Assert.AreEqual(2, f.Denominator);

        var zero = new Fraction(0, -7);
        Assert.AreEqual(0, zero.Numerator);
        Assert.AreEqual(1, zero.Denominator);
    }

    [TestMethod]
    public void Constructor_RejectsZeroDenominator() =>
        Assert.ThrowsExactly<DivideByZeroException>(() => new Fraction(1, 0));

    [TestMethod]
    public void Default_BehavesAsZero()
    {
        var d = default(Fraction);
        Assert.AreEqual(1, d.Denominator);
        Assert.AreEqual("0", d.ToString());
        Assert.IsTrue(d == new Fraction(0, 5));
        Assert.AreEqual(new Fraction(1, 3), d + new Fraction(1, 3));

        var array = new Fraction[3];
        Assert.AreEqual(0.0, (double)array[0]);
    }

    [TestMethod]
    public void Arithmetic_ProducesNormalizedResults()
    {
        var half = new Fraction(1, 2);
        var third = new Fraction(1, 3);

        Assert.AreEqual(new Fraction(5, 6), half + third);
        Assert.AreEqual(new Fraction(1, 6), half - third);
        Assert.AreEqual(new Fraction(1, 6), half * third);
        Assert.AreEqual(new Fraction(3, 2), half / third);
        Assert.AreEqual(new Fraction(-1, 2), -half);
        Assert.AreEqual("2", (new Fraction(1, 2) / new Fraction(1, 4)).ToString());
    }

    [TestMethod]
    public void Division_ByZeroFractionThrows() =>
        Assert.ThrowsExactly<DivideByZeroException>(() => new Fraction(1, 2) / new Fraction(0, 1));

    [TestMethod]
    public void Arithmetic_ThrowsOnOverflow()
    {
        var huge = new Fraction(long.MaxValue, 1);
        Assert.ThrowsExactly<OverflowException>(() => huge + huge);
        Assert.ThrowsExactly<OverflowException>(() => huge * new Fraction(2, 1));
    }

    [TestMethod]
    public void Comparison_UsesCrossMultiplication()
    {
        var third = new Fraction(1, 3);
        var half = new Fraction(1, 2);

        Assert.IsTrue(third < half);
        Assert.IsTrue(half > third);
        Assert.IsFalse(half < new Fraction(2, 4));
        Assert.IsTrue(new Fraction(-1, 2) < new Fraction(1, 3));
        Assert.IsTrue(new Fraction(2, 4) == half);
        Assert.IsTrue(third != half);
        Assert.AreEqual(new Fraction(2, 4).GetHashCode(), half.GetHashCode());
    }

    [TestMethod]
    public void Sorting_UsesIComparable()
    {
        var list = new List<Fraction> { new(3, 4), new(-1, 2), new(1, 3), 1 };
        list.Sort();
        CollectionAssert.AreEqual(new[] { "-1/2", "1/3", "3/4", "1" }, list.Select(f => f.ToString()).ToArray());
    }

    [TestMethod]
    public void Conversions_ImplicitFromIntExplicitToDouble()
    {
        Fraction three = 3;
        Assert.AreEqual(new Fraction(3, 1), three);
        Assert.AreEqual(new Fraction(7, 2), three + new Fraction(1, 2));
        Assert.AreEqual(0.25, (double)new Fraction(1, 4), 1e-12);
    }

    [TestMethod]
    public void ToString_FormatsCanonically()
    {
        Assert.AreEqual("3/4", new Fraction(6, 8).ToString());
        Assert.AreEqual("-1/2", new Fraction(1, -2).ToString());
        Assert.AreEqual("5", new Fraction(10, 2).ToString());
    }

    [TestMethod]
    public void Parse_AcceptsFractionsAndWholeNumbers()
    {
        Assert.AreEqual(new Fraction(3, 4), Fraction.Parse("3/4"));
        Assert.AreEqual(new Fraction(-3, 4), Fraction.Parse("-3/4"));
        Assert.AreEqual(new Fraction(5, 1), Fraction.Parse("5"));
        Assert.AreEqual(new Fraction(1, 2), Fraction.Parse("2/4"));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("1/2/3")]
    [DataRow("one/two")]
    [DataRow("1.5")]
    public void Parse_RejectsInvalidText(string text) =>
        Assert.ThrowsExactly<FormatException>(() => Fraction.Parse(text));
}
