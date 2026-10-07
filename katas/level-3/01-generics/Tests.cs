using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L3.Generics;

[TestClass]
public class GenericsTests
{
    [TestMethod]
    public void Clamp_WorksForAnyComparable()
    {
        Assert.AreEqual(5, GenericKata.Clamp(7, 0, 5));
        Assert.AreEqual(0, GenericKata.Clamp(-3, 0, 5));
        Assert.AreEqual(3, GenericKata.Clamp(3, 0, 5));
        Assert.AreEqual("m", GenericKata.Clamp("z", "a", "m"));
        Assert.AreEqual(new DateOnly(2026, 1, 1), GenericKata.Clamp(new DateOnly(2025, 6, 1), new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)));
    }

    [TestMethod]
    public void Clamp_RejectsInvertedRange() =>
        Assert.ThrowsExactly<ArgumentException>(() => GenericKata.Clamp(1, 10, 0));

    [TestMethod]
    public void MaxOrDefault_ReturnsMaxOrFallback()
    {
        Assert.AreEqual(9, GenericKata.MaxOrDefault([3, 9, 2], -1));
        Assert.AreEqual(-1, GenericKata.MaxOrDefault(Array.Empty<int>(), -1));
        Assert.AreEqual("pear", GenericKata.MaxOrDefault(["apple", "pear", "fig"], "none"));
    }

    [TestMethod]
    public void MaxOrDefault_EnumeratesOnce()
    {
        var enumerations = 0;
        IEnumerable<int> Source()
        {
            enumerations++;
            yield return 1;
            yield return 4;
        }

        Assert.AreEqual(4, GenericKata.MaxOrDefault(Source(), 0));
        Assert.AreEqual(1, enumerations);
    }

    [TestMethod]
    public void ParseOrNull_UsesStaticAbstractTryParse()
    {
        Assert.AreEqual(42, GenericKata.ParseOrNull<int>("42"));
        Assert.AreEqual(3.5, GenericKata.ParseOrNull<double>("3.5"));
        Assert.AreEqual(new DateOnly(2026, 10, 7), GenericKata.ParseOrNull<DateOnly>("2026-10-07"));
        Assert.IsNull(GenericKata.ParseOrNull<int>("forty-two"));
        Assert.IsNull(GenericKata.ParseOrNull<Guid>(null));
    }

    private sealed class Connection
    {
        public Guid Id { get; } = Guid.NewGuid();
    }

    [TestMethod]
    public void Pool_ReusesReturnedInstances()
    {
        var pool = new Pool<Connection>();

        var first = pool.Rent();
        var second = pool.Rent();
        Assert.AreEqual(2, pool.Created);
        Assert.AreNotSame(first, second);

        pool.Return(first);
        var third = pool.Rent();

        Assert.AreSame(first, third);
        Assert.AreEqual(2, pool.Created);
    }

    [TestMethod]
    public void TypeMap_StoresOneValuePerType()
    {
        var map = new TypeMap();
        map.Set(42);
        map.Set("hello");
        map.Set<IComparable>(3.14);

        Assert.AreEqual(42, map.Get<int>());
        Assert.AreEqual("hello", map.Get<string>());
        Assert.AreEqual(3.14, map.Get<IComparable>());

        map.Set(7);
        Assert.AreEqual(7, map.Get<int>());
    }

    [TestMethod]
    public void TypeMap_ReportsMissingTypes()
    {
        var map = new TypeMap();
        Assert.IsFalse(map.TryGet<Uri>(out var missing));
        Assert.IsNull(missing);
        Assert.ThrowsExactly<KeyNotFoundException>(() => map.Get<DateTime>());
    }

    [TestMethod]
    public void Producer_IsCovariant()
    {
        var parameter = typeof(IProducer<>).GetGenericArguments()[0];
        Assert.IsTrue(parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Covariant), "Declare IProducer<out T>.");

        object producer = new ConstantProducer<string>("hi");
        var asObjects = (IProducer<object>)producer;
        Assert.AreEqual("hi", asObjects.Produce());
    }

    [TestMethod]
    public void Consumer_IsContravariant()
    {
        var parameter = typeof(IConsumer<>).GetGenericArguments()[0];
        Assert.IsTrue(parameter.GenericParameterAttributes.HasFlag(GenericParameterAttributes.Contravariant), "Declare IConsumer<in T>.");

        var sink = new ListConsumer<object>();
        object consumer = sink;
        var asStrings = (IConsumer<string>)consumer;
        asStrings.Consume("text");

        CollectionAssert.AreEqual(new object[] { "text" }, sink.Items.ToArray());
    }
}
