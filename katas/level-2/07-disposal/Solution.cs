using System.Runtime.InteropServices;

namespace Katas.L2.Disposal;

public static class Scope
{
    public static IDisposable Begin(string name, List<string> log)
    {
        log.Add($"enter {name}");
        return new LoggedScope(name, log);
    }

    private sealed class LoggedScope(string name, List<string> log) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            log.Add($"exit {name}");
        }
    }
}

public sealed class CompositeDisposable : IDisposable
{
    private readonly List<IDisposable> _items = [];

    public int Count => _items.Count;

    public bool IsDisposed { get; private set; }

    public void Add(IDisposable item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (IsDisposed)
        {
            item.Dispose();
            return;
        }

        _items.Add(item);
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;
        List<Exception>? failures = null;

        for (var i = _items.Count - 1; i >= 0; i--)
        {
            try
            {
                _items[i].Dispose();
            }
            catch (Exception ex)
            {
                (failures ??= []).Add(ex);
            }
        }

        _items.Clear();

        if (failures is not null)
        {
            throw new AggregateException("One or more items failed to dispose.", failures);
        }
    }
}

public sealed class BufferedLineWriter(List<string> destination) : IAsyncDisposable
{
    private readonly List<string> _buffer = [];
    private bool _disposed;

    public int PendingCount => _buffer.Count;

    public void Write(string line)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _buffer.Add(line);
    }

    public async ValueTask FlushAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await Task.Yield(); // Stand-in for real asynchronous I/O.
        destination.AddRange(_buffer);
        _buffer.Clear();
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await FlushAsync();
        _disposed = true;
    }
}

public sealed class NativeBuffer : IDisposable
{
    private static int s_liveCount;
    private IntPtr _handle;

    public NativeBuffer(int size)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(size);

        _handle = Marshal.AllocHGlobal(size);
        Size = size;
        Interlocked.Increment(ref s_liveCount);

        for (var i = 0; i < size; i++)
        {
            Marshal.WriteByte(_handle, i, 0);
        }
    }

    ~NativeBuffer() => Dispose(disposing: false);

    public static int LiveCount => Volatile.Read(ref s_liveCount);

    public int Size { get; }

    public bool IsDisposed => _handle == IntPtr.Zero;

    public byte Read(int offset)
    {
        ThrowIfInvalid(offset);
        return Marshal.ReadByte(_handle, offset);
    }

    public void Write(int offset, byte value)
    {
        ThrowIfInvalid(offset);
        Marshal.WriteByte(_handle, offset, value);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        // `disposing` is true when called from Dispose(); managed objects may be touched only then.
        // This class owns no managed disposables, so both paths release the same unmanaged memory.
        _ = disposing;

        var handle = Interlocked.Exchange(ref _handle, IntPtr.Zero);
        if (handle != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(handle);
            Interlocked.Decrement(ref s_liveCount);
        }
    }

    private void ThrowIfInvalid(int offset)
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(offset, Size);
    }
}
