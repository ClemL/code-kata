using System.Buffers;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Katas.L4.Pooling;

public static class BufferKata
{
    public static uint Adler32(Stream stream, ArrayPool<byte> pool, int bufferSize = 4096) => throw new NotImplementedException();

    public static string FormatClaimId(char prefix, int year, int sequence) => throw new NotImplementedException();

    public static void WriteUtf8Lines(IBufferWriter<byte> writer, IEnumerable<string> lines) => throw new NotImplementedException();

    public static string EscapeHtml(string text) => throw new NotImplementedException();

    public static long Sum(List<int> values) => throw new NotImplementedException();
}
