using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L1.Strings;

[TestClass]
public class StringWorkshopTests
{
    [TestMethod]
    public void Reverse_HandlesAscii()
    {
        Assert.AreEqual("cba", Text.Reverse("abc"));
        Assert.AreEqual("", Text.Reverse(""));
    }

    [TestMethod]
    public void Reverse_KeepsEmojiWithModifierTogether()
    {
        var thumbsUpMediumSkin = "\U0001F44D\U0001F3FD";
        Assert.AreEqual(thumbsUpMediumSkin + "ko", Text.Reverse("ok" + thumbsUpMediumSkin));
    }

    [TestMethod]
    public void Reverse_KeepsCombiningMarksAttached()
    {
        // "cafe" + COMBINING ACUTE ACCENT renders as "café".
        Assert.AreEqual("éfac", Text.Reverse("café"));
    }

    [TestMethod]
    [DataRow("A man, a plan, a canal: Panama", true)]
    [DataRow("No 'x' in Nixon", true)]
    [DataRow("racecar", true)]
    [DataRow("", true)]
    [DataRow("!!", true)]
    [DataRow("hello", false)]
    [DataRow("ab1ba2", false)]
    public void IsPalindrome_IgnoresCaseAndPunctuation(string input, bool expected) =>
        Assert.AreEqual(expected, Text.IsPalindrome(input));

    [TestMethod]
    [DataRow("  ada   king lovelace ", "AKL")]
    [DataRow("grace hopper", "GH")]
    [DataRow("linus", "L")]
    [DataRow("   ", "")]
    public void Initials_HandlesIrregularSpacing(string name, string expected) =>
        Assert.AreEqual(expected, Text.Initials(name));

    [TestMethod]
    [DataRow("Hello, World! C# Rocks", "hello-world-c-rocks")]
    [DataRow("Top 10 .NET Tips (2026)", "top-10-net-tips-2026")]
    [DataRow("  --Already-Slugged--  ", "already-slugged")]
    [DataRow("???", "")]
    public void Slugify_CollapsesSeparators(string title, string expected) =>
        Assert.AreEqual(expected, Text.Slugify(title));

    [TestMethod]
    public void CountOccurrences_IsCaseInsensitiveAndNonOverlapping()
    {
        Assert.AreEqual(2, Text.CountOccurrences("The cat sat on the CAT mat", "cat"));
        Assert.AreEqual(2, Text.CountOccurrences("aaaa", "aa"));
        Assert.AreEqual(0, Text.CountOccurrences("dog", "cat"));
    }

    [TestMethod]
    public void CountOccurrences_RejectsEmptyWord() =>
        Assert.ThrowsExactly<ArgumentException>(() => Text.CountOccurrences("abc", ""));

    [TestMethod]
    public void FormatReceipt_AlignsColumns()
    {
        var expected = """
            Item          Price
            -------------------
            Coffee         3.50
            Bagel          2.25
            -------------------
            Total          5.75
            """.ReplaceLineEndings("\n");

        var actual = Text.FormatReceipt([("Coffee", 3.5m), ("Bagel", 2.25m)]);

        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void FormatReceipt_IsCultureInvariant()
    {
        var original = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
            var actual = Text.FormatReceipt([("Tea", 1234.5m)]);
            StringAssert.Contains(actual, "1234.50");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = original;
        }
    }
}
