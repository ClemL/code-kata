using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Katas.L1.Nullables;

public sealed record Address(string Street, string? City, string? PostalCode);

public sealed record Customer(string Name, Address? Address, string? Email);

public static class NullSafety
{
    public static string CityOrUnknown(Customer? customer) =>
        customer?.Address?.City ?? "Unknown";

    public static string? NormalizeOrNull(string? input) =>
        string.IsNullOrWhiteSpace(input) ? null : input.Trim();

    public static bool TryGetPostalCode(Customer? customer, [NotNullWhen(true)] out string? postalCode)
    {
        postalCode = customer?.Address?.PostalCode;
        return postalCode is not null;
    }

    public static string Require(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, expression);
        return value;
    }

    public static int SumIgnoringNulls(IEnumerable<int?> values) =>
        values.Sum() ?? 0;

    public static int? SumStrict(IEnumerable<int?> values) =>
        values.Aggregate((int?)0, (total, value) => total + value);
}

public sealed class ConfigCache(Func<string> loader)
{
    private string? _connectionString;

    public int LoadCount { get; private set; }

    public string ConnectionString => _connectionString ??= Load();

    private string Load()
    {
        LoadCount++;
        return loader();
    }
}
