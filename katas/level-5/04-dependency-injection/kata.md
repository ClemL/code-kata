---
title: Build a DI Container
level: 5
minutes: 60
summary: Implement transient, scoped, and singleton lifetimes, constructor injection via reflection, circular-dependency detection, captive-dependency protection, and ordered disposal.
topics: [Dependency injection, Reflection, Object lifetimes, Architecture]
keywords: [typeof, interface, IDisposable, using]
apis: [Type.GetConstructors, ConstructorInfo.Invoke, IServiceProvider, IDisposable]
docs:
  - title: Dependency injection in .NET
    url: https://learn.microsoft.com/dotnet/core/extensions/dependency-injection
  - title: Service lifetimes
    url: https://learn.microsoft.com/dotnet/core/extensions/dependency-injection#service-lifetimes
  - title: Dependency injection guidelines (captive dependencies)
    url: https://learn.microsoft.com/dotnet/core/extensions/dependency-injection-guidelines
---

## Brief

`Microsoft.Extensions.DependencyInjection` is used by every ASP.NET Core and Worker Service app, and most of its rules make sense only once you have hit the problem they prevent. Building a small container yourself is the fastest way to internalize lifetimes, scopes, captive dependencies, and why `IDisposable` services are owned by the container that created them.

## Tasks

Implement `MiniContainer` (the root) and the scopes it creates:

1. **Registration**
   - `Register<TService, TImplementation>(Lifetime lifetime)` maps a service to a concrete type built by constructor injection.
   - `Register<TService>(Func<IResolver, TService> factory, Lifetime lifetime)` uses a factory.
   - Both return the container for chaining. Registering the same service again replaces the earlier registration.
2. **Constructor injection.** Use the public constructor with the **most** parameters and resolve each parameter from the container.
3. **Lifetimes**
   - `Transient`: a new instance on every resolve;
   - `Singleton`: one instance for the container, shared by all scopes;
   - `Scoped`: one instance per scope. Resolving a scoped service from the root container throws `InvalidOperationException`. A singleton that depends on a scoped service fails the same way (captive dependency).
4. **Errors**
   - an unregistered service throws `InvalidOperationException` whose message contains the service's type name;
   - a dependency cycle throws `InvalidOperationException` whose message shows the chain, e.g. `ServiceA -> ServiceB -> ServiceA`.
5. **Disposal**
   - disposing a scope disposes the `IDisposable` instances **it created** (scoped and transient), in reverse creation order;
   - disposing the container disposes singletons (and transients resolved from the root);
   - resolving from a disposed scope or container throws `ObjectDisposedException`.

## Hints

- Store registrations as `Dictionary<Type, Registration>` where `Registration` holds the lifetime and a `Func<Scope, object>` factory.
- One private `Scope` class can serve as both the root (with a flag) and child scopes. Singletons are always created by the root scope, so their dependencies are resolved from the root, which gives captive-dependency detection for free.
- `implementationType.GetConstructors().MaxBy(c => c.GetParameters().Length)`, then `constructor.Invoke(parameters.Select(p => Resolve(p.ParameterType)).ToArray())`.
- Track the types currently being resolved in a `Stack<Type>` (or `List<Type>`); seeing a type that is already on the stack means a cycle.
- Keep a `List<IDisposable>` per scope and dispose it backwards.

## Notes

- **Captive dependencies** are the most common DI bug in real applications: a singleton holding a scoped `DbContext` keeps one context alive for the whole process and shares it across requests and threads. `Microsoft.Extensions.DependencyInjection` detects this when `ValidateScopes` is on, which is the default in the Development environment.
- **Transient disposables resolved from the root** are kept alive until the root is disposed, which is effectively a memory leak in long-running processes. Resolve them from a scope.
- **Thread safety.** The real container uses locks for singleton creation and is safe to resolve from multiple threads; this kata's container does not need to be.
- **.NET 8 keyed services** (`AddKeyedSingleton`, `[FromKeyedServices]`) are the built-in answer to "two implementations of the same interface".
