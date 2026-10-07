"use client";

import Link from "next/link";
import { useRef, useState } from "react";
import type { KataSummary } from "@/lib/katas";
import type { LevelInfo } from "@/lib/levels";
import { exportProgress, importProgress, resetProgress, useHydrated, useProgress } from "@/lib/progress";
import {
  bestSeconds,
  currentStreak,
  formatDuration,
  formatRelativeDay,
  lastCompletion,
  longestStreak,
  reps,
  totals,
} from "@/lib/stats";
import { ActivityHeatmap } from "./ActivityHeatmap";
import { KataStatus } from "./KataStatus";

const days = (n: number) => `${n} day${n === 1 ? "" : "s"}`;

function timestamp(date = new Date()): string {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${date.getFullYear()}${pad(date.getMonth() + 1)}${pad(date.getDate())}_${pad(date.getHours())}${pad(date.getMinutes())}`;
}

export function ProgressDashboard({ katas, levels }: { katas: KataSummary[]; levels: LevelInfo[] }) {
  const hydrated = useHydrated();
  const progress = useProgress();
  const fileInput = useRef<HTMLInputElement>(null);
  const [message, setMessage] = useState<{ tone: "ok" | "error"; text: string } | null>(null);

  const { reps: totalReps, minutes, katas: completed } = totals(progress);

  const download = () => {
    const blob = new Blob([exportProgress()], { type: "application/json" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = `dotnet-katas-progress_${timestamp()}.json`;
    link.click();
    URL.revokeObjectURL(url);
    setMessage({ tone: "ok", text: `Saved ${link.download} to your downloads folder.` });
  };

  const upload = async (file: File | undefined) => {
    if (!file) return;
    const result = importProgress(await file.text());
    setMessage(result.ok ? { tone: "ok", text: `Imported ${file.name}.` } : { tone: "error", text: result.error });
    if (fileInput.current) fileInput.current.value = "";
  };

  const reset = () => {
    if (window.confirm("Delete all progress stored in this browser? Export first if you want to keep it.")) {
      resetProgress();
      setMessage({ tone: "ok", text: "Progress reset." });
    }
  };

  const value = (v: string | number) => (hydrated ? v : "–");

  return (
    <div className="space-y-10">
      <section aria-label="Summary" className="rounded-xl border border-line bg-surface p-5">
        <dl className="grid grid-cols-2 gap-3 sm:grid-cols-5">
          <Stat label="Current streak" value={value(days(currentStreak(progress)))} />
          <Stat label="Longest streak" value={value(days(longestStreak(progress)))} />
          <Stat label="Katas completed" value={value(`${completed} / ${katas.length}`)} />
          <Stat label="Total reps" value={value(totalReps)} />
          <Stat label="Timed minutes" value={value(minutes)} />
        </dl>
        <div className="mt-6">
          <ActivityHeatmap weeks={26} />
        </div>
      </section>

      {levels.map((level) => {
        const levelKatas = katas.filter((k) => k.level === level.number);
        return (
          <section key={level.number} aria-labelledby={`level-${level.number}`}>
            <h2 id={`level-${level.number}`} className="text-lg font-semibold tracking-tight">
              Level {level.number} · {level.title}
            </h2>
            <div className="mt-3 overflow-x-auto rounded-xl border border-line bg-surface">
              <table className="w-full min-w-[40rem] table-fixed text-sm">
                <colgroup>
                  <col className="w-12" />
                  <col />
                  <col className="w-16" />
                  <col className="w-24" />
                  <col className="w-32" />
                  <col className="w-40" />
                </colgroup>
                <thead className="border-b border-line bg-surface-2 text-left text-xs uppercase tracking-wide text-subtle">
                  <tr>
                    <th className="w-10 px-4 py-2 font-medium">
                      <span className="sr-only">Status</span>
                    </th>
                    <th className="px-2 py-2 font-medium">Kata</th>
                    <th className="px-2 py-2 text-right font-medium">Reps</th>
                    <th className="px-2 py-2 text-right font-medium">Best time</th>
                    <th className="px-2 py-2 font-medium">Last practiced</th>
                    <th className="px-4 py-2 font-medium">Help used</th>
                  </tr>
                </thead>
                <tbody>
                  {levelKatas.map((kata) => {
                    const best = bestSeconds(progress, kata.slug);
                    const last = lastCompletion(progress, kata.slug);
                    const hints = progress.hintsRevealed[kata.slug] ?? 0;
                    const sawSolution = progress.solutionViewed[kata.slug];
                    return (
                      <tr key={kata.slug} className="border-b border-line last:border-0">
                        <td className="px-4 py-2.5">
                          <KataStatus slug={kata.slug} />
                        </td>
                        <td className="px-2 py-2.5">
                          <Link href={`/katas/${kata.slug}`} className="font-medium hover:text-accent-text">
                            {kata.title}
                          </Link>
                        </td>
                        <td className="px-2 py-2.5 text-right tabular-nums">{hydrated ? reps(progress, kata.slug) : "–"}</td>
                        <td className="px-2 py-2.5 text-right tabular-nums">{hydrated && best ? formatDuration(best) : "–"}</td>
                        <td className="px-2 py-2.5 text-muted">{hydrated && last ? formatRelativeDay(last) : "–"}</td>
                        <td className="px-4 py-2.5 text-muted">
                          {hydrated
                            ? [hints > 0 ? `${hints} hint${hints === 1 ? "" : "s"}` : null, sawSolution ? "solution" : null]
                                .filter(Boolean)
                                .join(", ") || "none"
                            : "–"}
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </section>
        );
      })}

      <section aria-labelledby="data" className="rounded-xl border border-line bg-surface p-5">
        <h2 id="data" className="text-lg font-semibold tracking-tight">
          Your data
        </h2>
        <p className="mt-1 max-w-2xl text-sm text-muted">
          Progress lives in this browser&apos;s local storage only. Export it to move between devices (for example your
          laptop and tablet) or to keep a backup, then import the file on the other device.
        </p>
        <div className="mt-4 flex flex-wrap gap-2">
          <button
            type="button"
            onClick={download}
            className="rounded-lg bg-accent px-3 py-2 text-sm font-semibold text-accent-fg hover:bg-accent-hover"
          >
            Export JSON
          </button>
          <button
            type="button"
            onClick={() => fileInput.current?.click()}
            className="rounded-lg bg-surface-2 px-3 py-2 text-sm font-medium ring-1 ring-line hover:ring-line-strong"
          >
            Import JSON
          </button>
          <input
            ref={fileInput}
            type="file"
            accept="application/json,.json"
            className="hidden"
            onChange={(event) => upload(event.target.files?.[0])}
          />
          <button type="button" onClick={reset} className="rounded-lg px-3 py-2 text-sm font-medium text-muted hover:text-fg">
            Reset progress
          </button>
        </div>
        {message ? (
          <p role="status" className={`mt-3 text-sm ${message.tone === "ok" ? "text-ok" : "text-warn"}`}>
            {message.text}
          </p>
        ) : null}
      </section>
    </div>
  );
}

function Stat({ label, value }: { label: string; value: string | number }) {
  return (
    <div className="rounded-lg bg-surface-2 px-3 py-3">
      <dt className="text-[11px] uppercase tracking-wide text-subtle">{label}</dt>
      <dd className="mt-1 text-xl font-semibold tabular-nums">{value}</dd>
    </div>
  );
}
