import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { CodeTabs } from "@/components/CodeTabs";
import { CopyButton } from "@/components/CopyButton";
import { HintList } from "@/components/HintList";
import { ArrowLeftIcon, ArrowRightIcon, ClockIcon, DownloadIcon, ExternalIcon, FlaskIcon, TerminalIcon } from "@/components/icons";
import { PracticePanel } from "@/components/PracticePanel";
import { SolutionReveal } from "@/components/SolutionReveal";
import { Chip, LevelBadge, SectionHeading } from "@/components/ui";
import { getAllKatas, getKata } from "@/lib/katas";
import { getLevel } from "@/lib/levels";
import { highlight, renderMarkdown } from "@/lib/render";
import { anchorFor } from "@/lib/slug";
import { kataFilter, kataZipName, WORKSPACE_ZIP } from "@/lib/workspace";

type Props = { params: Promise<{ slug: string }> };

export const dynamicParams = false;

export function generateStaticParams() {
  return getAllKatas().map((kata) => ({ slug: kata.slug }));
}

export async function generateMetadata({ params }: Props): Promise<Metadata> {
  const kata = getKata((await params).slug);
  return kata ? { title: kata.title, description: kata.summary } : {};
}

export default async function KataPage({ params }: Props) {
  const { slug } = await params;
  const kata = getKata(slug);
  if (!kata) notFound();

  const all = getAllKatas();
  const index = all.findIndex((k) => k.slug === kata.slug);
  const previous = all[index - 1];
  const next = all[index + 1];
  const level = getLevel(kata.level);

  const [descriptionHtml, notesHtml, starterHtml, testsHtml, solutionHtml, hintsHtml] = await Promise.all([
    renderMarkdown(kata.descriptionMd),
    renderMarkdown(kata.notesMd),
    highlight(kata.starter),
    highlight(kata.tests),
    highlight(kata.solution),
    Promise.all(kata.hintsMd.map((hint) => renderMarkdown(hint))),
  ]);

  const filter = kataFilter(kata);
  const testCommand = `dotnet test --filter "${filter}"`;
  const watchCommand = `./kata.ps1 -Kata ${kata.slug} -Watch`;

  return (
    <div className="mx-auto max-w-6xl px-4 py-8 sm:px-6 sm:py-10">
      <nav aria-label="Breadcrumb" className="mb-5 flex flex-wrap items-center gap-1.5 text-sm text-subtle">
        <Link href="/levels" className="hover:text-fg">
          Levels
        </Link>
        <span aria-hidden>/</span>
        <Link href={`/levels/${kata.level}`} className="hover:text-fg">
          Level {kata.level} · {level?.title}
        </Link>
      </nav>

      <header className="space-y-3">
        <div className="flex flex-wrap items-center gap-x-4 gap-y-2 text-sm text-subtle">
          <LevelBadge level={kata.level} />
          <span className="inline-flex items-center gap-1">
            <ClockIcon size={15} /> {kata.minutes} min
          </span>
          <span className="inline-flex items-center gap-1">
            <FlaskIcon size={15} /> {kata.testCount} tests
          </span>
          <span className="font-mono text-xs">{kata.namespace}</span>
        </div>
        <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">{kata.title}</h1>
        <p className="max-w-3xl text-base text-muted sm:text-lg">{kata.summary}</p>
        <div className="flex flex-wrap gap-1.5 pt-1">
          {kata.topics.map((topic) => (
            <Chip key={topic} href={`/topics#${anchorFor("topic", topic)}`}>
              {topic}
            </Chip>
          ))}
        </div>
      </header>

      <div className="mt-8 grid gap-8 lg:grid-cols-[minmax(0,1fr)_19rem]">
        <aside className="space-y-4 lg:sticky lg:top-20 lg:order-last lg:self-start">
          <PracticePanel slug={kata.slug} minutes={kata.minutes} />

          <section aria-label="Run locally" className="rounded-xl border border-line bg-surface p-4">
            <h2 className="flex items-center gap-2 text-sm font-semibold">
              <TerminalIcon size={16} /> Run locally
            </h2>
            <p className="mt-2 text-xs text-subtle">From the workspace folder:</p>
            <div className="mt-1.5 space-y-2">
              <CommandLine command={testCommand} />
              <CommandLine command={watchCommand} />
            </div>
            <div className="mt-4 grid grid-cols-2 gap-2">
              <a
                href={`/download/${kataZipName(kata.slug)}`}
                className="inline-flex items-center justify-center gap-1.5 rounded-lg bg-surface-2 px-2 py-2 text-xs font-medium ring-1 ring-line hover:ring-line-strong"
              >
                <DownloadIcon size={14} /> This kata
              </a>
              <a
                href={`/download/${WORKSPACE_ZIP}`}
                className="inline-flex items-center justify-center gap-1.5 rounded-lg bg-surface-2 px-2 py-2 text-xs font-medium ring-1 ring-line hover:ring-line-strong"
              >
                <DownloadIcon size={14} /> All katas
              </a>
            </div>
            <p className="mt-2 text-[11px] text-subtle">
              First time? See <Link href="/setup" className="underline underline-offset-2">workspace setup</Link>.
            </p>
          </section>

          <section aria-label="Concepts" className="rounded-xl border border-line bg-surface p-4">
            <h2 className="text-sm font-semibold">Keywords & syntax</h2>
            <div className="mt-2 flex flex-wrap gap-1.5">
              {kata.keywords.map((keyword) => (
                <Chip key={keyword} mono href={`/topics#${anchorFor("kw", keyword)}`}>
                  {keyword}
                </Chip>
              ))}
            </div>
            <h2 className="mt-4 text-sm font-semibold">APIs</h2>
            <div className="mt-2 flex flex-wrap gap-1.5">
              {kata.apis.map((api) => (
                <Chip key={api} mono href={`/topics#${anchorFor("api", api)}`}>
                  {api}
                </Chip>
              ))}
            </div>
          </section>
        </aside>

        <div className="min-w-0 space-y-12">
          <section aria-label="Description" className="prose-kata" dangerouslySetInnerHTML={{ __html: descriptionHtml }} />

          <section aria-labelledby="code">
            <SectionHeading id="code">Code</SectionHeading>
            <p className="mb-4 text-sm text-muted">
              Implement the members in <code className="font-mono text-[0.85em]">Kata.cs</code> until every test in{" "}
              <code className="font-mono text-[0.85em]">Tests.cs</code> passes. Start the timer when you open your editor.
            </p>
            <CodeTabs
              files={[
                { name: "Kata.cs", html: starterHtml, code: kata.starter },
                { name: "Tests.cs", html: testsHtml, code: kata.tests },
              ]}
            />
          </section>

          {hintsHtml.length > 0 ? (
            <section aria-labelledby="hints">
              <SectionHeading id="hints">Hints</SectionHeading>
              <HintList slug={kata.slug} hints={hintsHtml} />
            </section>
          ) : null}

          <section aria-labelledby="solution">
            <SectionHeading id="solution">Reference solution</SectionHeading>
            <SolutionReveal slug={kata.slug} solutionHtml={solutionHtml} solutionCode={kata.solution} notesHtml={notesHtml} />
          </section>

          {kata.docs.length > 0 ? (
            <section aria-labelledby="reading">
              <SectionHeading id="reading">Further reading</SectionHeading>
              <ul className="space-y-2">
                {kata.docs.map((doc) => (
                  <li key={doc.url}>
                    <a
                      href={doc.url}
                      target="_blank"
                      rel="noreferrer noopener"
                      className="inline-flex items-center gap-1.5 text-sm text-accent-text underline-offset-2 hover:underline"
                    >
                      {doc.title} <ExternalIcon size={13} />
                    </a>
                  </li>
                ))}
              </ul>
            </section>
          ) : null}
        </div>
      </div>

      <nav aria-label="Kata navigation" className="mt-14 grid gap-3 border-t border-line pt-6 sm:grid-cols-2">
        {previous ? (
          <Link href={`/katas/${previous.slug}`} className="group rounded-xl border border-line bg-surface p-4 hover:border-line-strong">
            <span className="flex items-center gap-1 text-xs text-subtle">
              <ArrowLeftIcon size={13} /> Previous · Level {previous.level}
            </span>
            <span className="mt-1 block font-medium group-hover:text-accent-text">{previous.title}</span>
          </Link>
        ) : (
          <span />
        )}
        {next ? (
          <Link
            href={`/katas/${next.slug}`}
            className="group rounded-xl border border-line bg-surface p-4 text-right hover:border-line-strong"
          >
            <span className="flex items-center justify-end gap-1 text-xs text-subtle">
              Next · Level {next.level} <ArrowRightIcon size={13} />
            </span>
            <span className="mt-1 block font-medium group-hover:text-accent-text">{next.title}</span>
          </Link>
        ) : null}
      </nav>
    </div>
  );
}

function CommandLine({ command }: { command: string }) {
  return (
    <div className="flex items-start gap-2 rounded-lg border border-line bg-surface-2 p-2">
      <code className="min-w-0 flex-1 font-mono text-[11px] leading-relaxed break-all">{command}</code>
      <CopyButton text={command} label="Copy" className="shrink-0 !px-1.5" />
    </div>
  );
}
