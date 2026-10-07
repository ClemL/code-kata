namespace Katas.L2.Properties;

public sealed class Temperature
{
    public const double AbsoluteZero = -273.15;

    public double Celsius
    {
        get;
        set => field = value < AbsoluteZero
            ? throw new ArgumentOutOfRangeException(nameof(value), value, "Below absolute zero.")
            : value;
    }

    public double Fahrenheit
    {
        get => Celsius * 9 / 5 + 32;
        set => Celsius = (value - 32) * 5 / 9;
    }

    public double Kelvin => Celsius - AbsoluteZero;
}

public sealed class UserProfile
{
    public required string Email
    {
        get;
        init
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            field = value.Trim().ToLowerInvariant();
        }
    }

    public string DisplayName
    {
        get => field ?? Email;
        init;
    }
}

public sealed class Grid
{
    private readonly int[] _cells;

    public Grid(int rows, int columns)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);

        Rows = rows;
        Columns = columns;
        _cells = new int[rows * columns];
    }

    public int Rows { get; }

    public int Columns { get; }

    public int this[int row, int column]
    {
        get => _cells[IndexOf(row, column)];
        set => _cells[IndexOf(row, column)] = value;
    }

    private int IndexOf(int row, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Rows);
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Columns);

        return row * Columns + column;
    }
}

public sealed class Document
{
    private int? _wordCount;

    public string Text
    {
        get;
        set
        {
            field = value;
            _wordCount = null;
        }
    } = "";

    public int WordCount => _wordCount ??= CountWords();

    public int Computations { get; private set; }

    private int CountWords()
    {
        Computations++;
        return Text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
