import Link from "next/link";
import type { ReactNode } from "react";

export function LevelBadge({ level }: { level: number }) {
  return (
    <span className="inline-flex items-center rounded-md bg-accent-soft px-2 py-0.5 text-xs font-semibold text-accent-text">
      Level {level}
    </span>
  );
}

export function Chip({ children, href, mono = false }: { children: ReactNode; href?: string; mono?: boolean }) {
  const className = `inline-flex max-w-full items-center rounded-md border border-line bg-surface px-2 py-0.5 text-xs text-muted [overflow-wrap:anywhere] ${
    mono ? "font-mono" : ""
  } ${href ? "hover:border-line-strong hover:text-fg" : ""}`;

  return href ? (
    <Link href={href} className={className}>
      {children}
    </Link>
  ) : (
    <span className={className}>{children}</span>
  );
}

export function SectionHeading({ id, children, aside }: { id?: string; children: ReactNode; aside?: ReactNode }) {
  return (
    <div className="mb-4 flex items-end justify-between gap-4 border-b border-line pb-2">
      <h2 id={id} className="text-lg font-semibold tracking-tight">
        {children}
      </h2>
      {aside}
    </div>
  );
}

export function PageHeader({ eyebrow, title, children }: { eyebrow?: ReactNode; title: ReactNode; children?: ReactNode }) {
  return (
    <header className="space-y-3">
      {eyebrow ? <div className="flex flex-wrap items-center gap-2 text-sm text-subtle">{eyebrow}</div> : null}
      <h1 className="text-3xl font-semibold tracking-tight sm:text-4xl">{title}</h1>
      {children ? <div className="max-w-3xl text-base text-muted sm:text-lg">{children}</div> : null}
    </header>
  );
}
