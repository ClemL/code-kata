namespace Katas.L3.Delegates;

public static class Functional
{
    public static Func<TIn, TOut> Compose<TIn, TMid, TOut>(Func<TIn, TMid> f, Func<TMid, TOut> g) =>
        x => g(f(x));

    public static Func<TIn, TOut> Memoize<TIn, TOut>(Func<TIn, TOut> f) where TIn : notnull
    {
        var cache = new Dictionary<TIn, TOut>();
        return x =>
        {
            if (!cache.TryGetValue(x, out var result))
            {
                result = f(x);
                cache[x] = result;
            }

            return result;
        };
    }

    public static Func<int> MakeCounter()
    {
        var count = 0;
        return () => ++count;
    }

    public static IReadOnlyList<Func<int, int>> CreateMultipliers(int count)
    {
        var multipliers = new List<Func<int, int>>(count);
        for (var i = 0; i < count; i++)
        {
            var factor = i;
            multipliers.Add(x => x * factor);
        }

        return multipliers;
    }

    public static Action<List<string>> Pipeline(params Action<List<string>>[] steps)
    {
        Action<List<string>> combined = static _ => { };
        foreach (var step in steps)
        {
            combined += step;
        }

        return combined;
    }
}

public sealed class PriceChangedEventArgs(string symbol, decimal? oldPrice, decimal newPrice) : EventArgs
{
    public string Symbol { get; } = symbol;

    public decimal? OldPrice { get; } = oldPrice;

    public decimal NewPrice { get; } = newPrice;
}

public sealed class StockTicker(decimal thresholdPercent)
{
    private readonly Dictionary<string, decimal> _lastReported = new(StringComparer.OrdinalIgnoreCase);

    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    public void Update(string symbol, decimal price)
    {
        decimal? oldPrice = null;

        if (_lastReported.TryGetValue(symbol, out var previous))
        {
            if (!ExceedsThreshold(previous, price))
            {
                return;
            }

            oldPrice = previous;
        }

        _lastReported[symbol] = price;
        PriceChanged?.Invoke(this, new PriceChangedEventArgs(symbol, oldPrice, price));
    }

    private bool ExceedsThreshold(decimal previous, decimal current) =>
        previous == 0
            ? current != 0
            : Math.Abs(current - previous) / Math.Abs(previous) * 100m >= thresholdPercent;
}
