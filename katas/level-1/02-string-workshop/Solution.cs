using System.Globalization;
using System.Text;

namespace Katas.L1.Strings;

public static class Text
{
    public static string Reverse(string s)
    {
        ArgumentNullException.ThrowIfNull(s);

        var elements = new List<string>();
        var enumerator = StringInfo.GetTextElementEnumerator(s);
        while (enumerator.MoveNext())
        {
            elements.Add(enumerator.GetTextElement());
        }

        elements.Reverse();
        return string.Concat(elements);
    }

    public static bool IsPalindrome(string s)
    {
        int left = 0, right = s.Length - 1;
        while (left < right)
        {
            if (!char.IsLetterOrDigit(s[left])) { left++; continue; }
            if (!char.IsLetterOrDigit(s[right])) { right--; continue; }

            if (char.ToLowerInvariant(s[left]) != char.ToLowerInvariant(s[right]))
            {
                return false;
            }

            left++;
            right--;
        }

        return true;
    }

    public static string Initials(string fullName) =>
        string.Concat(fullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(part => char.ToUpperInvariant(part[0])));

    public static string Slugify(string title)
    {
        var sb = new StringBuilder(title.Length);
        var pendingDash = false;

        foreach (var c in title)
        {
            if (!char.IsAsciiLetterOrDigit(c))
            {
                pendingDash = true;
                continue;
            }

            if (pendingDash && sb.Length > 0)
            {
                sb.Append('-');
            }

            sb.Append(char.ToLowerInvariant(c));
            pendingDash = false;
        }

        return sb.ToString();
    }

    public static int CountOccurrences(string text, string word)
    {
        ArgumentException.ThrowIfNullOrEmpty(word);

        int count = 0, index = 0;
        while ((index = text.IndexOf(word, index, StringComparison.OrdinalIgnoreCase)) >= 0)
        {
            count++;
            index += word.Length;
        }

        return count;
    }

    public static string FormatReceipt(IReadOnlyList<(string Item, decimal Price)> lines)
    {
        var culture = CultureInfo.InvariantCulture;
        var rule = new string('-', 19);
        var sb = new StringBuilder();

        sb.Append(culture, $"{"Item",-12}{"Price",7}").Append('\n');
        sb.Append(rule).Append('\n');

        foreach (var (item, price) in lines)
        {
            sb.Append(culture, $"{item,-12}{price,7:F2}").Append('\n');
        }

        sb.Append(rule).Append('\n');
        sb.Append(culture, $"{"Total",-12}{lines.Sum(l => l.Price),7:F2}");

        return sb.ToString();
    }
}
