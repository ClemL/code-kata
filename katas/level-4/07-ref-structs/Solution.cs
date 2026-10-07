using System.Runtime.InteropServices;

namespace Katas.L4.RefStructs;

public interface IMeasurable
{
    int Length { get; }

    int Sum();
}

public static class RefKata
{
    public static ref int Largest(Span<int> values)
    {
        if (values.IsEmpty)
        {
            throw new ArgumentException("At least one value is required.", nameof(values));
        }

        var index = 0;
        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] > values[index])
            {
                index = i;
            }
        }

        return ref values[index];
    }

    public static Dictionary<char, int> CountChars(string text)
    {
        var counts = new Dictionary<char, int>();
        foreach (var c in text)
        {
            ref var count = ref CollectionsMarshal.GetValueRefOrAddDefault(counts, c, out _);
            count++;
        }

        return counts;
    }

    public static string Describe<T>(T item) where T : IMeasurable, allows ref struct =>
        $"{item.Length} items, sum {item.Sum()}";
}

public ref struct Tokenizer(ReadOnlySpan<char> text, char separator)
{
    private ReadOnlySpan<char> _remaining = text;

    public ReadOnlySpan<char> Current { get; private set; }

    public readonly Tokenizer GetEnumerator() => this;

    public bool MoveNext()
    {
        var start = 0;
        while (start < _remaining.Length && _remaining[start] == separator)
        {
            start++;
        }

        _remaining = _remaining[start..];
        if (_remaining.IsEmpty)
        {
            return false;
        }

        var end = _remaining.IndexOf(separator);
        if (end < 0)
        {
            end = _remaining.Length;
        }

        Current = _remaining[..end];
        _remaining = _remaining[end..];
        return true;
    }
}

public ref struct FixedStringBuilder(Span<char> buffer)
{
    private readonly Span<char> _buffer = buffer;
    private int _position;

    public readonly int Length => _position;

    public bool Append(char value)
    {
        if (_position >= _buffer.Length)
        {
            return false;
        }

        _buffer[_position++] = value;
        return true;
    }

    public bool Append(ReadOnlySpan<char> value)
    {
        if (!value.TryCopyTo(_buffer[_position..]))
        {
            return false;
        }

        _position += value.Length;
        return true;
    }

    public override readonly string ToString() => new(_buffer[.._position]);
}

public readonly ref struct SpanWindow(ReadOnlySpan<int> values) : IMeasurable
{
    private readonly ReadOnlySpan<int> _values = values;

    public int Length => _values.Length;

    public int Sum()
    {
        var total = 0;
        foreach (var value in _values)
        {
            total += value;
        }

        return total;
    }
}
