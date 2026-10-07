using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Katas.L2.Equality;

public readonly struct SemVer : IEquatable<SemVer>, IComparable<SemVer>
{
    public SemVer(int major, int minor, int patch, string? preRelease = null)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
    }

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    public string? PreRelease { get; }

    public static SemVer Parse(string text) => throw new NotImplementedException();

    public static bool TryParse([NotNullWhen(true)] string? text, out SemVer version) => throw new NotImplementedException();

    public bool Equals(SemVer other) => throw new NotImplementedException();

    public override bool Equals(object? obj) => throw new NotImplementedException();

    public override int GetHashCode() => throw new NotImplementedException();

    public int CompareTo(SemVer other) => throw new NotImplementedException();

    public override string ToString() => throw new NotImplementedException();

    public static bool operator ==(SemVer left, SemVer right) => throw new NotImplementedException();

    public static bool operator !=(SemVer left, SemVer right) => throw new NotImplementedException();

    public static bool operator <(SemVer left, SemVer right) => throw new NotImplementedException();

    public static bool operator >(SemVer left, SemVer right) => throw new NotImplementedException();

    public static bool operator <=(SemVer left, SemVer right) => throw new NotImplementedException();

    public static bool operator >=(SemVer left, SemVer right) => throw new NotImplementedException();
}

public sealed record Product(string Sku, string Name, decimal Price);

public sealed class SkuComparer : IEqualityComparer<Product>
{
    public static SkuComparer Instance { get; } = new();

    public bool Equals(Product? x, Product? y) => throw new NotImplementedException();

    public int GetHashCode(Product obj) => throw new NotImplementedException();
}
