import "server-only";
import fs from "node:fs";
import path from "node:path";
import { parse as parseYaml } from "yaml";

export type DocLink = { title: string; url: string };

export type Kata = {
  slug: string;
  level: number;
  order: number;
  folder: string;
  title: string;
  minutes: number;
  summary: string;
  topics: string[];
  keywords: string[];
  apis: string[];
  docs: DocLink[];
  namespace: string;
  testCount: number;
  /** Markdown for everything except Hints and Notes. */
  descriptionMd: string;
  hintsMd: string[];
  notesMd: string;
  starter: string;
  solution: string;
  tests: string;
};

/** A lightweight projection safe to pass to client components. */
export type KataSummary = Pick<
  Kata,
  "slug" | "level" | "order" | "title" | "minutes" | "summary" | "topics" | "namespace" | "testCount"
>;

const KATAS_DIR = path.join(process.cwd(), "katas");

let cache: Kata[] | undefined;

export function getAllKatas(): Kata[] {
  if (cache) return cache;

  const katas: Kata[] = [];
  const levelDirs = fs
    .readdirSync(KATAS_DIR, { withFileTypes: true })
    .filter((d) => d.isDirectory() && /^level-\d+$/.test(d.name))
    .map((d) => d.name)
    .sort();

  for (const levelDir of levelDirs) {
    const level = Number(levelDir.split("-")[1]);
    const kataDirs = fs
      .readdirSync(path.join(KATAS_DIR, levelDir), { withFileTypes: true })
      .filter((d) => d.isDirectory())
      .map((d) => d.name)
      .sort();

    for (const kataDir of kataDirs) {
      katas.push(loadKata(level, levelDir, kataDir));
    }
  }

  const slugs = new Set<string>();
  for (const kata of katas) {
    if (slugs.has(kata.slug)) throw new Error(`Duplicate kata slug: ${kata.slug}`);
    slugs.add(kata.slug);
  }

  cache = katas;
  return katas;
}

export function getKata(slug: string): Kata | undefined {
  return getAllKatas().find((kata) => kata.slug === slug);
}

export function getKatasForLevel(level: number): Kata[] {
  return getAllKatas().filter((kata) => kata.level === level);
}

export function toSummary(kata: Kata): KataSummary {
  return {
    slug: kata.slug,
    level: kata.level,
    order: kata.order,
    title: kata.title,
    minutes: kata.minutes,
    summary: kata.summary,
    topics: kata.topics,
    namespace: kata.namespace,
    testCount: kata.testCount,
  };
}

function loadKata(level: number, levelDir: string, kataDir: string): Kata {
  const dir = path.join(KATAS_DIR, levelDir, kataDir);
  const read = (file: string) => fs.readFileSync(path.join(dir, file), "utf8").replace(/\r\n/g, "\n");

  const { data, body } = splitFrontmatter(read("kata.md"), `${levelDir}/${kataDir}/kata.md`);
  const starter = read("Starter.cs");
  const solution = read("Solution.cs");
  const tests = read("Tests.cs");

  const match = /^(\d+)-(.+)$/.exec(kataDir);
  if (!match) throw new Error(`Kata folder must look like 01-name: ${kataDir}`);

  const namespace = /^namespace\s+([\w.]+)\s*;/m.exec(starter)?.[1];
  if (!namespace) throw new Error(`No file-scoped namespace in ${kataDir}/Starter.cs`);

  const sections = splitSections(body);
  const hints = sections.find((s) => s.heading === "Hints");
  const notes = sections.find((s) => s.heading === "Notes");
  const description = sections
    .filter((s) => s !== hints && s !== notes)
    .map((s) => (s.heading ? `## ${s.heading}\n\n${s.content}` : s.content))
    .join("\n\n");

  const fm = data as Record<string, unknown>;
  const list = (key: string) => ((fm[key] as unknown[] | undefined) ?? []).map(String);

  return {
    slug: match[2],
    level,
    order: Number(match[1]),
    folder: `${levelDir}/${kataDir}`,
    title: String(fm.title),
    minutes: Number(fm.minutes ?? 30),
    summary: String(fm.summary ?? ""),
    topics: list("topics"),
    keywords: list("keywords"),
    apis: list("apis"),
    docs: ((fm.docs as DocLink[] | undefined) ?? []).map((d) => ({ title: String(d.title), url: String(d.url) })),
    namespace,
    testCount: countTests(tests),
    descriptionMd: description.trim(),
    hintsMd: hints ? splitBullets(hints.content) : [],
    notesMd: notes?.content.trim() ?? "",
    starter,
    solution,
    tests,
  };
}

function splitFrontmatter(source: string, file: string): { data: unknown; body: string } {
  const match = /^---\n([\s\S]*?)\n---\n?([\s\S]*)$/.exec(source);
  if (!match) throw new Error(`Missing frontmatter in ${file}`);
  return { data: parseYaml(match[1]), body: match[2] };
}

type Section = { heading: string | null; content: string };

function splitSections(body: string): Section[] {
  const sections: Section[] = [];
  let current: Section = { heading: null, content: "" };
  let inFence = false;

  for (const line of body.split("\n")) {
    if (/^\s*```/.test(line)) inFence = !inFence;
    const heading = !inFence ? /^## (.+)$/.exec(line) : null;
    if (heading) {
      if (current.heading || current.content.trim()) sections.push(current);
      current = { heading: heading[1].trim(), content: "" };
    } else {
      current.content += `${line}\n`;
    }
  }

  if (current.heading || current.content.trim()) sections.push(current);
  return sections.map((s) => ({ ...s, content: s.content.trim() }));
}

/** Splits a top-level "- item" list into one markdown string per item (continuation lines included). */
function splitBullets(markdown: string): string[] {
  const items: string[] = [];
  for (const line of markdown.split("\n")) {
    if (line.startsWith("- ")) items.push(line.slice(2));
    else if (items.length > 0 && line.trim()) items[items.length - 1] += `\n${line.trim()}`;
  }
  return items;
}

/** Counts test cases the way the runner does: one per [TestMethod], or one per [DataRow] when present. */
function countTests(tests: string): number {
  const chunks = tests.split("[TestMethod]").slice(1);
  return chunks.reduce((total, chunk) => {
    const header = chunk.slice(0, chunk.search(/\bpublic\b/));
    const rows = (header.match(/\[DataRow\(/g) ?? []).length;
    return total + Math.max(rows, 1);
  }, 0);
}
