using System.Collections;
using System.Globalization;

namespace Katas.L2.Polymorphism;

public interface IShape
{
    string Name { get; }

    double Area { get; }

    double Perimeter { get; }

    string Describe() => throw new NotImplementedException();
}

public abstract class Polygon : IShape
{
    public abstract IReadOnlyList<double> Sides { get; }

    public abstract double Area { get; }

    public double Perimeter => throw new NotImplementedException();

    public virtual string Name => throw new NotImplementedException();
}

public class Rectangle(double width, double height) : Polygon
{
    public double Width => throw new NotImplementedException();

    public double Height => throw new NotImplementedException();

    public override IReadOnlyList<double> Sides => throw new NotImplementedException();

    public override double Area => throw new NotImplementedException();
}

public class Square(double side) : Rectangle(side, side)
{
    public override string Name => throw new NotImplementedException();
}

public class Triangle(double a, double b, double c) : Polygon
{
    public override IReadOnlyList<double> Sides => throw new NotImplementedException();

    public override double Area => throw new NotImplementedException();
}

public class Circle(double radius) : IShape
{
    public double Radius => throw new NotImplementedException();

    public string Name => throw new NotImplementedException();

    public double Area => throw new NotImplementedException();

    public double Perimeter => throw new NotImplementedException();
}

public sealed class ShapeSet : IEnumerable<IShape>
{
    public void Add(IShape shape) => throw new NotImplementedException();

    public double TotalArea => throw new NotImplementedException();

    public IShape? Largest() => throw new NotImplementedException();

    public IEnumerator<IShape> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}
