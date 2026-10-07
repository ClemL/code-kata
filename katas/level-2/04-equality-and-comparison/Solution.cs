using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Katas.L2.Equality;

public readonly struct SemVer : IEquatable<SemVer>, IComparable<SemVer>
{
    public SemVer(int major, int minor, int patch, string? preRelease = null)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(major);
        ArgumentOutOfRangeException.ThrowIfNegative(minor);
        ArgumentOutOfRangeException.ThrowIfNegative(patch);

        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = string.IsNullOrEmpty(preRelease) ? null : preRelease;
    }

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    public string? PreRelease { get; }

    public static SemVer Parse(string text) =>
        TryParse(text, out var version)
            ? version
            : throw new FormatException($"'{text}' is not a valid semantic version.");

    public static bool TryParse([NotNullWhen(true)] string? text, out SemVer version)
    {
        version = default;
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        var dash = text.IndexOf('-');
        var core = dash < 0 ? text : text[..dash];
        var preRelease = dash < 0 ? null : text[(dash + 1)..];

        if (preRelease is not null && (preRelease.Length == 0 || preRelease.Split('.').Any(id => id.Length == 0)))
        {
            return false;
        }

        var parts = core.Split('.');
        if (parts.Length != 3
            || !TryParseComponent(parts[0], out var major)
            || !TryParseComponent(parts[1], out var minor)
            || !TryParseComponent(parts[2], out var patch))
        {
            return false;
        }

        version = new SemVer(major, minor, patch, preRelease);
        return true;
    }

    private static bool TryParseComponent(string text, out int value) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out value);

    public bool Equals(SemVer other) =>
        Major == other.Major
        && Minor == other.Minor
        && Patch == other.Patch
        && string.Equals(PreRelease, other.PreRelease, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is SemVer other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Patch, PreRelease);

    public int CompareTo(SemVer other)
    {
        var result = Major.CompareTo(other.Major);
        if (result != 0) return result;

        result = Minor.CompareTo(other.Minor);
        if (result != 0) return result;

        result = Patch.CompareTo(other.Patch);
        if (result != 0) return result;

        return ComparePreRelease(PreRelease, other.PreRelease);
    }

    private static int ComparePreRelease(string? left, string? right)
    {
        if (left is null) return right is null ? 0 : 1;
        if (right is null) return -1;

        var leftIds = left.Split('.');
        var rightIds = right.Split('.');

        for (var i = 0; i < Math.Min(leftIds.Length, rightIds.Length); i++)
        {
            var leftIsNumber = int.TryParse(leftIds[i], NumberStyles.None, CultureInfo.InvariantCulture, out var leftNumber);
            var rightIsNumber = int.TryParse(rightIds[i], NumberStyles.None, CultureInfo.InvariantCulture, out var rightNumber);

            var result = (leftIsNumber, rightIsNumber) switch
            {
                (true, true) => leftNumber.CompareTo(rightNumber),
                (true, false) => -1,
                (false, true) => 1,
                (false, false) => string.CompareOrdinal(leftIds[i], rightIds[i]),
            };

            if (result != 0) return result;
        }

        return leftIds.Length.CompareTo(rightIds.Length);
    }

    public override string ToString() =>
        PreRelease is null
            ? string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Patch}")
            : string.Create(CultureInfo.InvariantCulture, $"{Major}.{Minor}.{Patch}-{PreRelease}");

    public static bool operator ==(SemVer left, SemVer right) => left.Equals(right);

    public static bool operator !=(SemVer left, SemVer right) => !left.Equals(right);

    public static bool operator <(SemVer left, SemVer right) => left.CompareTo(right) < 0;

    public static bool operator >(SemVer left, SemVer right) => left.CompareTo(right) > 0;

    public static bool operator <=(SemVer left, SemVer right) => left.CompareTo(right) <= 0;

    public static bool operator >=(SemVer left, SemVer right) => left.CompareTo(right) >= 0;
}

public sealed record Product(string Sku, string Name, decimal Price);

public sealed class SkuComparer : IEqualityComparer<Product>
{
    public static SkuComparer Instance { get; } = new();

    public bool Equals(Product? x, Product? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return StringComparer.OrdinalIgnoreCase.Equals(x.Sku, y.Sku);
    }

    public int GetHashCode(Product obj) => StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Sku);
}
