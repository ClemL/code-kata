using System.Collections.Concurrent;

namespace Katas.L4.Synchronization;

public sealed class HitCounter
{
    private long _count;

    public long Count => Interlocked.Read(ref _count);

    public void Increment() => Interlocked.Increment(ref _count);
}

public sealed class Account(int id, decimal openingBalance)
{
    private readonly Lock _gate = new();
    private decimal _balance = openingBalance;

    public int Id { get; } = id;

    public decimal Balance
    {
        get
        {
            lock (_gate)
            {
                return _balance;
            }
        }
    }

    public void Deposit(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        lock (_gate)
        {
            _balance += amount;
        }
    }

    public void Withdraw(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        lock (_gate)
        {
            WithdrawUnlocked(amount);
        }
    }

    public static void Transfer(Account from, Account to, decimal amount)
    {
        if (ReferenceEquals(from, to) || from.Id == to.Id)
        {
            throw new ArgumentException("Cannot transfer to the same account.", nameof(to));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        var (first, second) = from.Id < to.Id ? (from, to) : (to, from);
        lock (first._gate)
        {
            lock (second._gate)
            {
                from.WithdrawUnlocked(amount);
                to._balance += amount;
            }
        }
    }

    private void WithdrawUnlocked(decimal amount)
    {
        if (_balance < amount)
        {
            throw new InvalidOperationException($"Insufficient funds in account {Id}.");
        }

        _balance -= amount;
    }
}

public sealed class AsyncLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<IDisposable> LockAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        return new Releaser(_semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int _released;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _released, 1) == 0)
            {
                semaphore.Release();
            }
        }
    }
}

public sealed class SingleFlightCache<TKey, TValue>(Func<TKey, Task<TValue>> factory) where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, Lazy<Task<TValue>>> _entries = new();

    public async Task<TValue> GetAsync(TKey key)
    {
        var entry = _entries.GetOrAdd(key, k => new Lazy<Task<TValue>>(() => factory(k)));

        try
        {
            return await entry.Value;
        }
        catch
        {
            _entries.TryRemove(KeyValuePair.Create(key, entry));
            throw;
        }
    }
}

public static class WordCounter
{
    public static IReadOnlyDictionary<string, int> Count(IEnumerable<string> documents)
    {
        var counts = new ConcurrentDictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        Parallel.ForEach(documents, document =>
        {
            foreach (var word in document.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                counts.AddOrUpdate(word, 1, static (_, current) => current + 1);
            }
        });

        return counts;
    }
}
