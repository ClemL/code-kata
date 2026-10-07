using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L2.Records;

[TestClass]
public class RecordsTests
{
    [TestMethod]
    public void Money_HasValueEquality()
    {
        var a = new Money(10m, "USD");
        var b = new Money(10m, "USD");

        Assert.AreEqual(a, b);
        Assert.IsTrue(a == b, "== should compare values, not references.");
        Assert.IsFalse(a != b);
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        Assert.AreNotEqual(a, new Money(10m, "EUR"));
    }

    [TestMethod]
    public void Money_HasReadableToString() =>
        Assert.AreEqual("Money { Amount = 10, Currency = USD }", new Money(10m, "USD").ToString());

    [TestMethod]
    public void Money_Deconstructs()
    {
        var (amount, currency) = new Money(4.5m, "GBP");
        Assert.AreEqual(4.5m, amount);
        Assert.AreEqual("GBP", currency);
    }

    [TestMethod]
    public void Add_ReturnsNewValueAndLeavesOperandsAlone()
    {
        var a = new Money(10m, "USD");
        var b = new Money(2.5m, "USD");

        var sum = a.Add(b);

        Assert.AreEqual(new Money(12.5m, "USD"), sum);
        Assert.AreEqual(10m, a.Amount);
        Assert.AreEqual(2.5m, b.Amount);
    }

    [TestMethod]
    public void Add_RejectsMismatchedCurrencies() =>
        Assert.ThrowsExactly<InvalidOperationException>(() => new Money(1m, "USD").Add(new Money(1m, "EUR")));

    [TestMethod]
    public void Scale_RoundsToCents()
    {
        Assert.AreEqual(new Money(10.75m, "USD"), new Money(10m, "USD").Scale(1.075m));
        Assert.AreEqual(new Money(0.13m, "USD"), new Money(0.25m, "USD").Scale(0.5m));
    }

    [TestMethod]
    public void PersonAndEmployee_RespectRuntimeType()
    {
        Assert.AreEqual(new Person("Ada"), new Person("Ada"));
        Assert.AreEqual(new Employee("Ada", "Engineer"), new Employee("Ada", "Engineer"));

        Person employeeAsPerson = new Employee("Ada", "Engineer");
        Assert.IsFalse(new Person("Ada").Equals(employeeAsPerson));
        Assert.IsFalse(employeeAsPerson.Equals(new Person("Ada")));
        Assert.AreEqual("Employee { Name = Ada, Title = Engineer }", employeeAsPerson.ToString());
    }

    [TestMethod]
    public void Promote_ReturnsModifiedCopy()
    {
        var before = new Employee("Grace", "Engineer");
        var after = RecordKata.Promote(before, "Rear Admiral");

        Assert.AreEqual("Rear Admiral", after.Title);
        Assert.AreEqual("Grace", after.Name);
        Assert.AreEqual("Engineer", before.Title);
        Assert.AreNotSame(before, after);
    }

    [TestMethod]
    public void Total_SumsAmounts()
    {
        Assert.AreEqual(new Money(15.5m, "USD"), RecordKata.Total([new(10m, "USD"), new(5.5m, "USD")], "USD"));
        Assert.AreEqual(new Money(0m, "EUR"), RecordKata.Total([], "EUR"));
        Assert.ThrowsExactly<InvalidOperationException>(() => RecordKata.Total([new(1m, "USD"), new(1m, "EUR")], "USD"));
    }

    [TestMethod]
    public void DistinctAmounts_UsesValueEquality()
    {
        var distinct = RecordKata.DistinctAmounts([new(10m, "USD"), new(10m, "USD"), new(10m, "EUR"), new(10m, "USD")]);

        Assert.HasCount(2, distinct);
        Assert.AreEqual(new Money(10m, "USD"), distinct[0]);
        Assert.AreEqual(new Money(10m, "EUR"), distinct[1]);
    }
}
