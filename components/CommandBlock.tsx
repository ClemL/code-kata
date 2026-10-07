import { CopyButton } from "./CopyButton";

export function CommandBlock({ html, code, label }: { html: string; code: string; label?: string }) {
  return (
    <div className="code-panel overflow-hidden rounded-xl border border-line bg-surface">
      <div className="flex items-center justify-between border-b border-line bg-surface-2 px-3 py-1.5">
        <span className="font-mono text-xs text-subtle">{label ?? "PowerShell"}</span>
        <CopyButton text={code} />
      </div>
      <div dangerouslySetInnerHTML={{ __html: html }} />
    </div>
  );
}
