using System.Collections.Frozen;
using System.Collections.Immutable;

namespace Katas.L3.Immutable;

public static class Snapshots
{
    public static IReadOnlyList<int> ReadOnlyView(List<int> source) => throw new NotImplementedException();

    public static ImmutableArray<int> Snapshot(List<int> source) => throw new NotImplementedException();

    public static ImmutableArray<int> Squares(int count) => throw new NotImplementedException();
}

public sealed class AuditTrail
{
    public static AuditTrail Empty => throw new NotImplementedException();

    public IReadOnlyList<string> Entries => throw new NotImplementedException();

    public AuditTrail Append(string entry) => throw new NotImplementedException();
}

public sealed class Inventory
{
    public static Inventory Empty => throw new NotImplementedException();

    public int QuantityOf(string sku) => throw new NotImplementedException();

    public Inventory Restock(string sku, int quantity) => throw new NotImplementedException();

    public Inventory Remove(string sku, int quantity) => throw new NotImplementedException();

    public IReadOnlyDictionary<string, int> Stock => throw new NotImplementedException();
}

public static class CodeTables
{
    public static FrozenDictionary<string, string> BuildLookup(IEnumerable<(string Code, string Description)> rows) =>
        throw new NotImplementedException();

    public static FrozenSet<string> BuildAllowList(IEnumerable<string> codes) => throw new NotImplementedException();
}
