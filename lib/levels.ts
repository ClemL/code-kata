export type LevelInfo = {
  number: number;
  title: string;
  tagline: string;
  description: string;
};

export const LEVELS: LevelInfo[] = [
  {
    number: 1,
    title: "Syntax & Fundamentals",
    tagline: "Modern C# muscle memory",
    description:
      "Switch expressions and patterns, strings and culture, nullable reference types, parameter passing, collections, numeric edge cases, enums, and dates. Short katas that rebuild fluency fast.",
  },
  {
    number: 2,
    title: "Types & Object Model",
    tagline: "Designing types the C# way",
    description:
      "Records and value equality, properties with the C# 14 field keyword, interfaces and inheritance, IEquatable/IComparable, operators and conversions, exceptions, disposal, and extension methods.",
  },
  {
    number: 3,
    title: "Generics, Delegates & LINQ",
    tagline: "Composable, reusable code",
    description:
      "Constraints and variance, closures and events, LINQ aggregation and joins, iterators, advanced pattern matching, immutable and frozen collections, and graph algorithms with PriorityQueue.",
  },
  {
    number: 4,
    title: "Async, Concurrency & Performance",
    tagline: "Correct under load",
    description:
      "async/await done right, cancellation, async streams, channels, locks and concurrent collections, Span<T> with zero-allocation tests, ref structs, and ArrayPool/SearchValues.",
  },
  {
    number: 5,
    title: "Advanced & Modern .NET",
    tagline: "How the frameworks work inside",
    description:
      "Generic math, attributes and reflection, expression trees, a DI container and middleware pipeline from scratch, System.Text.Json internals, TimeProvider-based resilience, LINQ over your own types, and C# 14.",
  },
];

export function getLevel(number: number): LevelInfo | undefined {
  return LEVELS.find((level) => level.number === number);
}
