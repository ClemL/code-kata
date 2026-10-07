import type { Metadata } from "next";
import { TopicIndex, type TopicEntry, type TopicGroup } from "@/components/TopicIndex";
import { PageHeader } from "@/components/ui";
import { getAllKatas, type Kata } from "@/lib/katas";
import { anchorFor } from "@/lib/slug";

export const metadata: Metadata = {
  title: "Topics, keywords & APIs",
  description: "Find katas by C# keyword, concept, or .NET API.",
};

function buildIndex(katas: Kata[], prefix: string, pick: (kata: Kata) => string[]): TopicEntry[] {
  const map = new Map<string, TopicEntry>();
  for (const kata of katas) {
    for (const term of pick(kata)) {
      const entry = map.get(term) ?? { id: anchorFor(prefix, term), term, katas: [] };
      entry.katas.push({ slug: kata.slug, title: kata.title, level: kata.level });
      map.set(term, entry);
    }
  }
  return [...map.values()].sort((a, b) =>
    a.term.replace(/^[^a-z0-9]+/i, "").localeCompare(b.term.replace(/^[^a-z0-9]+/i, ""), "en", { sensitivity: "base" }),
  );
}

export default function TopicsPage() {
  const katas = getAllKatas();

  const groups: TopicGroup[] = [
    {
      id: "keywords",
      title: "Keywords & syntax",
      description: "C# keywords, contextual keywords, and operators, with the katas that exercise them.",
      mono: true,
      entries: buildIndex(katas, "kw", (k) => k.keywords),
    },
    {
      id: "concepts",
      title: "Concepts",
      description: "Language features and engineering topics.",
      mono: false,
      entries: buildIndex(katas, "topic", (k) => k.topics),
    },
    {
      id: "apis",
      title: ".NET APIs",
      description: "Types and methods from the base class library that the reference solutions rely on.",
      mono: true,
      entries: buildIndex(katas, "api", (k) => k.apis),
    },
  ];

  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
      <PageHeader title="Topics, keywords & APIs">
        A cross-reference of every kata. Use it to find practice for a specific keyword before a code review, or to see
        which parts of the language you have not touched in a while.
      </PageHeader>
      <div className="mt-6">
        <TopicIndex groups={groups} />
      </div>
    </div>
  );
}
