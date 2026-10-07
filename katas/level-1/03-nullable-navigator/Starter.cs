using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Katas.L1.Nullables;

public sealed record Address(string Street, string? City, string? PostalCode);

public sealed record Customer(string Name, Address? Address, string? Email);

public static class NullSafety
{
    public static string CityOrUnknown(Customer? customer) => throw new NotImplementedException();

    public static string? NormalizeOrNull(string? input) => throw new NotImplementedException();

    public static bool TryGetPostalCode(Customer? customer, [NotNullWhen(true)] out string? postalCode) =>
        throw new NotImplementedException();

    public static string Require(
        [NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? expression = null) =>
        throw new NotImplementedException();

    public static int SumIgnoringNulls(IEnumerable<int?> values) => throw new NotImplementedException();

    public static int? SumStrict(IEnumerable<int?> values) => throw new NotImplementedException();
}

public sealed class ConfigCache(Func<string> loader)
{
    public int LoadCount => throw new NotImplementedException();

    public string ConnectionString => throw new NotImplementedException();
}
