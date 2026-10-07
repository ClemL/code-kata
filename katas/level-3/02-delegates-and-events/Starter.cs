namespace Katas.L3.Delegates;

public static class Functional
{
    public static Func<TIn, TOut> Compose<TIn, TMid, TOut>(Func<TIn, TMid> f, Func<TMid, TOut> g) =>
        throw new NotImplementedException();

    public static Func<TIn, TOut> Memoize<TIn, TOut>(Func<TIn, TOut> f) where TIn : notnull =>
        throw new NotImplementedException();

    public static Func<int> MakeCounter() => throw new NotImplementedException();

    public static IReadOnlyList<Func<int, int>> CreateMultipliers(int count) => throw new NotImplementedException();

    public static Action<List<string>> Pipeline(params Action<List<string>>[] steps) =>
        throw new NotImplementedException();
}

public sealed class PriceChangedEventArgs(string symbol, decimal? oldPrice, decimal newPrice) : EventArgs
{
    public string Symbol { get; } = symbol;

    public decimal? OldPrice { get; } = oldPrice;

    public decimal NewPrice { get; } = newPrice;
}

public sealed class StockTicker(decimal thresholdPercent)
{
    public event EventHandler<PriceChangedEventArgs>? PriceChanged;

    public void Update(string symbol, decimal price) => throw new NotImplementedException();
}
