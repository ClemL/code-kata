using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L2.Disposal;

[TestClass]
public class DisposalTests
{
    private sealed class Probe(string name, List<string> log, bool fail = false) : IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
            log.Add(name);
            if (fail)
            {
                throw new InvalidOperationException($"{name} failed");
            }
        }
    }

    [TestMethod]
    public void Scope_LogsInNestedOrder()
    {
        var log = new List<string>();

        void Work()
        {
            using var outer = Scope.Begin("outer", log);
            using var inner = Scope.Begin("inner", log);
            log.Add("work");
        }

        Work();

        CollectionAssert.AreEqual(new[] { "enter outer", "enter inner", "work", "exit inner", "exit outer" }, log);
    }

    [TestMethod]
    public void Scope_DisposeIsIdempotent()
    {
        var log = new List<string>();
        var scope = Scope.Begin("once", log);
        scope.Dispose();
        scope.Dispose();

        CollectionAssert.AreEqual(new[] { "enter once", "exit once" }, log);
    }

    [TestMethod]
    public void Composite_DisposesInReverseOrderOnce()
    {
        var log = new List<string>();
        var a = new Probe("a", log);
        var b = new Probe("b", log);
        var c = new Probe("c", log);

        var composite = new CompositeDisposable();
        composite.Add(a);
        composite.Add(b);
        composite.Add(c);
        Assert.AreEqual(3, composite.Count);

        composite.Dispose();
        composite.Dispose();

        CollectionAssert.AreEqual(new[] { "c", "b", "a" }, log);
        Assert.AreEqual(1, a.DisposeCount);
        Assert.IsTrue(composite.IsDisposed);
    }

    [TestMethod]
    public void Composite_DisposesLateAdditionsImmediately()
    {
        var log = new List<string>();
        var composite = new CompositeDisposable();
        composite.Dispose();

        var late = new Probe("late", log);
        composite.Add(late);

        Assert.AreEqual(1, late.DisposeCount);
    }

    [TestMethod]
    public void Composite_ContinuesAfterFailuresAndAggregatesThem()
    {
        var log = new List<string>();
        var composite = new CompositeDisposable();
        composite.Add(new Probe("a", log, fail: true));
        composite.Add(new Probe("b", log));
        composite.Add(new Probe("c", log, fail: true));

        var ex = Assert.ThrowsExactly<AggregateException>(composite.Dispose);

        CollectionAssert.AreEqual(new[] { "c", "b", "a" }, log);
        Assert.HasCount(2, ex.InnerExceptions);
        Assert.AreEqual("c failed", ex.InnerExceptions[0].Message);
    }

    [TestMethod]
    public async Task BufferedWriter_FlushesOnDisposeAsync()
    {
        var destination = new List<string>();

        await using (var writer = new BufferedLineWriter(destination))
        {
            writer.Write("first");
            writer.Write("second");
            Assert.AreEqual(2, writer.PendingCount);
            Assert.IsEmpty(destination, "Nothing is written until a flush.");
        }

        CollectionAssert.AreEqual(new[] { "first", "second" }, destination);
    }

    [TestMethod]
    public async Task BufferedWriter_RejectsWritesAfterDisposal()
    {
        var destination = new List<string>();
        var writer = new BufferedLineWriter(destination);
        writer.Write("only");

        await writer.DisposeAsync();
        await writer.DisposeAsync();

        Assert.ThrowsExactly<ObjectDisposedException>(() => writer.Write("late"));
        CollectionAssert.AreEqual(new[] { "only" }, destination, "A second DisposeAsync must not flush again.");
    }

    [TestMethod]
    public void NativeBuffer_ReadsAndWritesWithinBounds()
    {
        using var buffer = new NativeBuffer(4);
        Assert.AreEqual(4, buffer.Size);
        Assert.AreEqual((byte)0, buffer.Read(3));

        buffer.Write(3, 0xFF);
        Assert.AreEqual((byte)0xFF, buffer.Read(3));

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => buffer.Read(4));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => buffer.Write(-1, 1));
    }

    [TestMethod]
    public void NativeBuffer_TracksLiveCountAndRejectsUseAfterDispose()
    {
        var before = NativeBuffer.LiveCount;
        var buffer = new NativeBuffer(8);
        Assert.AreEqual(before + 1, NativeBuffer.LiveCount);

        buffer.Dispose();
        buffer.Dispose();

        Assert.AreEqual(before, NativeBuffer.LiveCount);
        Assert.IsTrue(buffer.IsDisposed);
        Assert.ThrowsExactly<ObjectDisposedException>(() => buffer.Read(0));
    }

    [TestMethod]
    public void NativeBuffer_HasAFinalizer()
    {
        var finalizer = typeof(NativeBuffer).GetMethod("Finalize", BindingFlags.Instance | BindingFlags.NonPublic, Type.EmptyTypes);
        Assert.AreEqual(typeof(NativeBuffer), finalizer?.DeclaringType, "Declare ~NativeBuffer() as a safety net.");
    }
}
