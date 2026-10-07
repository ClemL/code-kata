using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L1.PatternMatching;

[TestClass]
public class PatternMatchingTests
{
    [TestMethod]
    [DataRow(1, "1")]
    [DataRow(3, "Fizz")]
    [DataRow(5, "Buzz")]
    [DataRow(9, "Fizz")]
    [DataRow(10, "Buzz")]
    [DataRow(15, "FizzBuzz")]
    [DataRow(98, "98")]
    [DataRow(300, "FizzBuzz")]
    public void FizzBuzz_ReturnsExpectedWord(int n, string expected) =>
        Assert.AreEqual(expected, Patterns.FizzBuzz(n));

    [TestMethod]
    [DataRow(0)]
    [DataRow(-3)]
    public void FizzBuzz_RejectsNonPositive(int n) =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Patterns.FizzBuzz(n));

    [TestMethod]
    [DataRow(100, "A")]
    [DataRow(90, "A")]
    [DataRow(89, "B")]
    [DataRow(80, "B")]
    [DataRow(79, "C")]
    [DataRow(70, "C")]
    [DataRow(69, "D")]
    [DataRow(60, "D")]
    [DataRow(59, "F")]
    [DataRow(0, "F")]
    public void Grade_MapsBoundariesCorrectly(int score, string expected) =>
        Assert.AreEqual(expected, Patterns.Grade(score));

    [TestMethod]
    [DataRow(-1)]
    [DataRow(101)]
    public void Grade_RejectsOutOfRange(int score) =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Patterns.Grade(score));

    [TestMethod]
    [DataRow(0, 0, "origin")]
    [DataRow(0, 5, "y-axis")]
    [DataRow(-2, 0, "x-axis")]
    [DataRow(1, 1, "Q1")]
    [DataRow(-1, 1, "Q2")]
    [DataRow(-1, -1, "Q3")]
    [DataRow(1, -1, "Q4")]
    public void Quadrant_ClassifiesPoints(int x, int y, string expected) =>
        Assert.AreEqual(expected, Patterns.Quadrant(x, y));

    [TestMethod]
    public void Describe_HandlesNullAndInts()
    {
        Assert.AreEqual("null", Patterns.Describe(null));
        Assert.AreEqual("zero", Patterns.Describe(0));
        Assert.AreEqual("negative int", Patterns.Describe(-7));
        Assert.AreEqual("positive int", Patterns.Describe(42));
    }

    [TestMethod]
    public void Describe_HandlesStringsAndBools()
    {
        Assert.AreEqual("empty string", Patterns.Describe(""));
        Assert.AreEqual("string of length 5", Patterns.Describe("hello"));
        Assert.AreEqual("yes", Patterns.Describe(true));
        Assert.AreEqual("no", Patterns.Describe(false));
    }

    [TestMethod]
    public void Describe_HandlesDoublesAndUnknowns()
    {
        Assert.AreEqual("not a number", Patterns.Describe(double.NaN));
        Assert.AreEqual("double", Patterns.Describe(3.14));
        Assert.AreEqual("unknown: Decimal", Patterns.Describe(1.5m));
        Assert.AreEqual("unknown: Int64", Patterns.Describe(0L), "A boxed long 0 is not the int constant 0.");
    }

    [TestMethod]
    public void IsWeekend_DetectsSaturdayAndSunday()
    {
        Assert.IsTrue(Patterns.IsWeekend(new DateOnly(2026, 10, 3)));  // Saturday
        Assert.IsTrue(Patterns.IsWeekend(new DateOnly(2026, 10, 4)));  // Sunday
        Assert.IsFalse(Patterns.IsWeekend(new DateOnly(2026, 10, 5))); // Monday
        Assert.IsFalse(Patterns.IsWeekend(new DateOnly(2026, 10, 9))); // Friday
    }
}
