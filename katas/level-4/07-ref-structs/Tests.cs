using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L4.RefStructs;

[TestClass]
public class RefStructTests
{
    [TestMethod]
    public void Largest_ReturnsAWritableReference()
    {
        int[] scores = [40, 95, 70];

        RefKata.Largest(scores) = 0;
        CollectionAssert.AreEqual(new[] { 40, 0, 70 }, scores);

        ref var top = ref RefKata.Largest(scores);
        top += 5;
        CollectionAssert.AreEqual(new[] { 40, 0, 75 }, scores);
    }

    [TestMethod]
    public void Largest_RejectsEmptyInput() =>
        Assert.ThrowsExactly<ArgumentException>(() => RefKata.Largest([]));

    [TestMethod]
    public void CountChars_CountsInPlace()
    {
        var counts = RefKata.CountChars("mississippi");

        Assert.AreEqual(1, counts['m']);
        Assert.AreEqual(4, counts['i']);
        Assert.AreEqual(4, counts['s']);
        Assert.AreEqual(2, counts['p']);
        Assert.HasCount(4, counts);
    }

    [TestMethod]
    public void Tokenizer_SupportsForeachWithoutAllocatingTokens()
    {
        var tokens = new List<string>();
        foreach (var token in new Tokenizer(",alpha,,beta,gamma,", ','))
        {
            tokens.Add(token.ToString());
        }

        CollectionAssert.AreEqual(new[] { "alpha", "beta", "gamma" }, tokens);
    }

    [TestMethod]
    public void Tokenizer_HandlesEdgeCases()
    {
        var count = 0;
        foreach (var _ in new Tokenizer(",,,", ','))
        {
            count++;
        }

        Assert.AreEqual(0, count);

        var single = new List<string>();
        foreach (var token in new Tokenizer("solo", ' '))
        {
            single.Add(token.ToString());
        }

        CollectionAssert.AreEqual(new[] { "solo" }, single);
    }

    [TestMethod]
    public void FixedStringBuilder_AppendsUntilFull()
    {
        Span<char> buffer = stackalloc char[8];
        var builder = new FixedStringBuilder(buffer);

        Assert.IsTrue(builder.Append("RX-"));
        Assert.IsTrue(builder.Append('4'));
        Assert.IsTrue(builder.Append("2"));
        Assert.AreEqual(5, builder.Length);
        Assert.AreEqual("RX-42", builder.ToString());

        Assert.IsFalse(builder.Append("TOOLONG"), "Appending past the end must fail without writing.");
        Assert.AreEqual("RX-42", builder.ToString());

        Assert.IsTrue(builder.Append("abc"));
        Assert.IsFalse(builder.Append('!'));
        Assert.AreEqual("RX-42abc", builder.ToString());
    }

    [TestMethod]
    public void Describe_AcceptsARefStructThroughAnInterfaceConstraint()
    {
        ReadOnlySpan<int> data = [3, 4, 5];
        Assert.AreEqual("3 items, sum 12", RefKata.Describe(new SpanWindow(data)));
        Assert.AreEqual("0 items, sum 0", RefKata.Describe(new SpanWindow([])));
    }
}
