namespace Katas.L2.Properties;

public sealed class Temperature
{
    public const double AbsoluteZero = -273.15;

    public double Celsius
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public double Fahrenheit
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public double Kelvin => throw new NotImplementedException();
}

public sealed class UserProfile
{
    public required string Email
    {
        get => throw new NotImplementedException();
        init => throw new NotImplementedException();
    }

    public string DisplayName
    {
        get => throw new NotImplementedException();
        init => throw new NotImplementedException();
    }
}

public sealed class Grid(int rows, int columns)
{
    public int Rows => throw new NotImplementedException();

    public int Columns => throw new NotImplementedException();

    public int this[int row, int column]
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}

public sealed class Document
{
    public string Text
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public int WordCount => throw new NotImplementedException();

    public int Computations => throw new NotImplementedException();
}
