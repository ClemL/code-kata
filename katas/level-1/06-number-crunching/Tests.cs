using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L1.Numbers;

[TestClass]
public class NumberCrunchingTests
{
    [TestMethod]
    public void CheckedAdd_ThrowsOnOverflow()
    {
        Assert.AreEqual(3, Numbers.CheckedAdd(1, 2));
        Assert.ThrowsExactly<OverflowException>(() => Numbers.CheckedAdd(int.MaxValue, 1));
        Assert.ThrowsExactly<OverflowException>(() => Numbers.CheckedAdd(int.MinValue, -1));
    }

    [TestMethod]
    public void WrappingAdd_WrapsAround()
    {
        Assert.AreEqual(int.MinValue, Numbers.WrappingAdd(int.MaxValue, 1));
        Assert.AreEqual(int.MaxValue, Numbers.WrappingAdd(int.MinValue, -1));
    }

    [TestMethod]
    public void RoundMoney_RoundsMidpointsAwayFromZero()
    {
        Assert.AreEqual(2.35m, Numbers.RoundMoney(2.345m));
        Assert.AreEqual(-2.35m, Numbers.RoundMoney(-2.345m));
        Assert.AreEqual(2.34m, Numbers.RoundMoney(2.344m));
        Assert.AreEqual(1_000_000.01m, Numbers.RoundMoney(1_000_000.005m));
    }

    [TestMethod]
    public void RoundBankers_RoundsMidpointsToEven()
    {
        Assert.AreEqual(2.34m, Numbers.RoundBankers(2.345m));
        Assert.AreEqual(2.36m, Numbers.RoundBankers(2.355m));
        Assert.AreEqual(-2.34m, Numbers.RoundBankers(-2.345m));
    }

    [TestMethod]
    public void NearlyEqual_UsesRelativeTolerance()
    {
        double tenth = 0.1, fifth = 0.2;
        Assert.AreNotEqual(0.3, tenth + fifth, "This is why NearlyEqual exists.");
        Assert.IsTrue(Numbers.NearlyEqual(tenth + fifth, 0.3));
        Assert.IsTrue(Numbers.NearlyEqual(1e15, 1e15 + 0.5));
        Assert.IsFalse(Numbers.NearlyEqual(1e-12, 2e-12), "Tiny values that differ by 2x are not equal.");
        Assert.IsFalse(Numbers.NearlyEqual(1.0, 1.001));
        Assert.IsTrue(Numbers.NearlyEqual(1.0, 1.001, relativeTolerance: 0.01));
    }

    [TestMethod]
    public void NearlyEqual_HandlesSpecialValues()
    {
        Assert.IsTrue(Numbers.NearlyEqual(0.0, 0.0));
        Assert.IsTrue(Numbers.NearlyEqual(0.0, -0.0));
        Assert.IsFalse(Numbers.NearlyEqual(double.NaN, double.NaN));
        Assert.IsTrue(Numbers.NearlyEqual(double.PositiveInfinity, double.PositiveInfinity));
        Assert.IsFalse(Numbers.NearlyEqual(double.PositiveInfinity, double.MaxValue));
    }

    [TestMethod]
    [DataRow(1L, true)]
    [DataRow(2L, true)]
    [DataRow(1024L, true)]
    [DataRow(1L << 62, true)]
    [DataRow(0L, false)]
    [DataRow(-8L, false)]
    [DataRow(6L, false)]
    [DataRow(long.MaxValue, false)]
    public void IsPowerOfTwo_UsesBitTrick(long n, bool expected) =>
        Assert.AreEqual(expected, Numbers.IsPowerOfTwo(n));

    [TestMethod]
    public void CountSetBits_CountsOnes()
    {
        Assert.AreEqual(0, Numbers.CountSetBits(0));
        Assert.AreEqual(3, Numbers.CountSetBits(0b_1010_0001));
        Assert.AreEqual(32, Numbers.CountSetBits(uint.MaxValue));
    }

    [TestMethod]
    public void Factorial_HandlesLargeValues()
    {
        Assert.AreEqual(BigInteger.One, Numbers.Factorial(0));
        Assert.AreEqual(new BigInteger(120), Numbers.Factorial(5));
        Assert.AreEqual(BigInteger.Parse("15511210043330985984000000"), Numbers.Factorial(25));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Numbers.Factorial(-1));
    }
}
