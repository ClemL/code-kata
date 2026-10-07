import Link from "next/link";
import { HomeDashboard } from "@/components/HomeDashboard";
import { ArrowRightIcon, DownloadIcon } from "@/components/icons";
import { LevelCard } from "@/components/LevelCard";
import { getAllKatas, getKatasForLevel, toSummary } from "@/lib/katas";
import { LEVELS } from "@/lib/levels";
import { WORKSPACE_ZIP } from "@/lib/workspace";

const STEPS = [
  {
    title: "Get the workspace",
    body: "Download one MSTest project with every starter and test suite. It runs with dotnet test on the .NET 10 SDK.",
  },
  {
    title: "Pick today's kata",
    body: "Take the next new kata or the suggested review rep. Start the timer and stick to the timebox.",
  },
  {
    title: "Turn the tests green",
    body: "Run one kata with a filter or with dotnet watch. Use hints in order only when you are stuck.",
  },
  {
    title: "Compare and log",
    body: "Read the reference solution and the design notes, then log the rep to keep your streak.",
  },
];

export default function HomePage() {
  const katas = getAllKatas();
  const summaries = katas.map(toSummary);
  const totalTests = katas.reduce((sum, k) => sum + k.testCount, 0);

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6 sm:py-14">
      <section className="max-w-3xl">
        <p className="font-mono text-sm font-medium text-accent-text">C# 14 · .NET 10 · MSTest 4</p>
        <h1 className="mt-3 text-4xl font-semibold tracking-tight sm:text-5xl">
          Daily C# reps for engineers who already know how to program.
        </h1>
        <p className="mt-5 text-lg text-muted">
          {katas.length} katas across {LEVELS.length} levels, from switch expressions to expression trees. Each one is a
          focused exercise with a starter file, a failing MSTest suite ({totalTests} tests in total), progressive hints,
          and a reference solution with design notes. Write the code in your own IDE; track your streak here.
        </p>
        <div className="mt-7 flex flex-wrap gap-3">
          <Link
            href={`/katas/${katas[0].slug}`}
            className="inline-flex items-center gap-2 rounded-lg bg-accent px-4 py-2.5 text-sm font-semibold text-accent-fg hover:bg-accent-hover"
          >
            Start with Level 1 <ArrowRightIcon size={16} />
          </Link>
          <a
            href={`/download/${WORKSPACE_ZIP}`}
            className="inline-flex items-center gap-2 rounded-lg bg-surface px-4 py-2.5 text-sm font-semibold ring-1 ring-line hover:ring-line-strong"
          >
            <DownloadIcon size={16} /> Download workspace
          </a>
          <Link href="/setup" className="inline-flex items-center px-2 py-2.5 text-sm font-medium text-muted hover:text-fg">
            Setup guide
          </Link>
        </div>
      </section>

      <div className="mt-12">
        <HomeDashboard katas={summaries} />
      </div>

      <section aria-labelledby="levels" className="mt-16">
        <h2 id="levels" className="text-2xl font-semibold tracking-tight">
          Levels
        </h2>
        <p className="mt-1 text-sm text-muted">Each level builds on the previous one, but you can start wherever you feel rusty.</p>
        <div className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {LEVELS.map((level) => {
            const levelKatas = getKatasForLevel(level.number);
            return (
              <LevelCard
                key={level.number}
                level={level}
                slugs={levelKatas.map((k) => k.slug)}
                minutes={levelKatas.reduce((sum, k) => sum + k.minutes, 0)}
              />
            );
          })}
        </div>
      </section>

      <section aria-labelledby="how" className="mt-16">
        <h2 id="how" className="text-2xl font-semibold tracking-tight">
          How a session works
        </h2>
        <ol className="mt-6 grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {STEPS.map((step, i) => (
            <li key={step.title} className="rounded-xl border border-line bg-surface p-5">
              <span className="grid h-7 w-7 place-items-center rounded-full bg-accent-soft text-sm font-semibold text-accent-text">
                {i + 1}
              </span>
              <h3 className="mt-3 font-semibold">{step.title}</h3>
              <p className="mt-1 text-sm text-muted">{step.body}</p>
            </li>
          ))}
        </ol>
      </section>
    </div>
  );
}
