using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.GenericMath;

[TestClass]
public class GenericMathTests
{
    [TestMethod]
    public void Sum_WorksForBuiltInNumbers()
    {
        Assert.AreEqual(10, GenericStats.Sum([1, 2, 3, 4]));
        Assert.AreEqual(0.6, GenericStats.Sum([0.1, 0.2, 0.3]), 1e-12);
        Assert.AreEqual(0.6m, GenericStats.Sum([0.1m, 0.2m, 0.3m]));
        Assert.AreEqual(0L, GenericStats.Sum(Array.Empty<long>()));
    }

    [TestMethod]
    public void Sum_WorksForYourOwnType() =>
        Assert.AreEqual(new Money(12.75m), GenericStats.Sum([new Money(10m), new Money(2.5m), new Money(0.25m)]));

    [TestMethod]
    public void Average_AccumulatesInResultType()
    {
        Assert.AreEqual(2.5, GenericStats.Average<int, double>([1, 2, 3, 4]));
        Assert.AreEqual(2.5m, GenericStats.Average<int, decimal>([1, 2, 3, 4]));
        Assert.AreEqual((double)int.MaxValue, GenericStats.Average<int, double>([int.MaxValue, int.MaxValue]), "Summing in int would overflow.");
    }

    [TestMethod]
    public void Average_RejectsEmptyInput() =>
        Assert.ThrowsExactly<InvalidOperationException>(() => GenericStats.Average<int, double>([]));

    [TestMethod]
    public void StandardDeviation_WorksForEveryFloatingPointType()
    {
        Assert.AreEqual(2.0, GenericStats.StandardDeviation<double>([2, 4, 4, 4, 5, 5, 7, 9]), 1e-12);
        Assert.AreEqual(2.0f, GenericStats.StandardDeviation<float>([2f, 4f, 4f, 4f, 5f, 5f, 7f, 9f]), 1e-6f);
        Assert.AreEqual(0.0, GenericStats.StandardDeviation<double>([42]), 1e-12);
        Assert.AreEqual((Half)2, GenericStats.StandardDeviation<Half>([(Half)2, (Half)4, (Half)4, (Half)4, (Half)5, (Half)5, (Half)7, (Half)9]));
    }

    [TestMethod]
    public void StandardDeviation_RejectsEmptyInput() =>
        Assert.ThrowsExactly<InvalidOperationException>(() => GenericStats.StandardDeviation<double>([]));

    [TestMethod]
    public void Gcd_WorksForEveryBinaryInteger()
    {
        Assert.AreEqual(6, GenericStats.Gcd(48, 18));
        Assert.AreEqual(6, GenericStats.Gcd(-48, 18));
        Assert.AreEqual(7L, GenericStats.Gcd(0L, 7L));
        Assert.AreEqual((byte)4, GenericStats.Gcd((byte)12, (byte)8));
        Assert.AreEqual(BigInteger.Pow(2, 100), GenericStats.Gcd(BigInteger.Pow(2, 100) * 3, BigInteger.Pow(2, 101)));
    }

    [TestMethod]
    public void Fold_UsesStaticAbstractMembers()
    {
        Assert.AreEqual(new MaxInt(9), GenericStats.Fold([new MaxInt(3), new MaxInt(9), new MaxInt(-2)]));
        Assert.AreEqual(new MaxInt(int.MinValue), GenericStats.Fold(Array.Empty<MaxInt>()));

        Assert.AreEqual(new Csv("a,b,c"), GenericStats.Fold([new Csv("a"), new Csv(""), new Csv("b"), new Csv("c")]));
        Assert.AreEqual(new Csv(""), GenericStats.Fold(Array.Empty<Csv>()));
    }
}
