namespace Katas.L1.Enums;

[Flags]
public enum Permission
{
    None = 0,
    Read = 1 << 0,
    Write = 1 << 1,
    Delete = 1 << 2,
    Share = 1 << 3,
    Admin = Read | Write | Delete | Share,
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
    public static Permission Grant(Permission current, Permission add) => current | add;

    public static Permission Revoke(Permission current, Permission remove) => current & ~remove;

    public static bool HasAll(Permission current, Permission required) => (current & required) == required;

    public static bool HasAny(Permission current, Permission candidates) => (current & candidates) != 0;

    public static bool TryParse(string text, out Permission value)
    {
        value = Permission.None;

        if (string.IsNullOrWhiteSpace(text) || text.Any(char.IsDigit))
        {
            return false;
        }

        return Enum.TryParse(text, ignoreCase: true, out value);
    }
}

public static class Workflow
{
    public static bool CanTransition(TicketStatus from, TicketStatus to) => (from, to) switch
    {
        (TicketStatus.Open, TicketStatus.InProgress or TicketStatus.Closed) => true,
        (TicketStatus.InProgress, TicketStatus.Blocked or TicketStatus.Resolved) => true,
        (TicketStatus.Blocked, TicketStatus.InProgress) => true,
        (TicketStatus.Resolved, TicketStatus.Closed or TicketStatus.InProgress) => true,
        _ => false,
    };

    public static IReadOnlyList<TicketStatus> ActiveStatuses() =>
        Enum.GetValues<TicketStatus>()
            .Where(status => status is not (TicketStatus.Resolved or TicketStatus.Closed))
            .ToArray();
}
