using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L3.Immutable;

[TestClass]
public class ImmutableAndFrozenTests
{
    [TestMethod]
    public void ReadOnlyView_ReflectsChangesToSource()
    {
        var source = new List<int> { 1, 2 };
        var view = Snapshots.ReadOnlyView(source);

        source.Add(3);

        Assert.HasCount(3, view);
        Assert.IsNotInstanceOfType<List<int>>(view, "Return a wrapper, not the list itself, so callers cannot cast back.");
    }

    [TestMethod]
    public void Snapshot_IsIsolatedFromSource()
    {
        var source = new List<int> { 1, 2 };
        var snapshot = Snapshots.Snapshot(source);

        source.Add(3);
        source[0] = 99;

        CollectionAssert.AreEqual(new[] { 1, 2 }, snapshot.ToArray());
    }

    [TestMethod]
    public void Squares_UsesExactCapacityBuilder()
    {
        var squares = Snapshots.Squares(5);
        CollectionAssert.AreEqual(new[] { 0, 1, 4, 9, 16 }, squares.ToArray());
        Assert.IsTrue(Snapshots.Squares(0).IsEmpty);
    }

    [TestMethod]
    public void AuditTrail_AppendReturnsNewInstance()
    {
        var empty = AuditTrail.Empty;
        var one = empty.Append("created");
        var two = one.Append("approved");

        Assert.IsEmpty(empty.Entries);
        CollectionAssert.AreEqual(new[] { "created" }, one.Entries.ToArray());
        CollectionAssert.AreEqual(new[] { "created", "approved" }, two.Entries.ToArray());
        Assert.AreSame(empty, AuditTrail.Empty, "Empty should be a shared instance.");
    }

    [TestMethod]
    public void Inventory_RestockAndRemoveAreNonDestructive()
    {
        var start = Inventory.Empty.Restock("ABC", 10);
        var after = start.Restock("ABC", 5).Remove("ABC", 3).Restock("XYZ", 1);

        Assert.AreEqual(10, start.QuantityOf("ABC"));
        Assert.AreEqual(12, after.QuantityOf("ABC"));
        Assert.AreEqual(1, after.QuantityOf("XYZ"));
        Assert.AreEqual(0, after.QuantityOf("NOPE"));
    }

    [TestMethod]
    public void Inventory_RemovesSkuAtZeroAndRejectsOverdraw()
    {
        var inventory = Inventory.Empty.Restock("ABC", 2);

        var emptied = inventory.Remove("ABC", 2);
        Assert.IsFalse(emptied.Stock.ContainsKey("ABC"));

        Assert.ThrowsExactly<InvalidOperationException>(() => inventory.Remove("ABC", 3));
        Assert.ThrowsExactly<InvalidOperationException>(() => inventory.Remove("MISSING", 1));
    }

    [TestMethod]
    public void BuildLookup_IsCaseInsensitiveAndFrozen()
    {
        var lookup = CodeTables.BuildLookup([("rx", "Prescription"), ("OTC", "Over the counter")]);

        Assert.AreEqual("Prescription", lookup["RX"]);
        Assert.AreEqual("Over the counter", lookup["otc"]);
        Assert.IsFalse(lookup.ContainsKey("DME"));
    }

    [TestMethod]
    public void BuildLookup_RejectsDuplicateCodes() =>
        Assert.ThrowsExactly<ArgumentException>(() => CodeTables.BuildLookup([("rx", "a"), ("RX", "b")]));

    [TestMethod]
    public void BuildAllowList_IsCaseInsensitive()
    {
        var allowed = CodeTables.BuildAllowList(["NDC", "hcpcs", "ndc"]);

        Assert.IsTrue(allowed.Contains("ndc"));
        Assert.IsTrue(allowed.Contains("HCPCS"));
        Assert.IsFalse(allowed.Contains("cpt"));
        Assert.HasCount(2, allowed);
    }
}
