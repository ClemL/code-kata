/**
 * Validates every kata folder: required files, frontmatter, unique slugs, and namespace consistency.
 * Run with `npm run validate`.
 */
import fs from "node:fs";
import path from "node:path";
import { parse as parseYaml } from "yaml";

const root = path.join(process.cwd(), "katas");
const required = ["kata.md", "Starter.cs", "Solution.cs", "Tests.cs"];
const errors: string[] = [];
const slugs = new Map<string, string>();
const namespaces = new Map<string, string>();
let count = 0;

for (const level of fs.readdirSync(root).filter((d) => /^level-\d+$/.test(d)).sort()) {
  for (const kata of fs.readdirSync(path.join(root, level)).sort()) {
    const dir = path.join(root, level, kata);
    if (!fs.statSync(dir).isDirectory()) continue;
    count++;
    const where = `${level}/${kata}`;

    for (const file of required) {
      if (!fs.existsSync(path.join(dir, file))) errors.push(`${where}: missing ${file}`);
    }
    if (errors.some((e) => e.startsWith(where))) continue;

    const md = fs.readFileSync(path.join(dir, "kata.md"), "utf8");
    const fm = /^---\n([\s\S]*?)\n---/.exec(md.replace(/\r\n/g, "\n"));
    if (!fm) {
      errors.push(`${where}: kata.md has no frontmatter`);
      continue;
    }

    const data = parseYaml(fm[1]) as Record<string, unknown>;
    for (const key of ["title", "level", "minutes", "summary", "topics", "keywords", "apis", "docs"]) {
      if (data[key] === undefined) errors.push(`${where}: frontmatter is missing "${key}"`);
    }
    if (Number(data.level) !== Number(level.split("-")[1])) errors.push(`${where}: frontmatter level does not match folder`);
    for (const heading of ["## Brief", "## Tasks", "## Hints", "## Notes"]) {
      if (!md.includes(`\n${heading}\n`)) errors.push(`${where}: kata.md is missing "${heading}"`);
    }

    const slug = kata.replace(/^\d+-/, "");
    if (slugs.has(slug)) errors.push(`${where}: slug "${slug}" already used by ${slugs.get(slug)}`);
    slugs.set(slug, where);

    const nsOf = (file: string) =>
      /^namespace\s+([\w.]+)\s*;/m.exec(fs.readFileSync(path.join(dir, file), "utf8"))?.[1];
    const ns = nsOf("Starter.cs");
    if (!ns) errors.push(`${where}: Starter.cs has no file-scoped namespace`);
    for (const file of ["Solution.cs", "Tests.cs"]) {
      if (nsOf(file) !== ns) errors.push(`${where}: ${file} namespace differs from Starter.cs`);
    }
    if (ns && namespaces.has(ns)) errors.push(`${where}: namespace ${ns} already used by ${namespaces.get(ns)}`);
    if (ns) namespaces.set(ns, where);
  }
}

if (errors.length > 0) {
  console.error(`Kata validation failed:\n- ${errors.join("\n- ")}`);
  process.exit(1);
}

console.log(`Validated ${count} katas.`);
