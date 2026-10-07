"use client";

import { useId, useState } from "react";
import { CopyButton } from "./CopyButton";

export type CodeFile = { name: string; html: string; code: string; caption?: string };

export function CodeTabs({ files }: { files: CodeFile[] }) {
  const [active, setActive] = useState(0);
  const baseId = useId();
  const file = files[active];

  return (
    <div className="code-panel overflow-hidden rounded-xl border border-line bg-surface">
      <div className="flex items-center gap-1 border-b border-line bg-surface-2 px-2 py-1.5">
        <div role="tablist" aria-label="Files" className="flex min-w-0 flex-1 gap-1 overflow-x-auto">
          {files.map((f, i) => (
            <button
              key={f.name}
              id={`${baseId}-tab-${i}`}
              role="tab"
              type="button"
              aria-selected={i === active}
              aria-controls={`${baseId}-panel`}
              onClick={() => setActive(i)}
              className={`shrink-0 rounded-md px-2.5 py-1 font-mono text-xs transition-colors ${
                i === active ? "bg-surface text-fg shadow-sm ring-1 ring-line" : "text-muted hover:text-fg"
              }`}
            >
              {f.name}
            </button>
          ))}
        </div>
        <CopyButton text={file.code} label="Copy" />
      </div>
      {file.caption ? <p className="border-b border-line px-4 py-2 text-xs text-subtle">{file.caption}</p> : null}
      <div
        id={`${baseId}-panel`}
        role="tabpanel"
        aria-labelledby={`${baseId}-tab-${active}`}
        dangerouslySetInnerHTML={{ __html: file.html }}
      />
    </div>
  );
}
