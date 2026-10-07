using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L3.LinqJoins;

[TestClass]
public class LinqJoinsTests
{
    private static readonly Department[] Departments =
    [
        new(1, "Engineering"),
        new(2, "Analytics"),
        new(3, "Finance"),
    ];

    private static readonly Employee[] Employees =
    [
        new(1, "Ada", 1, 150_000m, null),
        new(2, "Grace", 1, 160_000m, 1),
        new(3, "Linus", 2, 120_000m, 1),
        new(4, "Margaret", 2, 130_000m, 3),
        new(5, "Ken", null, 90_000m, 1),
        new(6, "Barbara", 1, 140_000m, 2),
    ];

    private static readonly Project[] Projects =
    [
        new(10, "Atlas"),
        new(20, "Beacon"),
        new(30, "Comet"),
    ];

    private static readonly Assignment[] Assignments =
    [
        new(1, 10, 20), new(1, 20, 10), new(1, 30, 5),
        new(2, 10, 40), new(2, 20, 8),
        new(3, 30, 30),
        new(4, 10, 12), new(4, 20, 6), new(4, 30, 4),
        new(6, 20, 25),
    ];

    [TestMethod]
    public void EmployeeDepartments_IsAnInnerJoin()
    {
        var rows = OrgQueries.EmployeeDepartments(Employees, Departments);

        CollectionAssert.AreEqual(
            new[] { ("Ada", "Engineering"), ("Barbara", "Engineering"), ("Grace", "Engineering"), ("Linus", "Analytics"), ("Margaret", "Analytics") },
            rows.ToArray());
    }

    [TestMethod]
    public void EmployeeDepartmentsOrNone_IsALeftJoin()
    {
        var rows = OrgQueries.EmployeeDepartmentsOrNone(Employees, Departments);

        Assert.HasCount(6, rows);
        Assert.AreEqual(("Ken", "(none)"), rows[3]);
        Assert.AreEqual(("Ada", "Engineering"), rows[0]);
    }

    [TestMethod]
    public void HeadcountByDepartment_IncludesEmptyDepartments()
    {
        CollectionAssert.AreEqual(
            new[] { ("Engineering", 3), ("Analytics", 2), ("Finance", 0) },
            OrgQueries.HeadcountByDepartment(Departments, Employees).ToArray());
    }

    [TestMethod]
    public void EarnsMoreThanManager_IsASelfJoin() =>
        CollectionAssert.AreEqual(new[] { "Grace", "Margaret" }, OrgQueries.EarnsMoreThanManager(Employees).ToArray());

    [TestMethod]
    public void HoursByProject_SumsAndSortsDescending() =>
        CollectionAssert.AreEqual(
            new[] { ("Atlas", 72), ("Beacon", 49), ("Comet", 39) },
            OrgQueries.HoursByProject(Projects, Assignments).ToArray());

    [TestMethod]
    public void HoursByProject_IncludesProjectsWithoutAssignments()
    {
        var rows = OrgQueries.HoursByProject([.. Projects, new Project(40, "Dormant")], Assignments);
        Assert.AreEqual(("Dormant", 0), rows[^1]);
    }

    [TestMethod]
    public void EmployeesOnEveryProject_PerformsRelationalDivision() =>
        CollectionAssert.AreEqual(
            new[] { "Ada", "Margaret" },
            OrgQueries.EmployeesOnEveryProject(Employees, Projects, Assignments).ToArray());

    [TestMethod]
    public void CompareRosters_UsesSetOperations()
    {
        var diff = OrgQueries.CompareRosters(["Ada", "Grace", "Ken", "Ken"], ["Grace", "Linus", "Ada", "Barbara"]);

        CollectionAssert.AreEqual(new[] { "Barbara", "Linus" }, diff.Added.ToArray());
        CollectionAssert.AreEqual(new[] { "Ken" }, diff.Removed.ToArray(), "Set operations remove duplicates.");
        CollectionAssert.AreEqual(new[] { "Ada", "Grace" }, diff.Kept.ToArray());
    }

    [TestMethod]
    public void Page_ReturnsOneBasedPages()
    {
        var numbers = Enumerable.Range(1, 10);

        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4 }, OrgQueries.Page(numbers, 1, 4).ToArray());
        CollectionAssert.AreEqual(new[] { 9, 10 }, OrgQueries.Page(numbers, 3, 4).ToArray());
        Assert.IsEmpty(OrgQueries.Page(numbers, 4, 4));
    }

    [TestMethod]
    public void Page_ValidatesArguments()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => OrgQueries.Page(new[] { 1 }, 0, 4));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => OrgQueries.Page(new[] { 1 }, 1, 0));
    }
}
