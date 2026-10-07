"use client";

import { useHydrated, useProgress } from "@/lib/progress";
import { isCompleted } from "@/lib/stats";

export function LevelProgress({ slugs, compact = false }: { slugs: string[]; compact?: boolean }) {
  const hydrated = useHydrated();
  const progress = useProgress();
  const done = slugs.filter((slug) => isCompleted(progress, slug)).length;
  const pct = slugs.length === 0 ? 0 : Math.round((done / slugs.length) * 100);

  return (
    <div className={compact ? "" : "space-y-1.5"}>
      {!compact ? (
        <div className="flex justify-between text-xs text-subtle">
          <span>
            {hydrated ? done : 0} of {slugs.length} completed
          </span>
          <span className="tabular-nums">{hydrated ? pct : 0}%</span>
        </div>
      ) : null}
      <div
        className="h-1.5 overflow-hidden rounded-full bg-surface-2"
        role="progressbar"
        aria-valuemin={0}
        aria-valuemax={slugs.length}
        aria-valuenow={hydrated ? done : 0}
        aria-label="Level progress"
      >
        <div className="h-full rounded-full bg-accent transition-[width] duration-500" style={{ width: `${hydrated ? pct : 0}%` }} />
      </div>
    </div>
  );
}
