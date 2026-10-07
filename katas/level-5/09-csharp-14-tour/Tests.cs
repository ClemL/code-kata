using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.CSharp14;

[TestClass]
public class CSharp14TourTests
{
    [TestMethod]
    public void ExtensionProperties_OnSequencesAndStrings()
    {
        Assert.IsTrue(Array.Empty<int>().IsEmpty);
        Assert.IsFalse(new[] { 1 }.IsEmpty);
        Assert.AreEqual(4, "  the quick\tbrown fox ".WordCount);
        Assert.AreEqual(0, "".WordCount);
    }

    [TestMethod]
    public void IsEmpty_StopsAfterOneElement()
    {
        static IEnumerable<int> Endless()
        {
            for (var i = 0; ; i++)
            {
                yield return i;
            }
        }

        Assert.IsFalse(Endless().IsEmpty);
    }

    [TestMethod]
    public void InstanceExtensionProperties_OnDateOnly()
    {
        var date = new DateOnly(2024, 2, 10);
        Assert.AreEqual(1, date.Quarter);
        Assert.AreEqual(new DateOnly(2024, 2, 29), date.EndOfMonth);
        Assert.AreEqual(4, new DateOnly(2026, 10, 7).Quarter);
    }

    [TestMethod]
    public void StaticExtensionMembers_LookLikeTheyBelongToDateOnly()
    {
        Assert.AreEqual(new DateOnly(2026, 10, 1), DateOnly.FirstOfMonth(2026, 10));
        Assert.AreEqual(new DateOnly(2026, 10, 7), DateOnly.ParseIso("2026-10-07"));
        Assert.ThrowsExactly<FormatException>(() => DateOnly.ParseIso("10/07/2026"));
    }

    [TestMethod]
    public void ExtensionOperator_SubtractsDates()
    {
        int days = new DateOnly(2026, 10, 7) - new DateOnly(2026, 1, 1);
        Assert.AreEqual(279, days);
        Assert.AreEqual(-1, new DateOnly(2026, 1, 1) - new DateOnly(2026, 1, 2));
        Assert.AreEqual(366, new DateOnly(2025, 1, 1) - new DateOnly(2024, 1, 1));
    }

    [TestMethod]
    public void NullConditionalAssignment_SkipsNullReceivers()
    {
        var now = new DateTimeOffset(2026, 10, 7, 9, 30, 0, TimeSpan.Zero);
        var customer = new Customer();

        Accounts.Touch(customer, now);
        Accounts.AddPoints(customer, 10);
        Accounts.AddPoints(customer, 5);

        Assert.AreEqual(now, customer.LastSeen);
        Assert.AreEqual(15, customer.LoyaltyPoints);

        Accounts.Touch(null, now);
        Accounts.AddPoints(null, 10);
    }

    [TestMethod]
    public void Parsers_UseOutParameters()
    {
        Assert.IsTrue(Parsers.Int("42", out var answer));
        Assert.AreEqual(42, answer);
        Assert.IsFalse(Parsers.Int("forty-two", out _));

        Assert.IsTrue(Parsers.IsoDate("2026-10-07", out var date));
        Assert.AreEqual(new DateOnly(2026, 10, 7), date);
        Assert.IsFalse(Parsers.IsoDate(null, out _));
    }

    [TestMethod]
    public void Names_UsesUnboundGenerics() =>
        Assert.AreEqual(("List", "Dictionary"), Names.Of());

    [TestMethod]
    public void CompoundAssignment_MutatesInPlace()
    {
        var total = new RunningTotal();
        var alias = total;

        total += 5m;
        total += 2.5m;

        Assert.AreSame(alias, total, "An instance += operator updates the existing object.");
        Assert.AreEqual(7.5m, total.Total);
        Assert.AreEqual(2, total.Count);
    }
}
