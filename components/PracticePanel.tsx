"use client";

import { useEffect, useState } from "react";
import {
  completeKata,
  elapsedMs,
  pauseTimer,
  resetTimer,
  startTimer,
  undoLastCompletion,
  useHydrated,
  useProgress,
  useTimer,
} from "@/lib/progress";
import { bestSeconds, formatDuration, formatRelativeDay, lastCompletion, reps } from "@/lib/stats";
import { CheckCircleIcon, PauseIcon, PlayIcon, ResetIcon } from "./icons";

export function PracticePanel({ slug, minutes }: { slug: string; minutes: number }) {
  const hydrated = useHydrated();
  const progress = useProgress();
  const timer = useTimer(slug);
  const running = timer.startedAt !== null;
  const [now, setNow] = useState(() => Date.now());
  const [justLogged, setJustLogged] = useState<number | null>(null);

  useEffect(() => {
    if (!running) return;
    setNow(Date.now());
    const id = window.setInterval(() => setNow(Date.now()), 1000);
    return () => window.clearInterval(id);
  }, [running]);

  const elapsedSeconds = Math.floor(elapsedMs(timer, running ? now : 0) / 1000);
  const targetSeconds = minutes * 60;
  const ratio = Math.min(elapsedSeconds / targetSeconds, 1);
  const over = elapsedSeconds > targetSeconds;

  const count = reps(progress, slug);
  const best = bestSeconds(progress, slug);
  const last = lastCompletion(progress, slug);

  const markDone = () => {
    completeKata(slug, elapsedSeconds);
    resetTimer(slug);
    setJustLogged(count + 1);
  };

  return (
    <section aria-label="Practice" className="rounded-xl border border-line bg-surface p-4">
      <div className="flex items-baseline justify-between">
        <h2 className="text-sm font-semibold">Practice</h2>
        <span className="text-xs text-subtle">Timebox {minutes} min</span>
      </div>

      <div className="mt-3 flex items-center gap-3">
        <div
          className={`font-mono text-3xl font-semibold tabular-nums tracking-tight ${over ? "text-warn" : ""}`}
          aria-live="off"
        >
          {formatDuration(hydrated ? elapsedSeconds : 0)}
        </div>
        <div className="ml-auto flex gap-1.5">
          <button
            type="button"
            onClick={() => (running ? pauseTimer(slug) : startTimer(slug))}
            className="grid h-9 w-9 place-items-center rounded-lg bg-surface-2 ring-1 ring-line hover:ring-line-strong"
            aria-label={running ? "Pause timer" : "Start timer"}
          >
            {running ? <PauseIcon size={16} /> : <PlayIcon size={16} />}
          </button>
          <button
            type="button"
            onClick={() => resetTimer(slug)}
            className="grid h-9 w-9 place-items-center rounded-lg bg-surface-2 ring-1 ring-line hover:ring-line-strong disabled:opacity-40"
            aria-label="Reset timer"
            disabled={elapsedSeconds === 0}
          >
            <ResetIcon size={16} />
          </button>
        </div>
      </div>

      <div className="mt-3 h-1.5 overflow-hidden rounded-full bg-surface-2" aria-hidden>
        <div
          className={`h-full rounded-full transition-[width] duration-500 ${over ? "bg-warn" : "bg-accent"}`}
          style={{ width: `${ratio * 100}%` }}
        />
      </div>

      <button
        type="button"
        onClick={markDone}
        className="mt-4 inline-flex w-full items-center justify-center gap-2 rounded-lg bg-accent px-3 py-2.5 text-sm font-semibold text-accent-fg hover:bg-accent-hover"
      >
        <CheckCircleIcon size={18} />
        {count > 0 ? "Log another rep" : "Mark complete"}
      </button>

      {justLogged !== null && count === justLogged ? (
        <p className="mt-2 text-center text-xs text-ok" role="status">
          Rep {justLogged} logged.{" "}
          <button
            type="button"
            className="underline underline-offset-2"
            onClick={() => {
              undoLastCompletion(slug);
              setJustLogged(null);
            }}
          >
            Undo
          </button>
        </p>
      ) : null}

      <dl className="mt-4 grid grid-cols-3 gap-2 border-t border-line pt-3 text-center">
        <div>
          <dt className="text-[11px] uppercase tracking-wide text-subtle">Reps</dt>
          <dd className="mt-0.5 text-sm font-semibold tabular-nums">{hydrated ? count : "–"}</dd>
        </div>
        <div>
          <dt className="text-[11px] uppercase tracking-wide text-subtle">Best</dt>
          <dd className="mt-0.5 text-sm font-semibold tabular-nums">{hydrated && best ? formatDuration(best) : "–"}</dd>
        </div>
        <div>
          <dt className="text-[11px] uppercase tracking-wide text-subtle">Last</dt>
          <dd className="mt-0.5 text-sm font-semibold">{hydrated && last ? formatRelativeDay(last) : "–"}</dd>
        </div>
      </dl>
    </section>
  );
}
