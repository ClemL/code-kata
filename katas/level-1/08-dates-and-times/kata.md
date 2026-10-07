---
title: Dates & Times
level: 1
minutes: 25
summary: DateOnly, DateTimeOffset, TimeSpan formatting, exact parsing, and the calendar arithmetic that reporting code needs.
topics: [Dates and times, Parsing and formatting, Reporting]
keywords: [struct, static, "?"]
apis: [DateOnly, DateTimeOffset, TimeSpan, DateOnly.TryParseExact, CultureInfo.InvariantCulture, DateTime.DaysInMonth]
docs:
  - title: How to use DateOnly and TimeOnly
    url: https://learn.microsoft.com/dotnet/standard/datetime/how-to-use-dateonly-timeonly
  - title: Choose between DateTime, DateOnly, DateTimeOffset, TimeSpan, TimeOnly and TimeZoneInfo
    url: https://learn.microsoft.com/dotnet/standard/datetime/choosing-between-datetime
  - title: Custom date and time format strings
    url: https://learn.microsoft.com/dotnet/standard/base-types/custom-date-and-time-format-strings
  - title: Custom TimeSpan format strings
    url: https://learn.microsoft.com/dotnet/standard/base-types/custom-timespan-format-strings
---

## Brief

`DateOnly` and `TimeOnly` (.NET 6) finally separate "a calendar date" from "an instant in time". `DateTimeOffset` remains the right type for instants that cross systems, and `DateTime` is best avoided in new APIs. This kata covers the arithmetic that shows up in every reporting or claims pipeline: ages, business days, month ends, quarters, and strict ISO parsing.

## Tasks

Implement the static methods on `DateKata`:

1. **`AgeOn(DateOnly birthDate, DateOnly onDate)`** returns full years. People born on 29 February have their birthday on **1 March** in non-leap years. Throw `ArgumentOutOfRangeException` if `onDate` is before `birthDate`.
2. **`BusinessDaysBetween(DateOnly start, DateOnly endExclusive)`** counts Monday-Friday dates in the half-open range `[start, endExclusive)`. Throw `ArgumentException` when the range is reversed.
3. **`NextOccurrence(DateOnly from, DayOfWeek day)`** returns the next date strictly *after* `from` that falls on `day`.
4. **`EndOfMonth(DateOnly date)`** returns the last day of the month (leap years included).
5. **`QuarterOf(DateOnly date)`** returns `1`-`4`.
6. **`ParseIsoDate(string? text)`** parses exactly `yyyy-MM-dd` (culture-invariant) and returns `null` for anything else, including impossible dates like `2026-02-30`.
7. **`IsSameInstant(DateTimeOffset a, DateTimeOffset b)`** returns `true` when both values denote the same moment, even with different offsets.
8. **`ToUtcStamp(DateTimeOffset value)`** formats the instant in UTC as `yyyy-MM-ddTHH:mm:ssZ`.
9. **`FormatDuration(TimeSpan span)`** returns `"01h 02m 05s"`, prefixed with `"Nd "` when the span is at least one day (`"1d 01h 01m 01s"`). Negative spans throw `ArgumentOutOfRangeException`.

## Hints

- `onDate.Year - birthDate.Year`, then subtract one if `birthDate` is later than `onDate.AddYears(-age)`. `AddYears` clamps 29 February to 28 February, which gives the 1 March rule for free.
- `DateOnly` supports `AddDays`, comparison operators, and `DayNumber` (days since 0001-01-01). Iterating day by day is acceptable here; a closed-form count is a good stretch goal.
- Days until the target weekday: `((int)day - (int)from.DayOfWeek + 7) % 7`, with `0` meaning "a full week".
- `DateTime.DaysInMonth(year, month)` knows about leap years.
- `DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)`.
- `DateTimeOffset` equality compares instants; `EqualsExact` also compares offsets.
- Quote literal characters in custom formats: `"yyyy-MM-dd'T'HH:mm:ss'Z'"`. In `TimeSpan` formats, escape them: `@"hh\h\ mm\m\ ss\s"`.

## Notes

- **Store instants as `DateTimeOffset` (or UTC `DateTime`), store calendar dates as `DateOnly`.** A date of birth or a fill date is not an instant; converting it through a time zone can shift it by a day.
- **`DateTime.Now` in business logic is a testability smell.** .NET 8's `TimeProvider` abstraction fixes that; it is used in Level 5's *Rate Limiter* kata.
- **Time zones.** `TimeZoneInfo.FindSystemTimeZoneById` accepts both IANA (`America/New_York`) and Windows (`Eastern Standard Time`) IDs on .NET 6+ when ICU is available.
- **`yyyy` vs `YYYY`.** .NET has no `Y` specifier; `YYYY` is copied as literal text. `mm` is minutes and `MM` is months.
