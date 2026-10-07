using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L3.Delegates;

[TestClass]
public class DelegatesAndEventsTests
{
    [TestMethod]
    public void Compose_AppliesFirstThenSecond()
    {
        var lengthThenDouble = Functional.Compose((string s) => s.Length, (int n) => n * 2);
        Assert.AreEqual(10, lengthThenDouble("hello"));

        var parseThenFormat = Functional.Compose((string s) => int.Parse(s), (int n) => $"#{n + 1}");
        Assert.AreEqual("#42", parseThenFormat("41"));
    }

    [TestMethod]
    public void Memoize_CallsUnderlyingFunctionOncePerArgument()
    {
        var calls = 0;
        var slowSquare = (int x) => { calls++; return x * x; };
        var fastSquare = Functional.Memoize(slowSquare);

        Assert.AreEqual(9, fastSquare(3));
        Assert.AreEqual(9, fastSquare(3));
        Assert.AreEqual(16, fastSquare(4));
        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public void Memoize_InstancesHaveSeparateCaches()
    {
        var calls = 0;
        Func<string, int> length = s => { calls++; return s.Length; };

        Functional.Memoize(length)("abc");
        Functional.Memoize(length)("abc");

        Assert.AreEqual(2, calls);
    }

    [TestMethod]
    public void MakeCounter_KeepsIndependentState()
    {
        var a = Functional.MakeCounter();
        var b = Functional.MakeCounter();

        Assert.AreEqual(1, a());
        Assert.AreEqual(2, a());
        Assert.AreEqual(3, a());
        Assert.AreEqual(1, b());
    }

    [TestMethod]
    public void CreateMultipliers_CaptureTheirOwnFactor()
    {
        var multipliers = Functional.CreateMultipliers(4);

        Assert.HasCount(4, multipliers);
        Assert.AreEqual(0, multipliers[0](10));
        Assert.AreEqual(20, multipliers[2](10));
        Assert.AreEqual(30, multipliers[3](10));
    }

    [TestMethod]
    public void Pipeline_RunsStepsInOrder()
    {
        var pipeline = Functional.Pipeline(
            log => log.Add("validate"),
            log => log.Add("transform"),
            log => log.Add("save"));

        var log = new List<string>();
        pipeline(log);

        CollectionAssert.AreEqual(new[] { "validate", "transform", "save" }, log);
    }

    [TestMethod]
    public void Pipeline_WithNoStepsDoesNothing()
    {
        var log = new List<string>();
        Functional.Pipeline()(log);
        Assert.IsEmpty(log);
    }

    [TestMethod]
    public void Ticker_RaisesOnFirstPriceAndSignificantChanges()
    {
        var ticker = new StockTicker(thresholdPercent: 5m);
        var events = new List<PriceChangedEventArgs>();
        ticker.PriceChanged += (_, e) => events.Add(e);

        ticker.Update("MSFT", 100m);   // first price: raised
        ticker.Update("MSFT", 104m);   // +4%: ignored
        ticker.Update("MSFT", 104.99m); // +4.99% from 100: ignored
        ticker.Update("MSFT", 105m);   // +5% from 100: raised
        ticker.Update("MSFT", 99m);    // -5.7% from 105: raised

        Assert.HasCount(3, events);
        Assert.IsNull(events[0].OldPrice);
        Assert.AreEqual(100m, events[0].NewPrice);
        Assert.AreEqual(100m, events[1].OldPrice);
        Assert.AreEqual(105m, events[1].NewPrice);
        Assert.AreEqual(105m, events[2].OldPrice);
        Assert.AreEqual("MSFT", events[2].Symbol);
    }

    [TestMethod]
    public void Ticker_TracksSymbolsIndependently()
    {
        var ticker = new StockTicker(thresholdPercent: 1m);
        var symbols = new List<string>();
        ticker.PriceChanged += (_, e) => symbols.Add(e.Symbol);

        ticker.Update("AAPL", 200m);
        ticker.Update("NVDA", 900m);
        ticker.Update("AAPL", 200.5m);

        CollectionAssert.AreEqual(new[] { "AAPL", "NVDA" }, symbols);
    }

    [TestMethod]
    public void Ticker_SupportsUnsubscribe()
    {
        var ticker = new StockTicker(thresholdPercent: 0m);
        var count = 0;
        EventHandler<PriceChangedEventArgs> handler = (_, _) => count++;

        ticker.PriceChanged += handler;
        ticker.Update("X", 1m);
        ticker.PriceChanged -= handler;
        ticker.Update("X", 2m);

        Assert.AreEqual(1, count);
    }

    [TestMethod]
    public void Ticker_PassesItselfAsSender()
    {
        var ticker = new StockTicker(thresholdPercent: 1m);
        object? sender = null;
        ticker.PriceChanged += (s, _) => sender = s;

        ticker.Update("X", 1m);

        Assert.AreSame(ticker, sender);
    }
}
