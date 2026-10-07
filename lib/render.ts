import "server-only";
import { Marked } from "marked";
import { createHighlighter, type Highlighter } from "shiki";

const THEMES = { light: "github-light-default", dark: "github-dark-default" } as const;
const LANGS = ["csharp", "powershell", "bash", "json", "xml", "yaml"] as const;

let highlighterPromise: Promise<Highlighter> | undefined;

function getHighlighter(): Promise<Highlighter> {
  highlighterPromise ??= createHighlighter({ themes: Object.values(THEMES), langs: [...LANGS] });
  return highlighterPromise;
}

function normalizeLang(lang: string | undefined): string {
  const value = (lang ?? "").trim().toLowerCase();
  if (value === "cs" || value === "c#") return "csharp";
  if (value === "ps1" || value === "pwsh") return "powershell";
  if (value === "sh" || value === "shell") return "bash";
  return (LANGS as readonly string[]).includes(value) ? value : "text";
}

function toHtml(highlighter: Highlighter, code: string, lang: string | undefined): string {
  return highlighter.codeToHtml(code.replace(/\n$/, ""), {
    lang: normalizeLang(lang),
    themes: THEMES,
    defaultColor: false,
  });
}

export async function highlight(code: string, lang = "csharp"): Promise<string> {
  return toHtml(await getHighlighter(), code, lang);
}

export async function renderMarkdown(markdown: string): Promise<string> {
  const highlighter = await getHighlighter();
  const marked = new Marked({
    gfm: true,
    renderer: {
      code({ text, lang }) {
        return toHtml(highlighter, text, lang);
      },
      link({ href, title, tokens }) {
        const text = this.parser.parseInline(tokens);
        const external = /^https?:\/\//.test(href);
        const titleAttr = title ? ` title="${escapeAttr(title)}"` : "";
        const target = external ? ' target="_blank" rel="noreferrer noopener"' : "";
        return `<a href="${escapeAttr(href)}"${titleAttr}${target}>${text}</a>`;
      },
    },
  });

  return marked.parse(markdown, { async: false });
}

/** Renders a single line of markdown without a wrapping paragraph. */
export async function renderInlineMarkdown(markdown: string): Promise<string> {
  const marked = new Marked({ gfm: true });
  return marked.parseInline(markdown, { async: false });
}

function escapeAttr(value: string): string {
  return value.replace(/&/g, "&amp;").replace(/"/g, "&quot;").replace(/</g, "&lt;");
}
