"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";

const LINKS = [
  { href: "/levels", label: "Levels", match: ["/levels", "/katas"] },
  { href: "/topics", label: "Topics", match: ["/topics"] },
  { href: "/progress", label: "Progress", match: ["/progress"] },
  { href: "/setup", label: "Setup", match: ["/setup"] },
];

export function NavLinks() {
  const pathname = usePathname();

  return (
    <nav aria-label="Main" className="flex flex-1 items-center gap-0.5 overflow-x-auto text-sm sm:gap-1">
      {LINKS.map((link) => {
        const active = link.match.some((prefix) => pathname.startsWith(prefix));
        return (
          <Link
            key={link.href}
            href={link.href}
            aria-current={active ? "page" : undefined}
            className={`rounded-md px-2.5 py-1.5 transition-colors ${
              active ? "bg-surface-2 text-fg font-medium" : "text-muted hover:text-fg hover:bg-surface-2"
            }`}
          >
            {link.label}
          </Link>
        );
      })}
    </nav>
  );
}
