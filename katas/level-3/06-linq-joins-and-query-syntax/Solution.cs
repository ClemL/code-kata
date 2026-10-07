namespace Katas.L3.LinqJoins;

public sealed record Department(int Id, string Name);

public sealed record Employee(int Id, string Name, int? DepartmentId, decimal Salary, int? ManagerId);

public sealed record Project(int Id, string Name);

public sealed record Assignment(int EmployeeId, int ProjectId, int Hours);

public sealed record RosterDiff(IReadOnlyList<string> Added, IReadOnlyList<string> Removed, IReadOnlyList<string> Kept);

public static class OrgQueries
{
    public static IReadOnlyList<(string Employee, string Department)> EmployeeDepartments(
        IEnumerable<Employee> employees, IEnumerable<Department> departments) =>
        (from e in employees
         join d in departments on e.DepartmentId equals (int?)d.Id
         orderby e.Name
         select (e.Name, d.Name))
        .ToList();

    public static IReadOnlyList<(string Employee, string Department)> EmployeeDepartmentsOrNone(
        IEnumerable<Employee> employees, IEnumerable<Department> departments) =>
        (from e in employees
         join d in departments on e.DepartmentId equals (int?)d.Id into matches
         from d in matches.DefaultIfEmpty()
         orderby e.Name
         select (e.Name, d?.Name ?? "(none)"))
        .ToList();

    public static IReadOnlyList<(string Department, int Headcount)> HeadcountByDepartment(
        IEnumerable<Department> departments, IEnumerable<Employee> employees) =>
        (from d in departments
         join e in employees on (int?)d.Id equals e.DepartmentId into staff
         orderby d.Id
         select (d.Name, staff.Count()))
        .ToList();

    public static IReadOnlyList<string> EarnsMoreThanManager(IEnumerable<Employee> employees) =>
        (from e in employees
         join m in employees on e.ManagerId equals (int?)m.Id
         where e.Salary > m.Salary
         orderby e.Name
         select e.Name)
        .ToList();

    public static IReadOnlyList<(string Project, int Hours)> HoursByProject(
        IEnumerable<Project> projects, IEnumerable<Assignment> assignments) =>
        (from p in projects
         join a in assignments on p.Id equals a.ProjectId into work
         let total = work.Sum(a => a.Hours)
         orderby total descending, p.Name
         select (p.Name, total))
        .ToList();

    public static IReadOnlyList<string> EmployeesOnEveryProject(
        IEnumerable<Employee> employees, IReadOnlyCollection<Project> projects, IEnumerable<Assignment> assignments)
    {
        var projectIds = projects.Select(p => p.Id).ToHashSet();

        return (from e in employees
                join a in assignments on e.Id equals a.EmployeeId into work
                where projectIds.IsSubsetOf(work.Select(a => a.ProjectId))
                orderby e.Name
                select e.Name)
            .ToList();
    }

    public static RosterDiff CompareRosters(IEnumerable<string> before, IEnumerable<string> after)
    {
        var old = before.ToList();
        var current = after.ToList();

        return new RosterDiff(
            Added: current.Except(old).Order(StringComparer.Ordinal).ToList(),
            Removed: old.Except(current).Order(StringComparer.Ordinal).ToList(),
            Kept: old.Intersect(current).Order(StringComparer.Ordinal).ToList());
    }

    public static IReadOnlyList<T> Page<T>(IEnumerable<T> source, int page, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(page, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);

        return source.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }
}
