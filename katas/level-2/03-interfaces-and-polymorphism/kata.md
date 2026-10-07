---
title: Interfaces & Polymorphism
level: 2
minutes: 30
summary: Default interface methods, abstract base classes, virtual/override/sealed, and a custom collection with an explicit interface implementation.
topics: [Interfaces, Inheritance, Polymorphism, Collection initializers]
keywords: [interface, abstract, virtual, override, sealed, base]
apis: ["IEnumerable<T>", "IEnumerator<T>", Math.PI]
docs:
  - title: Interfaces
    url: https://learn.microsoft.com/dotnet/csharp/fundamentals/types/interfaces
  - title: Default interface methods
    url: https://learn.microsoft.com/dotnet/csharp/advanced-topics/interface-implementation/default-interface-methods-versions
  - title: Explicit interface implementation
    url: https://learn.microsoft.com/dotnet/csharp/programming-guide/interfaces/explicit-interface-implementation
  - title: Polymorphism
    url: https://learn.microsoft.com/dotnet/csharp/fundamentals/object-oriented/polymorphism
---

## Brief

The classic shapes exercise, used to rebuild the vocabulary: an interface with a **default method** (C# 8), an **abstract** base class that implements part of the contract, **virtual** members with a **sealed override**, and a small collection type that plugs into `foreach` and collection-initializer syntax by implementing `IEnumerable<T>`, including the explicit non-generic `GetEnumerator`.

## Tasks

1. **`IShape.Describe()`** is a default interface method returning `"{Name}: area {Area:F2}, perimeter {Perimeter:F2}"` (invariant culture).
2. **`Polygon`** (abstract) implements `Perimeter` as the sum of `Sides`, and a virtual `Name` that returns the runtime type name (`GetType().Name`).
3. **`Rectangle(width, height)`** exposes `Width`, `Height`, `Sides`, and `Area`. Non-positive dimensions throw `ArgumentOutOfRangeException`.
4. **`Square(side)`** derives from `Rectangle`, cannot be subclassed further, and seals its `Name` override to return `"Square"`.
5. **`Triangle(a, b, c)`** computes its area with Heron's formula. Side lengths that violate the triangle inequality throw `ArgumentException`.
6. **`Circle(radius)`** implements `IShape` directly (it is not a polygon). Non-positive radius throws `ArgumentOutOfRangeException`.
7. **`ShapeSet`** implements `IEnumerable<IShape>` with `Add`, `TotalArea`, and `Largest()` (by area, `null` when empty). Because it has `Add` and implements `IEnumerable`, callers can write `new ShapeSet { new Circle(1), new Square(2) }`.

## Hints

- Default interface methods are written in the interface body and can call other interface members: `string Describe() => string.Create(CultureInfo.InvariantCulture, $"...")`.
- Default methods are only callable through the interface type: `((IShape)square).Describe()`.
- Heron's formula: `s = (a + b + c) / 2`, area `= √(s(s−a)(s−b)(s−c))`.
- `public sealed override string Name => "Square";` stops further overriding of that member; `sealed class` stops further inheritance.
- `IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();` is the standard explicit implementation.
- `MaxBy(shape => shape.Area)` (.NET 6) returns `null` for an empty sequence of reference types.

## Notes

- **Interface vs abstract class.** Interfaces define capability and allow multiple inheritance; abstract classes share state and implementation. Default interface methods blur the line but cannot hold instance state.
- **`new` vs `override`.** `override` replaces the virtual slot, so calls through a base reference reach the derived implementation. `new` *hides* the member: calls through a base reference still reach the base version. Hiding is almost always a bug or a versioning workaround.
- **Collection initializers are pattern-based.** The compiler requires `IEnumerable` plus an accessible `Add` method. C# 12 collection expressions (`[a, b]`) use a different mechanism (`CollectionBuilderAttribute` or a constructible type with `Add`).
- **Prefer `sealed` by default.** Sealed classes enable devirtualization in the JIT and make the inheritance surface explicit.
