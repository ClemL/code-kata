import type { Metadata, Viewport } from "next";
import Link from "next/link";
import { NavLinks } from "@/components/NavLinks";
import { ThemeToggle } from "@/components/ThemeToggle";
import "./globals.css";

export const metadata: Metadata = {
  title: {
    default: ".NET Katas · Daily C# practice",
    template: "%s · .NET Katas",
  },
  description:
    "Leveled C# 14 / .NET 10 code katas with MSTest suites, progressive hints, reference solutions, and streak tracking.",
};

export const viewport: Viewport = {
  themeColor: [
    { media: "(prefers-color-scheme: light)", color: "#f7f7f9" },
    { media: "(prefers-color-scheme: dark)", color: "#0c0c10" },
  ],
};

// Applies the saved (or system) theme before first paint to avoid a flash.
const themeScript = `(function(){try{var t=localStorage.getItem('dotnet-katas:theme');if(t!=='light'&&t!=='dark'){t=window.matchMedia('(prefers-color-scheme: dark)').matches?'dark':'light'}document.documentElement.dataset.theme=t}catch(e){}})();`;

export default function RootLayout({ children }: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <head>
        <script dangerouslySetInnerHTML={{ __html: themeScript }} />
      </head>
      <body className="min-h-dvh flex flex-col">
        <header className="sticky top-0 z-30 border-b border-line bg-bg/85 backdrop-blur supports-[backdrop-filter]:bg-bg/70">
          <div className="mx-auto flex h-14 max-w-6xl items-center gap-3 px-4 sm:px-6">
            <Link href="/" className="flex items-center gap-2 font-semibold tracking-tight shrink-0">
              <span className="grid h-7 w-7 place-items-center rounded-md bg-accent text-[11px] font-bold text-accent-fg">
                C#
              </span>
              <span className="hidden sm:inline">.NET Katas</span>
            </Link>
            <NavLinks />
            <ThemeToggle />
          </div>
        </header>
        <main className="flex-1">{children}</main>
        <footer className="border-t border-line">
          <div className="mx-auto flex max-w-6xl flex-col gap-1 px-4 py-6 text-xs text-subtle sm:flex-row sm:justify-between sm:px-6">
            <p>C# 14 · .NET 10 · MSTest 4. Progress is stored in this browser only.</p>
            <p>
              <Link href="/setup" className="hover:text-fg">
                Workspace setup
              </Link>
              {" · "}
              <Link href="/progress" className="hover:text-fg">
                Export progress
              </Link>
            </p>
          </div>
        </footer>
      </body>
    </html>
  );
}
