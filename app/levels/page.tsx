import type { Metadata } from "next";
import { DownloadIcon } from "@/components/icons";
import { LevelCard } from "@/components/LevelCard";
import { PageHeader } from "@/components/ui";
import { getKatasForLevel } from "@/lib/katas";
import { LEVELS } from "@/lib/levels";
import { WORKSPACE_ZIP } from "@/lib/workspace";

export const metadata: Metadata = { title: "Levels" };

export default function LevelsPage() {
  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
      <PageHeader title="Levels">
        Five levels, roughly ordered from syntax to framework internals. Levels 1-2 rebuild fluency; levels 3-5 are where
        most senior-level practice happens.
      </PageHeader>
      <a
        href={`/download/${WORKSPACE_ZIP}`}
        className="mt-5 inline-flex items-center gap-2 rounded-lg bg-surface px-3 py-2 text-sm font-medium ring-1 ring-line hover:ring-line-strong"
      >
        <DownloadIcon size={16} /> Download every kata as one project
      </a>
      <div className="mt-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {LEVELS.map((level) => {
          const katas = getKatasForLevel(level.number);
          return (
            <LevelCard
              key={level.number}
              level={level}
              slugs={katas.map((k) => k.slug)}
              minutes={katas.reduce((sum, k) => sum + k.minutes, 0)}
            />
          );
        })}
      </div>
    </div>
  );
}
