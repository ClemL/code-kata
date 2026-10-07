using System.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L3.Iterators;

[TestClass]
public class IteratorsTests
{
    private sealed class TrackingSequence<T>(params T[] items) : IEnumerable<T>
    {
        public int Disposed { get; private set; }

        public int Pulled { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            try
            {
                foreach (var item in items)
                {
                    Pulled++;
                    yield return item;
                }
            }
            finally
            {
                Disposed++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [TestMethod]
    public void Fibonacci_IsInfiniteAndLazy()
    {
        CollectionAssert.AreEqual(new long[] { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34 }, Sequences.Fibonacci().Take(10).ToArray());
        Assert.AreEqual(12586269025L, Sequences.Fibonacci().ElementAt(50));
    }

    [TestMethod]
    public void Range_SupportsPositiveAndNegativeSteps()
    {
        CollectionAssert.AreEqual(new[] { 0, 3, 6, 9 }, Sequences.Range(0, 10, 3).ToArray());
        CollectionAssert.AreEqual(new[] { 5, 3, 1 }, Sequences.Range(5, 0, -2).ToArray());
        Assert.IsFalse(Sequences.Range(5, 5, 1).Any());
    }

    [TestMethod]
    public void Range_ValidatesEagerly() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Sequences.Range(0, 10, 0));

    [TestMethod]
    public void Window_YieldsIndependentSlidingWindows()
    {
        var windows = Sequences.Window([1, 2, 3, 4, 5], 3).ToList();

        Assert.HasCount(3, windows);
        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, windows[0]);
        CollectionAssert.AreEqual(new[] { 2, 3, 4 }, windows[1]);
        CollectionAssert.AreEqual(new[] { 3, 4, 5 }, windows[2]);
        Assert.AreNotSame(windows[0], windows[1]);
    }

    [TestMethod]
    public void Window_HandlesShortInputAndValidatesEagerly()
    {
        Assert.IsFalse(Sequences.Window([1, 2], 3).Any());
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Sequences.Window([1], 0));
    }

    [TestMethod]
    public void Window_IsLazy()
    {
        var source = new TrackingSequence<int>(1, 2, 3, 4, 5, 6);
        var first = Sequences.Window(source, 2).First();

        CollectionAssert.AreEqual(new[] { 1, 2 }, first);
        Assert.AreEqual(2, source.Pulled);
    }

    [TestMethod]
    public void Interleave_AlternatesUntilBothAreExhausted()
    {
        CollectionAssert.AreEqual(
            new[] { "1", "a", "2", "b", "3" },
            Sequences.Interleave(["1", "2", "3"], ["a", "b"]).ToArray());
        CollectionAssert.AreEqual(
            new[] { "x", "p", "q" },
            Sequences.Interleave(["x"], ["p", "q"]).ToArray());
    }

    [TestMethod]
    public void Interleave_DisposesBothEnumerators()
    {
        var left = new TrackingSequence<int>(1, 2, 3);
        var right = new TrackingSequence<int>(10, 20);

        _ = Sequences.Interleave(left, right).Take(2).ToList();

        Assert.AreEqual(1, left.Disposed);
        Assert.AreEqual(1, right.Disposed);
    }

    [TestMethod]
    public void DistinctUntilChanged_CollapsesRuns()
    {
        CollectionAssert.AreEqual(new[] { 1, 2, 1, 3 }, Sequences.DistinctUntilChanged([1, 1, 2, 2, 2, 1, 3, 3]).ToArray());
        CollectionAssert.AreEqual(new string?[] { null, "a", null }, Sequences.DistinctUntilChanged<string?>([null, null, "a", null]).ToArray());
        Assert.IsFalse(Sequences.DistinctUntilChanged(Array.Empty<int>()).Any());
    }

    [TestMethod]
    public void Countdown_EnumeratesWithoutYield()
    {
        CollectionAssert.AreEqual(new[] { 3, 2, 1 }, new Countdown(3).ToArray());
        Assert.IsFalse(new Countdown(0).Any());

        var countdown = new Countdown(2);
        CollectionAssert.AreEqual(countdown.ToArray(), countdown.ToArray(), "Each GetEnumerator call starts fresh.");
    }

    [TestMethod]
    public void Countdown_EnumeratorFollowsTheProtocol()
    {
        using var e = new Countdown(1).GetEnumerator();

        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual(1, e.Current);
        Assert.IsFalse(e.MoveNext());
        Assert.IsFalse(e.MoveNext(), "MoveNext keeps returning false after the end.");
        Assert.ThrowsExactly<NotSupportedException>(e.Reset);
    }
}
