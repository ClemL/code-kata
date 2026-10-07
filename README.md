# .NET Katas

Leveled C# 14 / .NET 10 code katas for experienced engineers who want to get back into the habit of writing C#.
The site is a Next.js app built for Vercel; every kata is a real C# exercise with an MSTest 4 suite.

- **41 katas, 5 levels, 493 tests.** From switch expressions to expression trees, a DI container, and C# 14 extension members.
- **Each kata** has a brief, tasks, a starter (`Kata.cs`), an MSTest suite, progressive hints, a reference solution, design notes, and links to Microsoft Learn.
- **Habit tooling:** per-kata timer, reps and best times, daily streak, activity heatmap, a "next new kata" and a "review rep" suggestion, and JSON export/import of progress (stored in the browser's local storage).
- **Downloads:** one MSTest project with every kata, one per level, or one per kata, plus a `kata.ps1` helper for filtered and watch-mode test runs.

| Level | Theme | Katas |
| --- | --- | --- |
| 1 | Syntax & Fundamentals | Pattern matching, strings, nullable reference types, parameters and tuples, collections, numbers, enums and flags, dates |
| 2 | Types & Object Model | Records, properties (`field`), polymorphism, equality, operators, exceptions, disposal, extension methods |
| 3 | Generics, Delegates & LINQ | Generics and variance, delegates and events, LINQ analytics, iterators, advanced patterns, LINQ joins, immutable/frozen collections, priority queues and graphs |
| 4 | Async, Concurrency & Performance | async/await, cancellation, async streams, channels, synchronization, `Span<T>` (zero-allocation tests), ref structs, pooling |
| 5 | Advanced & Modern .NET | Generic math, reflection, expression trees, DI container, middleware pipeline, System.Text.Json, `TimeProvider` resilience, `Option<T>` + LINQ, C# 14 tour |

## Repository layout

```text
app/                 Next.js App Router pages (all statically generated)
  download/[file]/   Static route handler that builds the workspace zips at build time
components/          React components (client components handle progress, timer, hints)
lib/                 Content loader, Markdown/Shiki rendering, zip builder, progress store
katas/level-N/NN-slug/
  kata.md            Frontmatter (title, minutes, topics, keywords, apis, docs) + Brief/Tasks/Hints/Notes
  Starter.cs         What the learner starts from (members throw NotImplementedException)
  Solution.cs        Reference solution
  Tests.cs           MSTest suite shared by both
dotnet/
  Solutions/         Compiles every Solution.cs with its Tests.cs; must be green
  Starters/          Compiles every Starter.cs with its Tests.cs; must build (tests are expected to fail)
scripts/validate-katas.ts   Structural checks for kata folders
```

## Develop

Requirements: Node.js 20.9+ and, for the kata checks, the .NET 10 SDK.

```powershell
npm install
npm run dev            # http://localhost:3000
npm run validate       # kata folder structure and frontmatter
npm run typecheck
npm run build

npm run test:dotnet    # solutions pass + starters compile
```

`dotnet/global.json` opts `dotnet test` into Microsoft.Testing.Platform, which MSTest 4 requires on the .NET 10 SDK.

## Deploy to Vercel

The project uses Vercel's zero-configuration Next.js support; there is no `vercel.json`.

1. In the Vercel dashboard choose **Add New → Project** and import this GitHub repository.
2. Keep the detected framework preset (**Next.js**), root directory `./`, and the default build command.
3. Deploy. Every page and zip download is prerendered, so the deployment serves static files only.

Or from a terminal with the Vercel CLI:

```powershell
npm install --global vercel
vercel          # preview deployment
vercel --prod   # production deployment
```

`.vercelignore` keeps the `dotnet/` verification projects out of the upload.

## Add a kata

1. Create `katas/level-N/NN-your-slug/` with `kata.md`, `Starter.cs`, `Solution.cs`, and `Tests.cs`.
2. Use a unique file-scoped namespace such as `Katas.L3.YourKata` in all three `.cs` files.
3. In `kata.md`, include the frontmatter keys used by existing katas and the `## Brief`, `## Tasks`, `## Hints`, and `## Notes` sections. Each top-level bullet under `## Hints` becomes one progressively revealed hint.
4. Run `npm run validate` and `npm run test:dotnet`.
