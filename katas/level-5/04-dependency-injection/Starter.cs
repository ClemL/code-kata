using System.Reflection;

namespace Katas.L5.DependencyInjection;

public enum Lifetime
{
    Transient,
    Scoped,
    Singleton,
}

public interface IResolver
{
    T Resolve<T>();

    object Resolve(Type serviceType);
}

public interface IScope : IResolver, IDisposable;

public sealed class MiniContainer : IResolver, IDisposable
{
    public MiniContainer Register<TService, TImplementation>(Lifetime lifetime)
        where TService : class
        where TImplementation : class, TService =>
        throw new NotImplementedException();

    public MiniContainer Register<TService>(Func<IResolver, TService> factory, Lifetime lifetime)
        where TService : class =>
        throw new NotImplementedException();

    public IScope CreateScope() => throw new NotImplementedException();

    public T Resolve<T>() => throw new NotImplementedException();

    public object Resolve(Type serviceType) => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();
}
