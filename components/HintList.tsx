"use client";

import { revealHints, useProgress } from "@/lib/progress";
import { LightbulbIcon } from "./icons";

export function HintList({ slug, hints }: { slug: string; hints: string[] }) {
  const progress = useProgress();
  const revealed = Math.min(progress.hintsRevealed[slug] ?? 0, hints.length);

  if (hints.length === 0) return null;

  return (
    <div className="space-y-3">
      <ol className="space-y-2">
        {hints.slice(0, revealed).map((hint, i) => (
          <li key={i} className="flex gap-3 rounded-lg border border-line bg-surface p-3">
            <span className="mt-0.5 grid h-5 w-5 shrink-0 place-items-center rounded-full bg-accent-soft text-[11px] font-semibold text-accent-text">
              {i + 1}
            </span>
            <div className="prose-kata prose-compact min-w-0" dangerouslySetInnerHTML={{ __html: hint }} />
          </li>
        ))}
      </ol>
      <div className="flex flex-wrap items-center gap-3">
        {revealed < hints.length ? (
          <button
            type="button"
            onClick={() => revealHints(slug, revealed + 1)}
            className="inline-flex items-center gap-2 rounded-lg border border-line bg-surface px-3 py-2 text-sm font-medium hover:border-line-strong"
          >
            <LightbulbIcon size={16} />
            {revealed === 0 ? "Show first hint" : `Show hint ${revealed + 1} of ${hints.length}`}
          </button>
        ) : (
          <p className="text-sm text-subtle">All {hints.length} hints shown.</p>
        )}
        {revealed === 0 ? (
          <p className="text-xs text-subtle">Hints go from gentle nudges to near-solutions.</p>
        ) : null}
      </div>
    </div>
  );
}
