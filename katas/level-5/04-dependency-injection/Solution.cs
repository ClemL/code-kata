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
    private readonly Dictionary<Type, Registration> _registrations = [];
    private readonly Scope _root;

    public MiniContainer() => _root = new Scope(this, isRoot: true);

    private sealed record Registration(Lifetime Lifetime, Func<Scope, object> Factory);

    public MiniContainer Register<TService, TImplementation>(Lifetime lifetime)
        where TService : class
        where TImplementation : class, TService
    {
        _registrations[typeof(TService)] = new Registration(lifetime, scope => scope.Construct(typeof(TImplementation)));
        return this;
    }

    public MiniContainer Register<TService>(Func<IResolver, TService> factory, Lifetime lifetime)
        where TService : class
    {
        _registrations[typeof(TService)] = new Registration(lifetime, scope => factory(scope));
        return this;
    }

    public IScope CreateScope()
    {
        _root.ThrowIfDisposed();
        return new Scope(this, isRoot: false);
    }

    public T Resolve<T>() => _root.Resolve<T>();

    public object Resolve(Type serviceType) => _root.Resolve(serviceType);

    public void Dispose() => _root.Dispose();

    private sealed class Scope(MiniContainer container, bool isRoot) : IScope
    {
        private readonly Dictionary<Type, object> _cached = [];
        private readonly List<IDisposable> _disposables = [];
        private readonly List<Type> _resolving = [];
        private bool _disposed;

        public T Resolve<T>() => (T)Resolve(typeof(T));

        public object Resolve(Type serviceType)
        {
            ThrowIfDisposed();

            if (!container._registrations.TryGetValue(serviceType, out var registration))
            {
                throw new InvalidOperationException($"No service registered for {serviceType.Name}.");
            }

            if (_resolving.Contains(serviceType))
            {
                var chain = _resolving.SkipWhile(t => t != serviceType).Append(serviceType).Select(t => t.Name);
                throw new InvalidOperationException($"Circular dependency detected: {string.Join(" -> ", chain)}.");
            }

            _resolving.Add(serviceType);
            try
            {
                return registration.Lifetime switch
                {
                    Lifetime.Transient => Track(registration.Factory(this)),
                    Lifetime.Singleton => container._root.GetOrCreate(serviceType, registration),
                    Lifetime.Scoped when isRoot => throw new InvalidOperationException(
                        $"Cannot resolve scoped service {serviceType.Name} from the root container."),
                    Lifetime.Scoped => GetOrCreate(serviceType, registration),
                    _ => throw new InvalidOperationException($"Unknown lifetime {registration.Lifetime}."),
                };
            }
            finally
            {
                _resolving.RemoveAt(_resolving.Count - 1);
            }
        }

        public object Construct(Type implementationType)
        {
            var constructor = implementationType.GetConstructors().MaxBy(c => c.GetParameters().Length)
                ?? throw new InvalidOperationException($"{implementationType.Name} has no public constructor.");

            var arguments = constructor.GetParameters().Select(p => Resolve(p.ParameterType)).ToArray();
            return constructor.Invoke(arguments);
        }

        public void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            for (var i = _disposables.Count - 1; i >= 0; i--)
            {
                _disposables[i].Dispose();
            }

            _disposables.Clear();
            _cached.Clear();
        }

        private object GetOrCreate(Type serviceType, Registration registration)
        {
            ThrowIfDisposed();

            if (!_cached.TryGetValue(serviceType, out var instance))
            {
                instance = Track(registration.Factory(this));
                _cached[serviceType] = instance;
            }

            return instance;
        }

        private object Track(object instance)
        {
            if (instance is IDisposable disposable)
            {
                _disposables.Add(disposable);
            }

            return instance;
        }
    }
}
