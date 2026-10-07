using System.Buffers;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Katas.L4.Pooling;

public static class BufferKata
{
    private const uint AdlerModulus = 65521;
    private static readonly SearchValues<char> HtmlSpecial = SearchValues.Create("<>&\"'");

    public static uint Adler32(Stream stream, ArrayPool<byte> pool, int bufferSize = 4096)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        var buffer = pool.Rent(bufferSize);
        try
        {
            uint a = 1, b = 0;
            int read;
            while ((read = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                foreach (var value in buffer.AsSpan(0, read))
                {
                    a = (a + value) % AdlerModulus;
                    b = (b + a) % AdlerModulus;
                }
            }

            return (b << 16) | a;
        }
        finally
        {
            pool.Return(buffer);
        }
    }

    public static string FormatClaimId(char prefix, int year, int sequence)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(year, 1000);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(year, 9999);
        ArgumentOutOfRangeException.ThrowIfNegative(sequence);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sequence, 999_999);

        return string.Create(13, (prefix, year, sequence), static (span, state) =>
        {
            span[0] = state.prefix;
            span[1] = '-';
            state.year.TryFormat(span[2..6], out _, default, CultureInfo.InvariantCulture);
            span[6] = '-';
            state.sequence.TryFormat(span[7..], out _, "D6", CultureInfo.InvariantCulture);
        });
    }

    public static void WriteUtf8Lines(IBufferWriter<byte> writer, IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            Encoding.UTF8.GetBytes(line, writer);
            writer.Write("\n"u8);
        }
    }

    public static string EscapeHtml(string text)
    {
        var first = text.AsSpan().IndexOfAny(HtmlSpecial);
        if (first < 0)
        {
            return text;
        }

        var builder = new StringBuilder(text.Length + 16);
        builder.Append(text.AsSpan(0, first));

        foreach (var c in text.AsSpan(first))
        {
            switch (c)
            {
                case '<': builder.Append("&lt;"); break;
                case '>': builder.Append("&gt;"); break;
                case '&': builder.Append("&amp;"); break;
                case '"': builder.Append("&quot;"); break;
                case '\'': builder.Append("&#39;"); break;
                default: builder.Append(c); break;
            }
        }

        return builder.ToString();
    }

    public static long Sum(List<int> values)
    {
        long total = 0;
        foreach (var value in CollectionsMarshal.AsSpan(values))
        {
            total += value;
        }

        return total;
    }
}
