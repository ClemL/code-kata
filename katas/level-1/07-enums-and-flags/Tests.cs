using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L1.Enums;

[TestClass]
public class EnumsAndFlagsTests
{
    [TestMethod]
    public void Permission_IsAProperFlagsEnum()
    {
        Assert.IsTrue(Attribute.IsDefined(typeof(Permission), typeof(FlagsAttribute)), "Mark the enum with [Flags].");
        Assert.AreEqual("None", default(Permission).ToString());

        foreach (var single in new[] { Permission.Read, Permission.Write, Permission.Delete, Permission.Share })
        {
            Assert.AreEqual(1, int.PopCount((int)single), $"{single} should occupy exactly one bit.");
        }

        Assert.AreEqual("Read, Write", (Permission.Read | Permission.Write).ToString());
        Assert.AreEqual("Admin", (Permission.Read | Permission.Write | Permission.Delete | Permission.Share).ToString());
    }

    [TestMethod]
    public void Grant_AddsBits()
    {
        var p = Access.Grant(Permission.Read, Permission.Write);
        Assert.AreEqual(Permission.Read | Permission.Write, p);
        Assert.AreEqual(Permission.Admin, Access.Grant(p, Permission.Delete | Permission.Share));
    }

    [TestMethod]
    public void Revoke_RemovesOnlyRequestedBits()
    {
        Assert.AreEqual(Permission.Read | Permission.Share, Access.Revoke(Permission.Admin, Permission.Write | Permission.Delete));
        Assert.AreEqual(Permission.Read, Access.Revoke(Permission.Read, Permission.Write), "Revoking a missing bit is a no-op.");
    }

    [TestMethod]
    public void HasAll_RequiresEveryBit()
    {
        Assert.IsTrue(Access.HasAll(Permission.Admin, Permission.Read | Permission.Delete));
        Assert.IsFalse(Access.HasAll(Permission.Read, Permission.Read | Permission.Write));
        Assert.IsTrue(Access.HasAll(Permission.Read, Permission.None));
    }

    [TestMethod]
    public void HasAny_RequiresOverlap()
    {
        Assert.IsTrue(Access.HasAny(Permission.Read | Permission.Share, Permission.Share | Permission.Delete));
        Assert.IsFalse(Access.HasAny(Permission.Read, Permission.Write | Permission.Delete));
        Assert.IsFalse(Access.HasAny(Permission.Admin, Permission.None));
    }

    [TestMethod]
    public void TryParse_AcceptsNamesAndCombinations()
    {
        Assert.IsTrue(Access.TryParse("read, write", out var p));
        Assert.AreEqual(Permission.Read | Permission.Write, p);

        Assert.IsTrue(Access.TryParse("ADMIN", out p));
        Assert.AreEqual(Permission.Admin, p);
    }

    [TestMethod]
    [DataRow("3")]
    [DataRow("42")]
    [DataRow("Execute")]
    [DataRow("")]
    public void TryParse_RejectsNumbersAndUnknownNames(string text)
    {
        Assert.IsFalse(Access.TryParse(text, out var p));
        Assert.AreEqual(Permission.None, p);
    }

    [TestMethod]
    [DataRow(TicketStatus.Open, TicketStatus.InProgress, true)]
    [DataRow(TicketStatus.Open, TicketStatus.Closed, true)]
    [DataRow(TicketStatus.Open, TicketStatus.Resolved, false)]
    [DataRow(TicketStatus.InProgress, TicketStatus.Blocked, true)]
    [DataRow(TicketStatus.InProgress, TicketStatus.Resolved, true)]
    [DataRow(TicketStatus.InProgress, TicketStatus.Closed, false)]
    [DataRow(TicketStatus.Blocked, TicketStatus.InProgress, true)]
    [DataRow(TicketStatus.Blocked, TicketStatus.Resolved, false)]
    [DataRow(TicketStatus.Resolved, TicketStatus.Closed, true)]
    [DataRow(TicketStatus.Resolved, TicketStatus.InProgress, true)]
    [DataRow(TicketStatus.Closed, TicketStatus.Open, false)]
    [DataRow(TicketStatus.Closed, TicketStatus.Closed, false)]
    public void CanTransition_EncodesTheStateMachine(TicketStatus from, TicketStatus to, bool expected) =>
        Assert.AreEqual(expected, Workflow.CanTransition(from, to));

    [TestMethod]
    public void ActiveStatuses_ExcludesTerminalStates() =>
        CollectionAssert.AreEqual(
            new[] { TicketStatus.Open, TicketStatus.InProgress, TicketStatus.Blocked },
            Workflow.ActiveStatuses().ToArray());
}
