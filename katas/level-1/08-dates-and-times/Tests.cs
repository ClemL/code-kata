using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L1.Dates;

[TestClass]
public class DatesAndTimesTests
{
    [TestMethod]
    public void AgeOn_CountsFullYears()
    {
        var birth = new DateOnly(2000, 6, 15);
        Assert.AreEqual(25, DateKata.AgeOn(birth, new DateOnly(2026, 6, 14)));
        Assert.AreEqual(26, DateKata.AgeOn(birth, new DateOnly(2026, 6, 15)));
        Assert.AreEqual(0, DateKata.AgeOn(birth, birth));
    }

    [TestMethod]
    public void AgeOn_LeapDayBirthdayFallsOnMarchFirst()
    {
        var leapling = new DateOnly(2004, 2, 29);
        Assert.AreEqual(20, DateKata.AgeOn(leapling, new DateOnly(2025, 2, 28)));
        Assert.AreEqual(21, DateKata.AgeOn(leapling, new DateOnly(2025, 3, 1)));
        Assert.AreEqual(24, DateKata.AgeOn(leapling, new DateOnly(2028, 2, 29)));
    }

    [TestMethod]
    public void AgeOn_RejectsDateBeforeBirth() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            DateKata.AgeOn(new DateOnly(2000, 1, 2), new DateOnly(2000, 1, 1)));

    [TestMethod]
    public void BusinessDaysBetween_CountsWeekdaysInHalfOpenRange()
    {
        Assert.AreEqual(5, DateKata.BusinessDaysBetween(new DateOnly(2026, 10, 5), new DateOnly(2026, 10, 12)));
        Assert.AreEqual(0, DateKata.BusinessDaysBetween(new DateOnly(2026, 10, 3), new DateOnly(2026, 10, 5)));
        Assert.AreEqual(0, DateKata.BusinessDaysBetween(new DateOnly(2026, 10, 7), new DateOnly(2026, 10, 7)));
        Assert.AreEqual(22, DateKata.BusinessDaysBetween(new DateOnly(2026, 10, 1), new DateOnly(2026, 11, 1)));
    }

    [TestMethod]
    public void BusinessDaysBetween_RejectsReversedRange() =>
        Assert.ThrowsExactly<ArgumentException>(() =>
            DateKata.BusinessDaysBetween(new DateOnly(2026, 10, 8), new DateOnly(2026, 10, 7)));

    [TestMethod]
    public void NextOccurrence_IsStrictlyAfter()
    {
        var wednesday = new DateOnly(2026, 10, 7);
        Assert.AreEqual(new DateOnly(2026, 10, 9), DateKata.NextOccurrence(wednesday, DayOfWeek.Friday));
        Assert.AreEqual(new DateOnly(2026, 10, 12), DateKata.NextOccurrence(wednesday, DayOfWeek.Monday));
        Assert.AreEqual(new DateOnly(2026, 10, 14), DateKata.NextOccurrence(wednesday, DayOfWeek.Wednesday));
    }

    [TestMethod]
    public void EndOfMonth_HandlesLeapYears()
    {
        Assert.AreEqual(new DateOnly(2024, 2, 29), DateKata.EndOfMonth(new DateOnly(2024, 2, 10)));
        Assert.AreEqual(new DateOnly(2026, 2, 28), DateKata.EndOfMonth(new DateOnly(2026, 2, 10)));
        Assert.AreEqual(new DateOnly(2026, 12, 31), DateKata.EndOfMonth(new DateOnly(2026, 12, 31)));
    }

    [TestMethod]
    [DataRow(1, 1)]
    [DataRow(3, 1)]
    [DataRow(4, 2)]
    [DataRow(9, 3)]
    [DataRow(10, 4)]
    [DataRow(12, 4)]
    public void QuarterOf_MapsMonths(int month, int expected) =>
        Assert.AreEqual(expected, DateKata.QuarterOf(new DateOnly(2026, month, 1)));

    [TestMethod]
    public void ParseIsoDate_IsStrict()
    {
        Assert.AreEqual(new DateOnly(2026, 10, 7), DateKata.ParseIsoDate("2026-10-07"));
        Assert.IsNull(DateKata.ParseIsoDate("10/07/2026"));
        Assert.IsNull(DateKata.ParseIsoDate("2026-02-30"));
        Assert.IsNull(DateKata.ParseIsoDate("2026-10-07T00:00:00"));
        Assert.IsNull(DateKata.ParseIsoDate(null));
    }

    [TestMethod]
    public void IsSameInstant_IgnoresOffsets()
    {
        var boston = new DateTimeOffset(2026, 10, 7, 9, 0, 0, TimeSpan.FromHours(-4));
        var utc = new DateTimeOffset(2026, 10, 7, 13, 0, 0, TimeSpan.Zero);

        Assert.IsTrue(DateKata.IsSameInstant(boston, utc));
        Assert.IsFalse(boston.EqualsExact(utc), "EqualsExact also compares the offset.");
        Assert.IsFalse(DateKata.IsSameInstant(boston, utc.AddSeconds(1)));
    }

    [TestMethod]
    public void ToUtcStamp_FormatsInUtc() =>
        Assert.AreEqual(
            "2026-10-07T13:30:15Z",
            DateKata.ToUtcStamp(new DateTimeOffset(2026, 10, 7, 9, 30, 15, TimeSpan.FromHours(-4))));

    [TestMethod]
    public void FormatDuration_FormatsWithOptionalDays()
    {
        Assert.AreEqual("00h 00m 00s", DateKata.FormatDuration(TimeSpan.Zero));
        Assert.AreEqual("01h 02m 05s", DateKata.FormatDuration(TimeSpan.FromSeconds(3725)));
        Assert.AreEqual("1d 01h 01m 01s", DateKata.FormatDuration(TimeSpan.FromSeconds(90061)));
        Assert.AreEqual("12d 00h 00m 00s", DateKata.FormatDuration(TimeSpan.FromDays(12)));
    }

    [TestMethod]
    public void FormatDuration_RejectsNegative() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => DateKata.FormatDuration(TimeSpan.FromSeconds(-1)));
}
