"use client";

import Link from "next/link";
import { useDeferredValue, useState } from "react";

export type TopicEntry = {
  id: string;
  term: string;
  katas: { slug: string; title: string; level: number }[];
};

export type TopicGroup = { id: string; title: string; description: string; mono: boolean; entries: TopicEntry[] };

export function TopicIndex({ groups }: { groups: TopicGroup[] }) {
  const [query, setQuery] = useState("");
  const deferred = useDeferredValue(query.trim().toLowerCase());

  const matches = (entry: TopicEntry) =>
    !deferred ||
    entry.term.toLowerCase().includes(deferred) ||
    entry.katas.some((kata) => kata.title.toLowerCase().includes(deferred));

  return (
    <div>
      <div className="sticky top-14 z-10 -mx-4 bg-bg/90 px-4 py-3 backdrop-blur sm:-mx-6 sm:px-6">
        <label htmlFor="topic-filter" className="sr-only">
          Filter topics, keywords, and APIs
        </label>
        <input
          id="topic-filter"
          type="search"
          value={query}
          onChange={(event) => setQuery(event.target.value)}
          placeholder="Filter: async, record, Span, LINQ…"
          className="w-full rounded-lg border border-line bg-surface px-3 py-2 text-sm outline-none placeholder:text-subtle focus:border-accent"
        />
        <nav aria-label="Sections" className="mt-2 flex gap-3 text-xs text-subtle">
          {groups.map((group) => (
            <a key={group.id} href={`#${group.id}`} className="hover:text-fg">
              {group.title} ({group.entries.filter(matches).length})
            </a>
          ))}
        </nav>
      </div>

      {groups.map((group) => {
        const visible = group.entries.filter(matches);
        return (
          <section key={group.id} aria-labelledby={group.id} className="mt-8">
            <h2 id={group.id} className="scroll-mt-36 text-xl font-semibold tracking-tight">
              {group.title}
            </h2>
            <p className="mt-1 text-sm text-muted">{group.description}</p>
            {visible.length === 0 ? (
              <p className="mt-4 text-sm text-subtle">No matches.</p>
            ) : (
              <ul className="mt-4 grid grid-cols-1 gap-2 sm:grid-cols-2 lg:grid-cols-3">
                {visible.map((entry) => (
                  <li
                    key={entry.id}
                    id={entry.id}
                    className="min-w-0 scroll-mt-36 rounded-lg border border-line bg-surface px-3 py-2.5 target:border-accent target:ring-2 target:ring-accent/30"
                  >
                    <h3 className={`text-sm font-semibold [overflow-wrap:anywhere] ${group.mono ? "font-mono" : ""}`}>{entry.term}</h3>
                    <p className="mt-1 text-sm leading-relaxed text-muted">
                      {entry.katas.map((kata, i) => (
                        <span key={kata.slug}>
                          {i > 0 ? <span className="text-line-strong"> · </span> : null}
                          <Link href={`/katas/${kata.slug}`} className="hover:text-accent-text">
                            <span className="font-mono text-[11px] text-subtle">L{kata.level}</span> {kata.title}
                          </Link>
                        </span>
                      ))}
                    </p>
                  </li>
                ))}
              </ul>
            )}
          </section>
        );
      })}
    </div>
  );
}
