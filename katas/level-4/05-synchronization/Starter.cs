using System.Collections.Concurrent;

namespace Katas.L4.Synchronization;

public sealed class HitCounter
{
    public long Count => throw new NotImplementedException();

    public void Increment() => throw new NotImplementedException();
}

public sealed class Account(int id, decimal openingBalance)
{
    public int Id => throw new NotImplementedException();

    public decimal Balance => throw new NotImplementedException();

    public void Deposit(decimal amount) => throw new NotImplementedException();

    public void Withdraw(decimal amount) => throw new NotImplementedException();

    public static void Transfer(Account from, Account to, decimal amount) => throw new NotImplementedException();
}

public sealed class AsyncLock
{
    public Task<IDisposable> LockAsync(CancellationToken cancellationToken = default) => throw new NotImplementedException();
}

public sealed class SingleFlightCache<TKey, TValue>(Func<TKey, Task<TValue>> factory) where TKey : notnull
{
    public Task<TValue> GetAsync(TKey key) => throw new NotImplementedException();
}

public static class WordCounter
{
    public static IReadOnlyDictionary<string, int> Count(IEnumerable<string> documents) => throw new NotImplementedException();
}
