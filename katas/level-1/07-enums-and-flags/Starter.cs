namespace Katas.L1.Enums;

// TODO: This enum is broken: Read | Write == Delete. Fix it.
public enum Permission
{
    None,
    Read,
    Write,
    Delete,
    Share,
    Admin,
}

public enum TicketStatus
{
    Open,
    InProgress,
    Blocked,
    Resolved,
    Closed,
}

public static class Access
{
    public static Permission Grant(Permission current, Permission add) => throw new NotImplementedException();

    public static Permission Revoke(Permission current, Permission remove) => throw new NotImplementedException();

    public static bool HasAll(Permission current, Permission required) => throw new NotImplementedException();

    public static bool HasAny(Permission current, Permission candidates) => throw new NotImplementedException();

    public static bool TryParse(string text, out Permission value) => throw new NotImplementedException();
}

public static class Workflow
{
    public static bool CanTransition(TicketStatus from, TicketStatus to) => throw new NotImplementedException();

    public static IReadOnlyList<TicketStatus> ActiveStatuses() => throw new NotImplementedException();
}
