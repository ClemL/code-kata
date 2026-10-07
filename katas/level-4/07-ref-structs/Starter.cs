using System.Runtime.InteropServices;

namespace Katas.L4.RefStructs;

public interface IMeasurable
{
    int Length { get; }

    int Sum();
}

public static class RefKata
{
    public static ref int Largest(Span<int> values) => throw new NotImplementedException();

    public static Dictionary<char, int> CountChars(string text) => throw new NotImplementedException();

    public static string Describe<T>(T item) where T : IMeasurable, allows ref struct => throw new NotImplementedException();
}

public ref struct Tokenizer(ReadOnlySpan<char> text, char separator)
{
    public ReadOnlySpan<char> Current => throw new NotImplementedException();

    public Tokenizer GetEnumerator() => throw new NotImplementedException();

    public bool MoveNext() => throw new NotImplementedException();
}

public ref struct FixedStringBuilder(Span<char> buffer)
{
    public int Length => throw new NotImplementedException();

    public bool Append(char value) => throw new NotImplementedException();

    public bool Append(ReadOnlySpan<char> value) => throw new NotImplementedException();

    public override string ToString() => throw new NotImplementedException();
}

public ref struct SpanWindow(ReadOnlySpan<int> values) : IMeasurable
{
    public int Length => throw new NotImplementedException();

    public int Sum() => throw new NotImplementedException();
}
