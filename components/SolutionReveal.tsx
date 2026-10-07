"use client";

import { useState } from "react";
import { markSolutionViewed, useProgress } from "@/lib/progress";
import { isCompleted } from "@/lib/stats";
import { CodeTabs } from "./CodeTabs";
import { EyeIcon } from "./icons";

type Props = {
  slug: string;
  solutionHtml: string;
  solutionCode: string;
  notesHtml: string;
};

export function SolutionReveal({ slug, solutionHtml, solutionCode, notesHtml }: Props) {
  const progress = useProgress();
  const [open, setOpen] = useState(false);
  const done = isCompleted(progress, slug);

  if (!open) {
    return (
      <div className="rounded-xl border border-dashed border-line-strong bg-surface p-5">
        <p className="text-sm text-muted">
          {done
            ? "You have completed this kata. Compare your approach with the reference solution and the design notes."
            : "The reference solution is most useful after your tests are green, or after your timebox runs out. Struggling a little first is the point."}
        </p>
        <button
          type="button"
          onClick={() => {
            setOpen(true);
            markSolutionViewed(slug);
          }}
          className="mt-4 inline-flex items-center gap-2 rounded-lg bg-surface-2 px-3 py-2 text-sm font-medium ring-1 ring-line hover:ring-line-strong"
        >
          <EyeIcon size={16} />
          {done ? "Show solution and notes" : "Show solution anyway"}
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <CodeTabs files={[{ name: "Solution.cs", html: solutionHtml, code: solutionCode }]} />
      {notesHtml ? (
        <div className="rounded-xl border border-line bg-surface p-5">
          <h3 className="mb-3 text-sm font-semibold uppercase tracking-wide text-subtle">Design notes</h3>
          <div className="prose-kata" dangerouslySetInnerHTML={{ __html: notesHtml }} />
        </div>
      ) : null}
      <button type="button" onClick={() => setOpen(false)} className="text-sm text-subtle underline-offset-2 hover:underline">
        Hide solution
      </button>
    </div>
  );
}
