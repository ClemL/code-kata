using System.Collections;
using System.Globalization;

namespace Katas.L2.Polymorphism;

public interface IShape
{
    string Name { get; }

    double Area { get; }

    double Perimeter { get; }

    string Describe() =>
        string.Create(CultureInfo.InvariantCulture, $"{Name}: area {Area:F2}, perimeter {Perimeter:F2}");
}

public abstract class Polygon : IShape
{
    public abstract IReadOnlyList<double> Sides { get; }

    public abstract double Area { get; }

    public double Perimeter => Sides.Sum();

    public virtual string Name => GetType().Name;
}

public class Rectangle : Polygon
{
    public Rectangle(double width, double height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Width = width;
        Height = height;
    }

    public double Width { get; }

    public double Height { get; }

    public override IReadOnlyList<double> Sides => [Width, Height, Width, Height];

    public override double Area => Width * Height;
}

public sealed class Square(double side) : Rectangle(side, side)
{
    public sealed override string Name => "Square";
}

public sealed class Triangle : Polygon
{
    private readonly double[] _sides;

    public Triangle(double a, double b, double c)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(a);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(b);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(c);

        if (a + b <= c || a + c <= b || b + c <= a)
        {
            throw new ArgumentException("The sides violate the triangle inequality.");
        }

        _sides = [a, b, c];
    }

    public override IReadOnlyList<double> Sides => _sides;

    public override double Area
    {
        get
        {
            var (a, b, c) = (_sides[0], _sides[1], _sides[2]);
            var s = (a + b + c) / 2;
            return Math.Sqrt(s * (s - a) * (s - b) * (s - c));
        }
    }
}

public sealed class Circle : IShape
{
    public Circle(double radius)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(radius);
        Radius = radius;
    }

    public double Radius { get; }

    public string Name => "Circle";

    public double Area => Math.PI * Radius * Radius;

    public double Perimeter => 2 * Math.PI * Radius;
}

public sealed class ShapeSet : IEnumerable<IShape>
{
    private readonly List<IShape> _shapes = [];

    public void Add(IShape shape)
    {
        ArgumentNullException.ThrowIfNull(shape);
        _shapes.Add(shape);
    }

    public double TotalArea => _shapes.Sum(shape => shape.Area);

    public IShape? Largest() => _shapes.MaxBy(shape => shape.Area);

    public IEnumerator<IShape> GetEnumerator() => _shapes.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
