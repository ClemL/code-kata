---
title: String Workshop
level: 1
minutes: 25
summary: Immutable strings, StringBuilder, StringComparison, interpolation alignment, and grapheme-aware text handling.
topics: [Strings, String interpolation, Raw string literals, Globalization]
keywords: [string, char, foreach, while, continue]
apis: [StringBuilder, StringComparison, StringInfo, CultureInfo, StringSplitOptions, ReplaceLineEndings]
docs:
  - title: Best practices for comparing strings
    url: https://learn.microsoft.com/dotnet/standard/base-types/best-practices-strings
  - title: String interpolation
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/tokens/interpolated
  - title: Raw string literals
    url: https://learn.microsoft.com/dotnet/csharp/language-reference/tokens/raw-string
  - title: Character encoding (text elements)
    url: https://learn.microsoft.com/dotnet/standard/base-types/character-encoding-introduction
---

## Brief

`string` is immutable, UTF-16, and full of culture traps. The habits worth rebuilding: pass a `StringComparison` explicitly, reach for `StringBuilder` (or interpolation handlers) in loops, format with `CultureInfo.InvariantCulture` when output is machine-readable, and remember that one `char` is not one user-visible character.

## Tasks

Implement the static methods on `Text`:

1. **`Reverse(string s)`** reverses a string by *text element* (grapheme cluster), so `"ok👍🏽"` becomes `"👍🏽ko"` and an `e` followed by a combining accent stays together.
2. **`IsPalindrome(string s)`** ignores case and every character that is not a letter or digit. `"A man, a plan, a canal: Panama"` is a palindrome. Do it with two indices and no allocations.
3. **`Initials(string fullName)`** returns upper-case initials: `"  ada   king lovelace "` becomes `"AKL"`.
4. **`Slugify(string title)`** keeps ASCII letters and digits (lower-cased) and collapses every other run of characters into a single `-`, with no leading or trailing dashes. `"Top 10 .NET Tips (2026)"` becomes `"top-10-net-tips-2026"`.
5. **`CountOccurrences(string text, string word)`** counts non-overlapping, case-insensitive (ordinal) matches. `("aaaa", "aa")` returns `2`. Throw `ArgumentException` for a null or empty `word`.
6. **`FormatReceipt(IReadOnlyList<(string Item, decimal Price)> lines)`** renders a fixed-width receipt using interpolation alignment. Item column is 12 characters left-aligned, price column is 7 characters right-aligned with two decimals, rules are 19 dashes, lines are joined with `\n` (no trailing newline), and formatting is culture-invariant:

```text
Item          Price
-------------------
Coffee         3.50
Bagel          2.25
-------------------
Total          5.75
```

## Hints

- `StringInfo.GetTextElementEnumerator(s)` walks grapheme clusters; .NET 5+ follows the Unicode extended grapheme cluster rules.
- `char.IsLetterOrDigit` and `char.ToLowerInvariant` are all you need for the palindrome check.
- `Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)` handles irregular spacing.
- `char.IsAsciiLetterOrDigit` (.NET 7+) avoids accidental Unicode matches in slugs.
- `text.IndexOf(word, start, StringComparison.OrdinalIgnoreCase)` in a loop counts matches; advance by `word.Length` to avoid overlaps.
- Alignment and format share one hole: `$"{item,-12}{price,7:F2}"`. `StringBuilder.Append(IFormatProvider, $"...")` applies a culture without allocating an intermediate string.

## Notes

- **Ordinal vs culture.** Use `Ordinal` / `OrdinalIgnoreCase` for identifiers, keys, file paths, and protocol text. Culture-aware comparison is for sorting text shown to people. The Turkish-I problem (`"file".ToUpper()` in `tr-TR` is `"FİLE"`) is the classic bug.
- **`string.Reverse()` is a trap.** `new string(s.Reverse().ToArray())` splits surrogate pairs and detaches combining marks. Text elements fix that.
- **Interpolation is not just sugar.** Since C# 10, `$"..."` passed to `StringBuilder.Append` or `string.Create` uses an interpolated string *handler* and writes directly into the destination buffer.
- **Line endings.** The test uses `ReplaceLineEndings("\n")` on a raw string literal so it passes regardless of whether Git checked the file out with CRLF.
