"use client";

import { useProgress } from "@/lib/progress";
import { reps } from "@/lib/stats";
import { CheckCircleIcon, CircleIcon } from "./icons";

export function KataStatus({ slug, showReps = false }: { slug: string; showReps?: boolean }) {
  const progress = useProgress();
  const count = reps(progress, slug);

  if (count === 0) {
    return (
      <span className="inline-flex items-center text-line-strong" title="Not completed yet">
        <CircleIcon size={20} />
        <span className="sr-only">Not completed</span>
      </span>
    );
  }

  return (
    <span className="inline-flex items-center gap-1 text-ok" title={`Completed ${count} time${count === 1 ? "" : "s"}`}>
      <CheckCircleIcon size={20} />
      {showReps && count > 1 ? <span className="text-xs font-semibold tabular-nums">×{count}</span> : null}
      <span className="sr-only">Completed</span>
    </span>
  );
}
