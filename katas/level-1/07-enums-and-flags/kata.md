---
title: Enums & Flags
level: 1
minutes: 20
summary: Fix a broken [Flags] enum, then combine, test, parse, and switch on enum values safely.
topics: [Enums, Bitwise operations, State machines]
keywords: [enum, "|", "&", "~", switch, or]
apis: [FlagsAttribute, Enum.TryParse, Enum.GetValues, HasFlag]
docs:
  - title: Enumeration types
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/enum
  - title: FlagsAttribute
    url: https://learn.microsoft.com/dotnet/api/system.flagsattribute
  - title: Enum design guidelines
    url: https://learn.microsoft.com/dotnet/standard/design-guidelines/enum
---

## Brief

Enums look trivial until someone combines values that were never designed to be combined, or `Enum.TryParse` happily accepts `"42"`. The `Permission` enum in the starter is **broken on purpose**: its values are sequential, so `Read | Write` equals `Delete`. Fix the declaration first, then implement the helpers.

## Tasks

1. **Fix `Permission`.** Mark it `[Flags]`, give each permission its own bit (`None = 0`, then `1, 2, 4, 8`), and make `Admin` the combination of all four. `(Permission.Read | Permission.Write).ToString()` must return `"Read, Write"`.
2. **`Access.Grant(current, add)`** adds permissions.
3. **`Access.Revoke(current, remove)`** removes permissions without touching the others.
4. **`Access.HasAll(current, required)`** is `true` when every bit in `required` is set. `HasAll(x, Permission.None)` is `true`.
5. **`Access.HasAny(current, candidates)`** is `true` when at least one bit overlaps.
6. **`Access.TryParse(string text, out Permission value)`** accepts names only, case-insensitively, including comma-separated combinations such as `"read, write"`. It must **reject** numeric strings like `"3"` and unknown names.
7. **`Workflow.CanTransition(TicketStatus from, TicketStatus to)`** encodes this state machine with a tuple switch expression:
   - `Open → InProgress | Closed`
   - `InProgress → Blocked | Resolved`
   - `Blocked → InProgress`
   - `Resolved → Closed | InProgress` (reopen)
   - `Closed →` nothing
8. **`Workflow.ActiveStatuses()`** returns every `TicketStatus` except `Resolved` and `Closed`, in declaration order, using `Enum.GetValues<T>()`.

## Hints

- `1 << 0`, `1 << 1`, `1 << 2` makes the bit layout obvious at a glance.
- Removing bits: `current & ~remove`.
- `HasFlag` is fine for readability (the JIT has optimized it since .NET Core 2.1); `(current & required) == required` is the explicit version.
- `Enum.TryParse` accepts any integer string, including ones with no matching name. Check for digits before parsing, or validate the result against `Admin`.
- Pattern: `(from, to) switch { (TicketStatus.Open, TicketStatus.InProgress or TicketStatus.Closed) => true, ..., _ => false }`.

## Notes

- **Why `[Flags]` matters.** The attribute changes `ToString()` and `Enum.Parse` to understand combinations. It does *not* assign powers of two for you.
- **Always define `None = 0`.** A default-initialized enum field is `0`; give it a name.
- **Enums are not closed.** `(TicketStatus)42` is a legal value. A switch expression over an enum without a discard arm produces warning CS8524 for exactly that reason.
- **Prefer `Enum.GetValues<T>()`** (.NET 5) over the non-generic `Enum.GetValues(typeof(T))`, which returns an untyped `Array`.
