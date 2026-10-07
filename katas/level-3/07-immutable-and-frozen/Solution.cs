using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Katas.L3.Immutable;

public static class Snapshots
{
    public static IReadOnlyList<int> ReadOnlyView(List<int> source) => source.AsReadOnly();

    public static ImmutableArray<int> Snapshot(List<int> source) => [.. source];

    public static ImmutableArray<int> Squares(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var builder = ImmutableArray.CreateBuilder<int>(count);
        for (var i = 0; i < count; i++)
        {
            builder.Add(i * i);
        }

        return builder.MoveToImmutable();
    }
}

public sealed class AuditTrail
{
    private readonly ImmutableList<string> _entries;

    private AuditTrail(ImmutableList<string> entries) => _entries = entries;

    public static AuditTrail Empty { get; } = new(ImmutableList<string>.Empty);

    public IReadOnlyList<string> Entries => _entries;

    public AuditTrail Append(string entry) => new(_entries.Add(entry));
}

public sealed class Inventory
{
    private readonly ImmutableDictionary<string, int> _stock;

    private Inventory(ImmutableDictionary<string, int> stock) => _stock = stock;

    public static Inventory Empty { get; } = new(ImmutableDictionary<string, int>.Empty);

    public IReadOnlyDictionary<string, int> Stock => _stock;

    public int QuantityOf(string sku) => _stock.GetValueOrDefault(sku);

    public Inventory Restock(string sku, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        return new Inventory(_stock.SetItem(sku, QuantityOf(sku) + quantity));
    }

    public Inventory Remove(string sku, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);

        var available = QuantityOf(sku);
        if (available < quantity)
        {
            throw new InvalidOperationException($"Only {available} of {sku} in stock; cannot remove {quantity}.");
        }

        var remaining = available - quantity;
        return new Inventory(remaining == 0 ? _stock.Remove(sku) : _stock.SetItem(sku, remaining));
    }
}

public static class CodeTables
{
    public static FrozenDictionary<string, string> BuildLookup(IEnumerable<(string Code, string Description)> rows) =>
        rows.ToFrozenDictionary(row => row.Code, row => row.Description, StringComparer.OrdinalIgnoreCase);

    public static FrozenSet<string> BuildAllowList(IEnumerable<string> codes) =>
        codes.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
}
