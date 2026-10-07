import Link from "next/link";
import type { LevelInfo } from "@/lib/levels";
import { ArrowRightIcon } from "./icons";
import { LevelProgress } from "./LevelProgress";

export function LevelCard({ level, slugs, minutes }: { level: LevelInfo; slugs: string[]; minutes: number }) {
  return (
    <Link
      href={`/levels/${level.number}`}
      className="group flex flex-col rounded-xl border border-line bg-surface p-5 transition-colors hover:border-line-strong"
    >
      <div className="flex items-center justify-between">
        <span className="font-mono text-xs font-semibold uppercase tracking-wider text-accent-text">Level {level.number}</span>
        <span className="text-xs text-subtle">
          {slugs.length} katas · ~{Math.round(minutes / 60)} h
        </span>
      </div>
      <h3 className="mt-2 text-lg font-semibold tracking-tight">{level.title}</h3>
      <p className="mt-0.5 text-sm font-medium text-muted">{level.tagline}</p>
      <p className="mt-3 flex-1 text-sm text-subtle">{level.description}</p>
      <div className="mt-5">
        <LevelProgress slugs={slugs} />
      </div>
      <span className="mt-4 inline-flex items-center gap-1 text-sm font-medium text-accent-text">
        Open level <ArrowRightIcon size={14} className="transition-transform group-hover:translate-x-0.5" />
      </span>
    </Link>
  );
}
