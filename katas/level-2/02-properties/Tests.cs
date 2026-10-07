using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L2.Properties;

[TestClass]
public class PropertiesTests
{
    [TestMethod]
    public void Temperature_ConvertsBetweenScales()
    {
        var t = new Temperature { Celsius = 100 };
        Assert.AreEqual(212, t.Fahrenheit, 1e-9);
        Assert.AreEqual(373.15, t.Kelvin, 1e-9);

        t.Fahrenheit = 32;
        Assert.AreEqual(0, t.Celsius, 1e-9);
    }

    [TestMethod]
    public void Temperature_RejectsBelowAbsoluteZero()
    {
        var t = new Temperature();
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => t.Celsius = -300);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => t.Fahrenheit = -500);
        Assert.AreEqual(0, t.Celsius, "A rejected value must not be stored.");

        t.Celsius = Temperature.AbsoluteZero;
        Assert.AreEqual(0, t.Kelvin, 1e-9);
    }

    [TestMethod]
    public void UserProfile_NormalizesEmail()
    {
        var profile = new UserProfile { Email = "  Ada@Example.COM " };
        Assert.AreEqual("ada@example.com", profile.Email);
    }

    [TestMethod]
    public void UserProfile_RejectsBlankEmail() =>
        Assert.ThrowsExactly<ArgumentException>(() => new UserProfile { Email = "   " });

    [TestMethod]
    public void UserProfile_DisplayNameFallsBackToEmail()
    {
        Assert.AreEqual("grace@example.com", new UserProfile { Email = "grace@example.com" }.DisplayName);
        Assert.AreEqual("Grace", new UserProfile { Email = "grace@example.com", DisplayName = "Grace" }.DisplayName);
    }

    [TestMethod]
    public void Grid_SupportsTwoDimensionalIndexer()
    {
        var grid = new Grid(2, 3);
        Assert.AreEqual(2, grid.Rows);
        Assert.AreEqual(3, grid.Columns);

        grid[1, 2] = 42;
        grid[0, 0] = 7;

        Assert.AreEqual(42, grid[1, 2]);
        Assert.AreEqual(7, grid[0, 0]);
        Assert.AreEqual(0, grid[1, 0]);
    }

    [TestMethod]
    public void Grid_ValidatesCoordinates()
    {
        var grid = new Grid(2, 3);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => grid[2, 0]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => grid[0, 3]);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => grid[-1, 0] = 1);
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => grid[0, 3] = 1, "Column 3 must not wrap into row 1.");
    }

    [TestMethod]
    public void Document_CachesWordCountUntilTextChanges()
    {
        var doc = new Document();
        Assert.AreEqual(0, doc.WordCount);

        doc.Text = "the quick  brown\tfox";
        Assert.AreEqual(4, doc.WordCount);
        Assert.AreEqual(4, doc.WordCount);
        Assert.AreEqual(2, doc.Computations, "One computation for the empty text, one for the new text.");

        doc.Text = "jumps";
        Assert.AreEqual(1, doc.WordCount);
        Assert.AreEqual(3, doc.Computations);
    }
}
