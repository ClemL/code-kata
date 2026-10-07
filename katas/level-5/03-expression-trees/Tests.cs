using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.Expressions;

public sealed record Address(string City, string State);

public sealed record Patient(string Name, int Age, Address Address, DateOnly? LastVisit);

[TestClass]
public class ExpressionTreesTests
{
    private static readonly Patient[] Patients =
    [
        new("Ada", 36, new Address("Boston", "MA"), new DateOnly(2026, 9, 1)),
        new("Grace", 85, new Address("Arlington", "VA"), null),
        new("Linus", 36, new Address("Portland", "OR"), new DateOnly(2026, 1, 15)),
    ];

    [TestMethod]
    public void PropertyEquals_BuildsAQueryablePredicate()
    {
        var byName = ExpressionKata.PropertyEquals<Patient>("Name", "Grace");

        Assert.AreEqual(ExpressionType.Equal, byName.Body.NodeType);
        CollectionAssert.AreEqual(new[] { "Grace" }, Patients.AsQueryable().Where(byName).Select(p => p.Name).ToArray());
    }

    [TestMethod]
    public void PropertyEquals_ConvertsValueToPropertyType()
    {
        var byAge = ExpressionKata.PropertyEquals<Patient>("Age", 36L);
        Assert.AreEqual(2, Patients.AsQueryable().Count(byAge));

        var byVisit = ExpressionKata.PropertyEquals<Patient>("LastVisit", null);
        CollectionAssert.AreEqual(new[] { "Grace" }, Patients.AsQueryable().Where(byVisit).Select(p => p.Name).ToArray());
    }

    [TestMethod]
    public void PropertyEquals_RejectsUnknownProperty() =>
        Assert.ThrowsExactly<ArgumentException>(() => ExpressionKata.PropertyEquals<Patient>("Weight", 1));

    [TestMethod]
    public void AndAlso_ProducesASingleParameterLambda()
    {
        Expression<Func<Patient, bool>> adult = p => p.Age >= 18;
        Expression<Func<Patient, bool>> inMa = patient => patient.Address.State == "MA";

        var combined = ExpressionKata.AndAlso(adult, inMa);

        Assert.HasCount(1, combined.Parameters);
        Assert.AreEqual(ExpressionType.AndAlso, combined.Body.NodeType);
        CollectionAssert.AreEqual(new[] { "Ada" }, Patients.AsQueryable().Where(combined).Select(p => p.Name).ToArray());
        Assert.IsFalse(combined.Compile()(new Patient("Kid", 9, new Address("Boston", "MA"), null)));
    }

    [TestMethod]
    public void AndAlso_ComposesRepeatedly()
    {
        var predicate = ExpressionKata.AndAlso(
            ExpressionKata.PropertyEquals<Patient>("Age", 36),
            ExpressionKata.AndAlso<Patient>(p => p.Name.StartsWith('L'), p => p.LastVisit != null));

        CollectionAssert.AreEqual(new[] { "Linus" }, Patients.AsQueryable().Where(predicate).Select(p => p.Name).ToArray());
    }

    [TestMethod]
    public void MemberPath_ExtractsPaths()
    {
        Assert.AreEqual("Name", ExpressionKata.MemberPath<Patient>(p => p.Name));
        Assert.AreEqual("Age", ExpressionKata.MemberPath<Patient>(p => p.Age));
        Assert.AreEqual("Address.City", ExpressionKata.MemberPath<Patient>(p => p.Address.City));
        Assert.AreEqual("LastVisit", ExpressionKata.MemberPath<Patient>(p => p.LastVisit));
    }

    [TestMethod]
    public void MemberPath_RejectsNonMemberExpressions()
    {
        Assert.ThrowsExactly<ArgumentException>(() => ExpressionKata.MemberPath<Patient>(p => p.Name.ToUpperInvariant()));
        Assert.ThrowsExactly<ArgumentException>(() => ExpressionKata.MemberPath<Patient>(p => p));
        Assert.ThrowsExactly<ArgumentException>(() => ExpressionKata.MemberPath<Patient>(p => DateTime.Now));
    }

    [TestMethod]
    public void CompileGetter_ReadsProperties()
    {
        var getName = ExpressionKata.CompileGetter<Patient, string>("Name");
        var getAgeAsObject = ExpressionKata.CompileGetter<Patient, object>("Age");

        Assert.AreEqual("Ada", getName(Patients[0]));
        Assert.AreEqual(85, getAgeAsObject(Patients[1]));
        Assert.ThrowsExactly<ArgumentException>(() => ExpressionKata.CompileGetter<Patient, int>("Height"));
    }
}
