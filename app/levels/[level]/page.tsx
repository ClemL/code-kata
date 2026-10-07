import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { CopyButton } from "@/components/CopyButton";
import { DownloadIcon } from "@/components/icons";
import { KataRow } from "@/components/KataRow";
import { LevelProgress } from "@/components/LevelProgress";
import { PageHeader } from "@/components/ui";
import { getKatasForLevel, toSummary } from "@/lib/katas";
import { getLevel, LEVELS } from "@/lib/levels";
import { levelFilter, levelZipName } from "@/lib/workspace";

type Props = { params: Promise<{ level: string }> };

export const dynamicParams = false;

export function generateStaticParams() {
  return LEVELS.map((level) => ({ level: String(level.number) }));
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const level = getLevel(Number((await params).level));
  return level ? { title: `Level ${level.number}: ${level.title}`, description: level.description } : {};
}

export default async function LevelPage({ params }: Props) {
  const number = Number((await params).level);
  const level = getLevel(number);
  if (!level) notFound();

  const katas = getKatasForLevel(number);
  const minutes = katas.reduce((sum, k) => sum + k.minutes, 0);
  const tests = katas.reduce((sum, k) => sum + k.testCount, 0);
  const command = `dotnet test --filter "${levelFilter(number)}"`;

  return (
    <div className="mx-auto max-w-4xl px-4 py-10 sm:px-6">
      <nav aria-label="Breadcrumb" className="mb-5 text-sm text-subtle">
        <Link href="/levels" className="hover:text-fg">
          Levels
        </Link>
      </nav>
      <PageHeader
        eyebrow={
          <>
            <span className="font-mono font-semibold uppercase tracking-wider text-accent-text">Level {level.number}</span>
            <span>
              {katas.length} katas · {tests} tests · ~{Math.round(minutes / 60)} h
            </span>
          </>
        }
        title={level.title}
      >
        {level.description}
      </PageHeader>

      <div className="mt-6 grid gap-4 rounded-xl border border-line bg-surface p-4 sm:grid-cols-[1fr_auto] sm:items-center">
        <div className="min-w-0 space-y-3">
          <LevelProgress slugs={katas.map((k) => k.slug)} />
          <div className="flex items-start gap-2 rounded-lg border border-line bg-surface-2 p-2">
            <code className="min-w-0 flex-1 font-mono text-xs break-all">{command}</code>
            <CopyButton text={command} className="shrink-0" />
          </div>
        </div>
        <a
          href={`/download/${levelZipName(number)}`}
          className="inline-flex items-center justify-center gap-2 rounded-lg bg-accent px-3 py-2 text-sm font-semibold text-accent-fg hover:bg-accent-hover"
        >
          <DownloadIcon size={16} /> Level {number} project
        </a>
      </div>

      <ol className="mt-8 space-y-3">
        {katas.map((kata) => (
          <KataRow key={kata.slug} kata={toSummary(kata)} />
        ))}
      </ol>

      <nav aria-label="Level navigation" className="mt-10 flex justify-between text-sm">
        {getLevel(number - 1) ? (
          <Link href={`/levels/${number - 1}`} className="text-muted hover:text-fg">
            ← Level {number - 1}
          </Link>
        ) : (
          <span />
        )}
        {getLevel(number + 1) ? (
          <Link href={`/levels/${number + 1}`} className="text-muted hover:text-fg">
            Level {number + 1} →
          </Link>
        ) : null}
      </nav>
    </div>
  );
}
