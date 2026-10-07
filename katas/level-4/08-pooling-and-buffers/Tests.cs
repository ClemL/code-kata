using System.Buffers;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L4.Pooling;

[TestClass]
public class PoolingAndBuffersTests
{
    private sealed class TrackingPool : ArrayPool<byte>
    {
        public int Rented { get; private set; }

        public int Returned { get; private set; }

        public override byte[] Rent(int minimumLength)
        {
            Rented++;
            var array = new byte[minimumLength + 7]; // Pools may hand out larger arrays...
            Array.Fill(array, (byte)0xFF);           // ...that still hold old data.
            return array;
        }

        public override void Return(byte[] array, bool clearArray = false) => Returned++;
    }

    private sealed class FailingStream() : MemoryStream([1, 2, 3, 4, 5, 6, 7, 8])
    {
        private int _reads;

        public override int Read(byte[] buffer, int offset, int count) =>
            ++_reads > 1 ? throw new IOException("disk on fire") : base.Read(buffer, offset, count);
    }

    [TestMethod]
    public void Adler32_MatchesKnownValues()
    {
        var pool = new TrackingPool();

        Assert.AreEqual(0x11E60398u, BufferKata.Adler32(new MemoryStream("Wikipedia"u8.ToArray()), pool, bufferSize: 4));
        Assert.AreEqual(1u, BufferKata.Adler32(new MemoryStream(), pool));
        Assert.AreEqual(2, pool.Rented, "One rental per call.");
        Assert.AreEqual(pool.Rented, pool.Returned);
    }

    [TestMethod]
    public void Adler32_ReturnsBufferWhenStreamFails()
    {
        var pool = new TrackingPool();

        Assert.ThrowsExactly<IOException>(() => BufferKata.Adler32(new FailingStream(), pool, bufferSize: 4));
        Assert.AreEqual(1, pool.Rented);
        Assert.AreEqual(1, pool.Returned);
    }

    [TestMethod]
    public void Adler32_WorksWithSharedPool()
    {
        var data = Enumerable.Range(0, 10_000).Select(i => (byte)(i * 31)).ToArray();
        var expected = BufferKata.Adler32(new MemoryStream(data), new TrackingPool(), bufferSize: 1000);
        Assert.AreEqual(expected, BufferKata.Adler32(new MemoryStream(data), ArrayPool<byte>.Shared));
    }

    [TestMethod]
    public void FormatClaimId_PadsAndValidates()
    {
        Assert.AreEqual("R-2026-000042", BufferKata.FormatClaimId('R', 2026, 42));
        Assert.AreEqual("X-1999-999999", BufferKata.FormatClaimId('X', 1999, 999_999));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => BufferKata.FormatClaimId('R', 999, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => BufferKata.FormatClaimId('R', 2026, 1_000_000));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => BufferKata.FormatClaimId('R', 2026, -1));
    }

    [TestMethod]
    public void WriteUtf8Lines_EncodesIntoTheWriter()
    {
        var writer = new ArrayBufferWriter<byte>();
        BufferKata.WriteUtf8Lines(writer, ["plain", "café", "€5"]);

        CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("plain\ncafé\n€5\n"), writer.WrittenSpan.ToArray());
        Assert.AreEqual(17, writer.WrittenCount, "é is 2 bytes and € is 3 bytes in UTF-8.");
    }

    [TestMethod]
    public void EscapeHtml_EscapesSpecialCharacters()
    {
        Assert.AreEqual("a &lt; b &amp;&amp; c &gt; d", BufferKata.EscapeHtml("a < b && c > d"));
        Assert.AreEqual("&quot;quoted&quot; &#39;single&#39;", BufferKata.EscapeHtml("\"quoted\" 'single'"));
    }

    [TestMethod]
    public void EscapeHtml_ReturnsSameInstanceWhenNothingToEscape()
    {
        var clean = string.Concat("nothing ", "to escape here");
        Assert.AreSame(clean, BufferKata.EscapeHtml(clean));
    }

    [TestMethod]
    public void Sum_IteratesTheBackingArray()
    {
        var values = Enumerable.Range(1, 100_000).ToList();
        Assert.AreEqual(5_000_050_000L, BufferKata.Sum(values));
        Assert.AreEqual(0L, BufferKata.Sum([]));
    }
}
