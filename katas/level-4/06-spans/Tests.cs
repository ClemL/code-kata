using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L4.Spans;

[TestClass]
public class SpanTests
{
    // Runs the action once to warm up (JIT, static constructors), then measures a second run.
    private static long AllocatedBytes(Action action)
    {
        action();
        var before = GC.GetAllocatedBytesForCurrentThread();
        action();
        return GC.GetAllocatedBytesForCurrentThread() - before;
    }

    [TestMethod]
    public void SumCsv_ParsesAndTrims()
    {
        Assert.AreEqual(6, SpanKata.SumCsv(" 1, 2,3 "));
        Assert.AreEqual(-5, SpanKata.SumCsv("10,-15"));
        Assert.AreEqual(42, SpanKata.SumCsv("42"));
        Assert.AreEqual(0, SpanKata.SumCsv("   "));
        Assert.ThrowsExactly<FormatException>(() => SpanKata.SumCsv("1,two,3"));
    }

    [TestMethod]
    public void SumCsv_DoesNotAllocate()
    {
        var result = 0;
        Assert.AreEqual(0L, AllocatedBytes(() => result = SpanKata.SumCsv("100, 200, 300, 400")));
        Assert.AreEqual(1000, result);
    }

    [TestMethod]
    public void ReverseInPlace_ReversesWithoutAllocating()
    {
        int[] data = [1, 2, 3, 4, 5];
        SpanKata.ReverseInPlace(data);
        CollectionAssert.AreEqual(new[] { 5, 4, 3, 2, 1 }, data);

        SpanKata.ReverseInPlace(data.AsSpan(1, 3));
        CollectionAssert.AreEqual(new[] { 5, 2, 3, 4, 1 }, data);

        Assert.AreEqual(0L, AllocatedBytes(() => SpanKata.ReverseInPlace(data)));
    }

    [TestMethod]
    public void CountWords_CountsRuns()
    {
        Assert.AreEqual(4, SpanKata.CountWords("  the quick\tbrown\nfox "));
        Assert.AreEqual(0, SpanKata.CountWords("   "));
        Assert.AreEqual(1, SpanKata.CountWords("word"));
        Assert.AreEqual(0L, AllocatedBytes(() => SpanKata.CountWords("several words in a sentence")));
    }

    [TestMethod]
    public void TryFormatHex_WritesIntoCallerBuffer()
    {
        Span<char> buffer = stackalloc char[16];

        Assert.IsTrue(SpanKata.TryFormatHex(0xBEEF, buffer, out var written));
        Assert.AreEqual("0x0000BEEF", buffer[..written].ToString());

        Assert.IsFalse(SpanKata.TryFormatHex(1, buffer[..9], out written));
        Assert.AreEqual(0, written);

        var heapBuffer = new char[10];
        Assert.AreEqual(0L, AllocatedBytes(() => SpanKata.TryFormatHex(uint.MaxValue, heapBuffer, out _)));
        Assert.AreEqual("0xFFFFFFFF", new string(heapBuffer));
    }

    [TestMethod]
    public void TryParseKeyValue_SlicesWithoutCopying()
    {
        Assert.IsTrue(SpanKata.TryParseKeyValue("  timeout = 30 ", out var key, out var value));
        Assert.AreEqual("timeout", key.ToString());
        Assert.AreEqual("30", value.ToString());

        Assert.IsTrue(SpanKata.TryParseKeyValue("url=https://x.test/?a=b", out key, out value));
        Assert.AreEqual("url", key.ToString());
        Assert.AreEqual("https://x.test/?a=b", value.ToString(), "Split on the first '=' only.");

        Assert.IsTrue(SpanKata.TryParseKeyValue("empty=", out _, out value));
        Assert.IsTrue(value.IsEmpty);

        Assert.IsFalse(SpanKata.TryParseKeyValue("no separator", out _, out _));
        Assert.IsFalse(SpanKata.TryParseKeyValue(" = value", out _, out _));

        Assert.AreEqual(0L, AllocatedBytes(() => SpanKata.TryParseKeyValue(" retries = 5 ", out _, out _)));
    }

    [TestMethod]
    public void DigitsOnly_AllocatesOnlyTheResult()
    {
        Assert.AreEqual("6175550100", SpanKata.DigitsOnly("(617) 555-0100"));
        Assert.AreEqual("", SpanKata.DigitsOnly("no digits"));
        Assert.AreEqual(new string('7', 500), SpanKata.DigitsOnly(new string('7', 500)), "Inputs above 256 characters still work.");

        var bytes = AllocatedBytes(() => SpanKata.DigitsOnly("(617) 555-0100"));
        Assert.IsLessThanOrEqualTo(64L, bytes, $"Expected only the 10-character result string, saw {bytes} bytes.");
    }
}
