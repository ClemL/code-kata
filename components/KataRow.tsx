import Link from "next/link";
import type { KataSummary } from "@/lib/katas";
import { ClockIcon, FlaskIcon } from "./icons";
import { KataStatus } from "./KataStatus";

export function KataRow({ kata }: { kata: KataSummary }) {
  return (
    <li>
      <Link
        href={`/katas/${kata.slug}`}
        className="group flex gap-4 rounded-xl border border-line bg-surface p-4 transition-colors hover:border-line-strong sm:p-5"
      >
        <span className="mt-0.5 font-mono text-sm text-subtle tabular-nums">
          {kata.level}.{String(kata.order).padStart(2, "0")}
        </span>
        <div className="min-w-0 flex-1">
          <div className="flex items-start justify-between gap-3">
            <h3 className="font-semibold tracking-tight group-hover:text-accent-text">{kata.title}</h3>
            <KataStatus slug={kata.slug} showReps />
          </div>
          <p className="mt-1 text-sm text-muted">{kata.summary}</p>
          <div className="mt-3 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs text-subtle">
            <span className="inline-flex items-center gap-1">
              <ClockIcon size={14} /> {kata.minutes} min
            </span>
            <span className="inline-flex items-center gap-1">
              <FlaskIcon size={14} /> {kata.testCount} tests
            </span>
            <span className="truncate">{kata.topics.join(" · ")}</span>
          </div>
        </div>
      </Link>
    </li>
  );
}
