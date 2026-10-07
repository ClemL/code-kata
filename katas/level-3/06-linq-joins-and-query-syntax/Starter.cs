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
        throw new NotImplementedException();

    public static IReadOnlyList<(string Employee, string Department)> EmployeeDepartmentsOrNone(
        IEnumerable<Employee> employees, IEnumerable<Department> departments) =>
        throw new NotImplementedException();

    public static IReadOnlyList<(string Department, int Headcount)> HeadcountByDepartment(
        IEnumerable<Department> departments, IEnumerable<Employee> employees) =>
        throw new NotImplementedException();

    public static IReadOnlyList<string> EarnsMoreThanManager(IEnumerable<Employee> employees) =>
        throw new NotImplementedException();

    public static IReadOnlyList<(string Project, int Hours)> HoursByProject(
        IEnumerable<Project> projects, IEnumerable<Assignment> assignments) =>
        throw new NotImplementedException();

    public static IReadOnlyList<string> EmployeesOnEveryProject(
        IEnumerable<Employee> employees, IReadOnlyCollection<Project> projects, IEnumerable<Assignment> assignments) =>
        throw new NotImplementedException();

    public static RosterDiff CompareRosters(IEnumerable<string> before, IEnumerable<string> after) =>
        throw new NotImplementedException();

    public static IReadOnlyList<T> Page<T>(IEnumerable<T> source, int page, int pageSize) =>
        throw new NotImplementedException();
}
