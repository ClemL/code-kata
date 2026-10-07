using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L2.Polymorphism;

[TestClass]
public class PolymorphismTests
{
    [TestMethod]
    public void Rectangle_ComputesAreaAndPerimeter()
    {
        var r = new Rectangle(3, 4);
        Assert.AreEqual(12, r.Area, 1e-9);
        Assert.AreEqual(14, r.Perimeter, 1e-9);
        Assert.AreEqual("Rectangle", r.Name);
        Assert.HasCount(4, r.Sides);
    }

    [TestMethod]
    public void Square_IsARectangleWithItsOwnName()
    {
        Rectangle shape = new Square(2);
        Assert.AreEqual("Square", shape.Name, "Name is virtual, so the override runs through a base reference.");
        Assert.AreEqual(4, shape.Area, 1e-9);
        Assert.AreEqual(8, shape.Perimeter, 1e-9);
    }

    [TestMethod]
    public void Square_IsSealed() =>
        Assert.IsTrue(typeof(Square).IsSealed, "Nothing should derive from Square.");

    [TestMethod]
    public void Triangle_UsesHeronsFormula()
    {
        var t = new Triangle(3, 4, 5);
        Assert.AreEqual(6, t.Area, 1e-9);
        Assert.AreEqual(12, t.Perimeter, 1e-9);
        Assert.AreEqual("Triangle", t.Name);
    }

    [TestMethod]
    public void Triangle_RejectsImpossibleSides() =>
        Assert.ThrowsExactly<ArgumentException>(() => new Triangle(1, 2, 10));

    [TestMethod]
    public void Circle_IsAShapeButNotAPolygon()
    {
        IShape c = new Circle(1);
        Assert.AreEqual(Math.PI, c.Area, 1e-9);
        Assert.AreEqual(2 * Math.PI, c.Perimeter, 1e-9);
        Assert.AreEqual("Circle", c.Name);
        Assert.IsNotInstanceOfType<Polygon>(c);
    }

    [TestMethod]
    public void Constructors_ValidateDimensions()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Rectangle(0, 1));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Square(-2));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new Circle(0));
    }

    [TestMethod]
    public void Describe_IsADefaultInterfaceMethod()
    {
        IShape square = new Square(2);
        Assert.AreEqual("Square: area 4.00, perimeter 8.00", square.Describe());

        IShape circle = new Circle(1);
        Assert.AreEqual("Circle: area 3.14, perimeter 6.28", circle.Describe());
    }

    [TestMethod]
    public void ShapeSet_SupportsCollectionInitializerAndForeach()
    {
        var set = new ShapeSet { new Circle(1), new Square(3), new Triangle(3, 4, 5) };

        var names = new List<string>();
        foreach (var shape in set)
        {
            names.Add(shape.Name);
        }

        CollectionAssert.AreEqual(new[] { "Circle", "Square", "Triangle" }, names);
        Assert.AreEqual(Math.PI + 9 + 6, set.TotalArea, 1e-9);
        Assert.AreEqual("Square", set.Largest()?.Name);
    }

    [TestMethod]
    public void ShapeSet_LargestIsNullWhenEmpty() =>
        Assert.IsNull(new ShapeSet().Largest());

    [TestMethod]
    public void ShapeSet_WorksThroughNonGenericInterface()
    {
        System.Collections.IEnumerable untyped = new ShapeSet { new Square(1) };
        var count = 0;
        foreach (var _ in untyped)
        {
            count++;
        }

        Assert.AreEqual(1, count);
    }
}
