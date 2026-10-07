using System.Globalization;

namespace Katas.L4.Spans;

public static class SpanKata
{
    public static int SumCsv(ReadOnlySpan<char> line)
    {
        if (line.IsWhiteSpace())
        {
            return 0;
        }

        var total = 0;
        foreach (var range in line.Split(','))
        {
            total += int.Parse(line[range], NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        return total;
    }

    public static void ReverseInPlace(Span<int> values)
    {
        for (int left = 0, right = values.Length - 1; left < right; left++, right--)
        {
            (values[left], values[right]) = (values[right], values[left]);
        }
    }

    public static int CountWords(ReadOnlySpan<char> text)
    {
        var count = 0;
        var inWord = false;

        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                inWord = false;
            }
            else if (!inWord)
            {
                inWord = true;
                count++;
            }
        }

        return count;
    }

    public static bool TryFormatHex(uint value, Span<char> destination, out int charsWritten)
    {
        charsWritten = 0;
        if (destination.Length < 10)
        {
            return false;
        }

        "0x".CopyTo(destination);
        if (!value.TryFormat(destination[2..], out var digits, "X8", CultureInfo.InvariantCulture))
        {
            return false;
        }

        charsWritten = 2 + digits;
        return true;
    }

    public static bool TryParseKeyValue(ReadOnlySpan<char> input, out ReadOnlySpan<char> key, out ReadOnlySpan<char> value)
    {
        var separator = input.IndexOf('=');
        if (separator < 0)
        {
            key = value = default;
            return false;
        }

        key = input[..separator].Trim();
        value = input[(separator + 1)..].Trim();
        return !key.IsEmpty;
    }

    public static string DigitsOnly(ReadOnlySpan<char> input)
    {
        Span<char> buffer = input.Length <= 256 ? stackalloc char[input.Length] : new char[input.Length];

        var count = 0;
        foreach (var c in input)
        {
            if (char.IsAsciiDigit(c))
            {
                buffer[count++] = c;
            }
        }

        return new string(buffer[..count]);
    }
}
