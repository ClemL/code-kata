"use client";

import Link from "next/link";
import type { KataSummary } from "@/lib/katas";
import { useHydrated, useProgress } from "@/lib/progress";
import { currentStreak, formatRelativeDay, lastCompletion, localDateKey, nextUp, reviewPick, totals } from "@/lib/stats";
import { ActivityHeatmap } from "./ActivityHeatmap";
import { ArrowRightIcon, ClockIcon, FlameIcon } from "./icons";

export function HomeDashboard({ katas }: { katas: KataSummary[] }) {
  const hydrated = useHydrated();
  const progress = useProgress();

  const streak = currentStreak(progress);
  const { reps, minutes, katas: completed } = totals(progress);
  const next = nextUp(katas, progress);
  const review = reviewPick(katas, progress);
  const practicedToday = Object.values(progress.completions).some((list) =>
    list.some((c) => localDateKey(new Date(c.at)) === localDateKey(new Date())),
  );

  return (
    <section aria-label="Your practice" className="grid gap-4 lg:grid-cols-[minmax(0,1fr)_minmax(0,1fr)]">
      <div className="rounded-xl border border-line bg-surface p-5">
        <div className="flex items-center justify-between">
          <h2 className="text-sm font-semibold">Your practice</h2>
          {hydrated ? (
            <span className={`text-xs ${practicedToday ? "text-ok" : "text-subtle"}`}>
              {practicedToday ? "Practiced today" : "No reps yet today"}
            </span>
          ) : null}
        </div>
        <dl className="mt-4 grid grid-cols-4 gap-2">
          <Stat label="Streak" value={hydrated ? `${streak}d` : "–"} icon={<FlameIcon size={14} />} highlight={streak > 0} />
          <Stat label="Katas" value={hydrated ? `${completed}/${katas.length}` : "–"} />
          <Stat label="Reps" value={hydrated ? String(reps) : "–"} />
          <Stat label="Minutes" value={hydrated ? String(minutes) : "–"} />
        </dl>
        <div className="mt-5">
          <ActivityHeatmap weeks={18} />
        </div>
      </div>

      <div className="grid gap-4">
        <SuggestionCard
          label={hydrated && completed > 0 ? "Next new kata" : "Start here"}
          kata={hydrated ? next : katas[0]}
          empty="You have completed every kata. Keep repeating them: katas are meant to be practiced again and again."
        />
        <SuggestionCard
          label="Review rep"
          kata={hydrated ? review : undefined}
          detail={hydrated && review ? `Last practiced ${formatRelativeDay(lastCompletion(progress, review.slug)!)}` : undefined}
          empty="Complete a kata and it will come back here for a repeat, least recently practiced first."
        />
      </div>
    </section>
  );
}

function Stat({ label, value, icon, highlight }: { label: string; value: string; icon?: React.ReactNode; highlight?: boolean }) {
  return (
    <div className="rounded-lg bg-surface-2 px-2 py-2.5 text-center">
      <dt className="text-[11px] uppercase tracking-wide text-subtle">{label}</dt>
      <dd className={`mt-0.5 flex items-center justify-center gap-1 text-lg font-semibold tabular-nums ${highlight ? "text-accent-text" : ""}`}>
        {icon}
        {value}
      </dd>
    </div>
  );
}

function SuggestionCard({ label, kata, detail, empty }: { label: string; kata?: KataSummary; detail?: string; empty: string }) {
  if (!kata) {
    return (
      <div className="flex flex-col justify-center rounded-xl border border-dashed border-line-strong bg-surface p-5">
        <p className="text-xs font-semibold uppercase tracking-wide text-subtle">{label}</p>
        <p className="mt-2 text-sm text-muted">{empty}</p>
      </div>
    );
  }

  return (
    <Link href={`/katas/${kata.slug}`} className="group rounded-xl border border-line bg-surface p-5 transition-colors hover:border-line-strong">
      <p className="text-xs font-semibold uppercase tracking-wide text-subtle">{label}</p>
      <div className="mt-2 flex items-start justify-between gap-3">
        <div className="min-w-0">
          <p className="font-mono text-xs text-accent-text">
            Level {kata.level} · {String(kata.order).padStart(2, "0")}
          </p>
          <h3 className="mt-0.5 text-lg font-semibold tracking-tight group-hover:text-accent-text">{kata.title}</h3>
          <p className="mt-1 line-clamp-2 text-sm text-muted">{kata.summary}</p>
        </div>
        <ArrowRightIcon size={18} className="mt-1 shrink-0 text-subtle transition-transform group-hover:translate-x-0.5" />
      </div>
      <p className="mt-3 flex items-center gap-1 text-xs text-subtle">
        <ClockIcon size={13} /> {kata.minutes} min{detail ? ` · ${detail}` : ""}
      </p>
    </Link>
  );
}
