using System.Runtime.InteropServices;

namespace Katas.L2.Disposal;

public static class Scope
{
    public static IDisposable Begin(string name, List<string> log) => throw new NotImplementedException();
}

public sealed class CompositeDisposable : IDisposable
{
    public int Count => throw new NotImplementedException();

    public bool IsDisposed => throw new NotImplementedException();

    public void Add(IDisposable item) => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}

public sealed class BufferedLineWriter(List<string> destination) : IAsyncDisposable
{
    public int PendingCount => throw new NotImplementedException();

    public void Write(string line) => throw new NotImplementedException();

    public ValueTask FlushAsync() => throw new NotImplementedException();

    public ValueTask DisposeAsync() => throw new NotImplementedException();
}

// Add a finalizer once Dispose(bool) works. Never add one that can throw.
public sealed class NativeBuffer : IDisposable
{
    public NativeBuffer(int size) => throw new NotImplementedException();

    public static int LiveCount => throw new NotImplementedException();

    public int Size => throw new NotImplementedException();

    public bool IsDisposed => throw new NotImplementedException();

    public byte Read(int offset) => throw new NotImplementedException();

    public void Write(int offset, byte value) => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}
