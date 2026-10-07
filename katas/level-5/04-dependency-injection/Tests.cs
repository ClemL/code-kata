using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Katas.L5.DependencyInjection;

public interface IClock
{
    DateTimeOffset Now { get; }
}

public sealed class FixedClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset Now { get; } = now;
}

public interface IRepository;

public sealed class Repository(IClock clock, List<string> disposals) : IRepository, IDisposable
{
    public IClock Clock { get; } = clock;

    public bool IsDisposed { get; private set; }

    public void Dispose()
    {
        IsDisposed = true;
        disposals.Add(nameof(Repository));
    }
}

public sealed class AuditLog(List<string> disposals) : IDisposable
{
    public void Dispose() => disposals.Add(nameof(AuditLog));
}

public sealed class OrderService
{
    public OrderService() => throw new InvalidOperationException("The container should pick the richest constructor.");

    public OrderService(IRepository repository, IClock clock)
    {
        Repository = repository;
        Clock = clock;
    }

    public IRepository Repository { get; }

    public IClock Clock { get; }
}

public sealed class Cache(IRepository repository)
{
    public IRepository Repository { get; } = repository;
}

public sealed class ServiceA(ServiceB b)
{
    public ServiceB B { get; } = b;
}

public sealed class ServiceB(ServiceA a)
{
    public ServiceA A { get; } = a;
}

[TestClass]
public class DependencyInjectionTests
{
    private static readonly DateTimeOffset Noon = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    private static MiniContainer Build(List<string> disposals, Lifetime repositoryLifetime = Lifetime.Scoped) =>
        new MiniContainer()
            .Register<IClock>(_ => new FixedClock(Noon), Lifetime.Singleton)
            .Register<List<string>>(_ => disposals, Lifetime.Singleton)
            .Register<IRepository, Repository>(repositoryLifetime)
            .Register<OrderService, OrderService>(Lifetime.Transient);

    [TestMethod]
    public void Resolve_InjectsConstructorDependencies()
    {
        using var container = Build([]);
        using var scope = container.CreateScope();

        var service = scope.Resolve<OrderService>();

        Assert.IsInstanceOfType<Repository>(service.Repository);
        Assert.AreEqual(Noon, service.Clock.Now);
    }

    [TestMethod]
    public void Transient_CreatesNewInstances()
    {
        using var container = Build([]);
        using var scope = container.CreateScope();

        Assert.AreNotSame(scope.Resolve<OrderService>(), scope.Resolve<OrderService>());
    }

    [TestMethod]
    public void Singleton_IsSharedAcrossScopes()
    {
        using var container = Build([]);
        using var first = container.CreateScope();
        using var second = container.CreateScope();

        Assert.AreSame(first.Resolve<IClock>(), second.Resolve<IClock>());
        Assert.AreSame(container.Resolve<IClock>(), first.Resolve<IClock>());
    }

    [TestMethod]
    public void Scoped_IsSharedWithinAScopeOnly()
    {
        using var container = Build([]);
        using var first = container.CreateScope();
        using var second = container.CreateScope();

        Assert.AreSame(first.Resolve<IRepository>(), first.Resolve<IRepository>());
        Assert.AreNotSame(first.Resolve<IRepository>(), second.Resolve<IRepository>());
        Assert.AreSame(first.Resolve<IRepository>(), first.Resolve<OrderService>().Repository);
    }

    [TestMethod]
    public void Scoped_CannotBeResolvedFromRoot()
    {
        using var container = Build([]);
        Assert.ThrowsExactly<InvalidOperationException>(() => container.Resolve<IRepository>());
    }

    [TestMethod]
    public void Singleton_CannotCaptureScopedDependency()
    {
        using var container = Build([]).Register<Cache, Cache>(Lifetime.Singleton);
        using var scope = container.CreateScope();

        Assert.ThrowsExactly<InvalidOperationException>(() => scope.Resolve<Cache>());
    }

    [TestMethod]
    public void Resolve_ReportsMissingRegistrations()
    {
        using var container = new MiniContainer().Register<OrderService, OrderService>(Lifetime.Transient);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => container.Resolve<OrderService>());
        StringAssert.Contains(ex.Message, nameof(IRepository));
    }

    [TestMethod]
    public void Resolve_DetectsCycles()
    {
        using var container = new MiniContainer()
            .Register<ServiceA, ServiceA>(Lifetime.Transient)
            .Register<ServiceB, ServiceB>(Lifetime.Transient);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => container.Resolve<ServiceA>());
        StringAssert.Contains(ex.Message, "ServiceA -> ServiceB -> ServiceA");
    }

    [TestMethod]
    public void LaterRegistrationWins()
    {
        var later = new FixedClock(Noon.AddDays(1));
        using var container = Build([]).Register<IClock>(_ => later, Lifetime.Singleton);

        Assert.AreSame(later, container.Resolve<IClock>());
    }

    [TestMethod]
    public void DisposingScope_DisposesWhatItCreatedInReverseOrder()
    {
        var disposals = new List<string>();
        using var container = Build(disposals).Register<AuditLog, AuditLog>(Lifetime.Transient);

        Repository repository;
        using (var scope = container.CreateScope())
        {
            repository = (Repository)scope.Resolve<IRepository>();
            scope.Resolve<AuditLog>();
            Assert.IsFalse(repository.IsDisposed);
        }

        Assert.IsTrue(repository.IsDisposed);
        CollectionAssert.AreEqual(new[] { nameof(AuditLog), nameof(Repository) }, disposals);
    }

    [TestMethod]
    public void DisposingContainer_DisposesSingletons()
    {
        var disposals = new List<string>();
        var container = Build(disposals, repositoryLifetime: Lifetime.Singleton);
        var repository = (Repository)container.Resolve<IRepository>();

        using (var scope = container.CreateScope())
        {
            scope.Resolve<IRepository>();
        }

        Assert.IsFalse(repository.IsDisposed, "A scope must not dispose singletons.");

        container.Dispose();
        Assert.IsTrue(repository.IsDisposed);
    }

    [TestMethod]
    public void ResolvingAfterDispose_Throws()
    {
        var container = Build([]);
        var scope = container.CreateScope();
        scope.Dispose();

        Assert.ThrowsExactly<ObjectDisposedException>(() => scope.Resolve<IClock>());

        container.Dispose();
        Assert.ThrowsExactly<ObjectDisposedException>(() => container.Resolve<IClock>());
    }
}
