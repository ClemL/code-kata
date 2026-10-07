namespace Katas.L2.Operators;

public readonly struct Fraction : IEquatable<Fraction>, IComparable<Fraction>
{
    public Fraction(long numerator, long denominator) => throw new NotImplementedException();

    public long Numerator => throw new NotImplementedException();

    public long Denominator => throw new NotImplementedException();

    public static Fraction Parse(string text) => throw new NotImplementedException();

    public static Fraction operator -(Fraction value) => throw new NotImplementedException();

    public static Fraction operator +(Fraction left, Fraction right) => throw new NotImplementedException();

    public static Fraction operator -(Fraction left, Fraction right) => throw new NotImplementedException();

    public static Fraction operator *(Fraction left, Fraction right) => throw new NotImplementedException();

    public static Fraction operator /(Fraction left, Fraction right) => throw new NotImplementedException();

    public static bool operator ==(Fraction left, Fraction right) => throw new NotImplementedException();

    public static bool operator !=(Fraction left, Fraction right) => throw new NotImplementedException();

    public static bool operator <(Fraction left, Fraction right) => throw new NotImplementedException();

    public static bool operator >(Fraction left, Fraction right) => throw new NotImplementedException();

    public static implicit operator Fraction(int value) => throw new NotImplementedException();

    public static explicit operator double(Fraction value) => throw new NotImplementedException();

    public bool Equals(Fraction other) => throw new NotImplementedException();

    public override bool Equals(object? obj) => throw new NotImplementedException();

    public override int GetHashCode() => throw new NotImplementedException();

    public int CompareTo(Fraction other) => throw new NotImplementedException();

    public override string ToString() => throw new NotImplementedException();
}
