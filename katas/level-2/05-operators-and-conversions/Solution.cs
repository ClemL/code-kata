using System.Globalization;

namespace Katas.L2.Operators;

public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction>
{
    private readonly long _numerator;
    private readonly long _denominator;

    public Fraction(long numerator, long denominator)
    {
        if (denominator == 0)
        {
            throw new DivideByZeroException("The denominator must not be zero.");
        }

        if (denominator < 0)
        {
            numerator = checked(-numerator);
            denominator = checked(-denominator);
        }

        var gcd = Gcd(numerator, denominator);
        _numerator = numerator / gcd;
        _denominator = denominator / gcd;
    }

    public long Numerator => _numerator;

    public long Denominator => _denominator == 0 ? 1 : _denominator;

    private static long Gcd(long a, long b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }

        return a == 0 ? 1 : a;
    }

    public static Fraction Parse(string text)
    {
        var parts = text.Split('/');
        var style = NumberStyles.AllowLeadingSign;
        var culture = CultureInfo.InvariantCulture;

        return parts switch
        {
            [var whole] when long.TryParse(whole, style, culture, out var n) => new Fraction(n, 1),
            [var top, var bottom] when long.TryParse(top, style, culture, out var n)
                                    && long.TryParse(bottom, style, culture, out var d) => new Fraction(n, d),
            _ => throw new FormatException($"'{text}' is not a valid fraction."),
        };
    }

    public static Fraction operator -(Fraction value) => new(checked(-value.Numerator), value.Denominator);

    public static Fraction operator +(Fraction left, Fraction right) => new(
        checked(left.Numerator * right.Denominator + right.Numerator * left.Denominator),
        checked(left.Denominator * right.Denominator));

    public static Fraction operator -(Fraction left, Fraction right) => left + -right;

    public static Fraction operator *(Fraction left, Fraction right) => new(
        checked(left.Numerator * right.Numerator),
        checked(left.Denominator * right.Denominator));

    public static Fraction operator /(Fraction left, Fraction right) =>
        right.Numerator == 0
            ? throw new DivideByZeroException("Cannot divide by a zero fraction.")
            : new Fraction(
                checked(left.Numerator * right.Denominator),
                checked(left.Denominator * right.Numerator));

    public static bool operator ==(Fraction left, Fraction right) => left.Equals(right);

    public static bool operator !=(Fraction left, Fraction right) => !left.Equals(right);

    public static bool operator <(Fraction left, Fraction right) => left.CompareTo(right) < 0;

    public static bool operator >(Fraction left, Fraction right) => left.CompareTo(right) > 0;

    public static implicit operator Fraction(int value) => new(value, 1);

    public static explicit operator double(Fraction value) => (double)value.Numerator / value.Denominator;

    public bool Equals(Fraction other) => Numerator == other.Numerator && Denominator == other.Denominator;

    public override bool Equals(object? obj) => obj is Fraction other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);

    public int CompareTo(Fraction other) =>
        checked(Numerator * other.Denominator).CompareTo(checked(other.Numerator * Denominator));

    public override string ToString() =>
        Denominator == 1
            ? Numerator.ToString(CultureInfo.InvariantCulture)
            : string.Create(CultureInfo.InvariantCulture, $"{Numerator}/{Denominator}");
}
