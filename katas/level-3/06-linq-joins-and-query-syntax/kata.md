---
title: "LINQ: Joins & Query Syntax"
level: 3
minutes: 35
summary: Inner joins, left joins, group joins, self-joins, let clauses, relational division, set operations, and paging, mostly in query syntax.
topics: [LINQ, Joins, Query syntax, Set operations]
keywords: [from, join, on, equals, into, let, group, by, select, where, orderby]
apis: [Join, GroupJoin, DefaultIfEmpty, LeftJoin, SelectMany, Except, Intersect, Skip, Take]
docs:
  - title: Query keywords
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/query-keywords
  - title: Join operations
    url: https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/join-operations
  - title: Set operations
    url: https://learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/set-operations
---

## Brief

Query syntax (`from ... join ... select`) is worth knowing even if you prefer method chains: joins, `let`, and group joins read much more naturally in it, and the compiler translates it mechanically into `Join`, `GroupJoin`, `SelectMany`, and `Select` calls. If you write SQL every day, this kata maps directly onto `INNER JOIN`, `LEFT JOIN`, self-joins, and `EXCEPT`/`INTERSECT`.

## Tasks

Implement the static methods on `OrgQueries` using the `Department`, `Employee`, `Project`, and `Assignment` records. Order results as stated.

1. **`EmployeeDepartments`**: inner join of employees to departments as `(Employee, Department)` name pairs, ordered by employee name. Employees without a department are excluded.
2. **`EmployeeDepartmentsOrNone`**: the same as a **left join**; employees without a department get `"(none)"`.
3. **`HeadcountByDepartment`**: every department with its employee count (including zero), ordered by department `Id`. Use a group join (`join ... into`).
4. **`EarnsMoreThanManager`**: names of employees whose salary is greater than their manager's, sorted. This is a self-join.
5. **`HoursByProject`**: total assignment hours per project name, highest first.
6. **`EmployeesOnEveryProject`**: names of employees assigned to **every** project, sorted (relational division).
7. **`CompareRosters(before, after)`**: names `Added`, `Removed`, and `Kept`, each sorted.
8. **`Page<T>(source, page, pageSize)`**: the 1-based `page` of results. Invalid arguments throw `ArgumentOutOfRangeException`.

## Hints

- Join keys must have the same type. `Employee.DepartmentId` is `int?` and `Department.Id` is `int`, so write `on e.DepartmentId equals (int?)d.Id`.
- Left join in query syntax: `join d in departments on ... into matches from d in matches.DefaultIfEmpty() select ... d?.Name ?? "(none)"`. .NET 10 also added a `LeftJoin` method.
- Group join: `from d in departments join e in employees on (int?)d.Id equals e.DepartmentId into staff select (d.Name, staff.Count())`.
- Self-join: `from e in employees join m in employees on e.ManagerId equals (int?)m.Id where e.Salary > m.Salary select e.Name`.
- `let` names an intermediate value inside a query: `let total = g.Sum(a => a.Hours)`.
- Relational division: group assignments by employee, then keep groups whose distinct project count equals the number of projects.

## Notes

- **What the compiler generates.** `from a in xs from b in ys select f(a, b)` becomes `xs.SelectMany(a => ys, (a, b) => f(a, b))`. A `let` becomes a `Select` into an anonymous type. Query syntax is pure syntactic sugar over method calls, which is why any type with the right method shapes can support it (see Level 5's *Option & LINQ* kata).
- **`Join` is a hash join.** It builds a lookup from the inner sequence, so it is O(n + m), not O(n × m). A nested `from ... from ... where a.Id == b.Id` is O(n × m).
- **Null keys never match** in `Join`/`GroupJoin`, which mirrors SQL semantics.
- **`Except` and `Intersect` are set operations:** they remove duplicates from the result. Use `ExceptBy`/`IntersectBy` (.NET 6) to compare by key.
