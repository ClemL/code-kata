import type { Metadata } from "next";
import Link from "next/link";
import { CommandBlock } from "@/components/CommandBlock";
import { DownloadIcon } from "@/components/icons";
import { PageHeader, SectionHeading } from "@/components/ui";
import { getAllKatas } from "@/lib/katas";
import { highlight } from "@/lib/render";
import { kataFilter, MSTEST_SDK_VERSION, WORKSPACE_ZIP } from "@/lib/workspace";

export const metadata: Metadata = {
  title: "Workspace setup",
  description: "Install the .NET 10 SDK, download the kata workspace, and run one kata at a time with dotnet test.",
};

export default async function SetupPage() {
  const katas = getAllKatas();
  const first = katas[0];
  const totalTests = katas.reduce((sum, k) => sum + k.testCount, 0);

  const snippets = {
    install: `# Windows (PowerShell)
winget install Microsoft.DotNet.SDK.10

# macOS
brew install --cask dotnet-sdk

# Ubuntu 24.04
sudo apt-get update && sudo apt-get install -y dotnet-sdk-10.0

dotnet --version   # expect 10.0.x`,
    unzip: `$Zip         = "$HOME\\Downloads\\${WORKSPACE_ZIP}"
$Destination = "$HOME\\source"

Expand-Archive -Path $Zip -DestinationPath $Destination -Force
Set-Location "$Destination\\dotnet-katas"

# A commit of the starting point lets you reset a kata for another rep
git init; git add .; git commit -m "Kata starting point"

# Every test fails at first: that is the starting line
dotnet test`,
    one: `# One kata. The trailing dot stops Katas.L3.Linq from also matching Katas.L3.LinqJoins.
dotnet test --filter "${kataFilter(first)}"

# Re-run on every save
dotnet watch test --filter "${kataFilter(first)}"

# The same through the helper script. Unblock it once (it was downloaded from the internet).
Unblock-File .\\kata.ps1
./kata.ps1 -Kata ${first.slug} -Watch

# A whole level
./kata.ps1 -Level 1`,
    reset: `# Put a kata back to its starting state for another rep
git checkout -- Level1/01-${first.slug}/Kata.cs

# Or reset everything
git checkout -- .`,
    csproj: `<Project Sdk="MSTest.Sdk/${MSTEST_SDK_VERSION}">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>`,
    globalJson: `{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}`,
  };

  const [install, unzip, one, reset, csproj, globalJson] = await Promise.all([
    highlight(snippets.install, "powershell"),
    highlight(snippets.unzip, "powershell"),
    highlight(snippets.one, "powershell"),
    highlight(snippets.reset, "powershell"),
    highlight(snippets.csproj, "xml"),
    highlight(snippets.globalJson, "json"),
  ]);

  return (
    <div className="mx-auto max-w-3xl px-4 py-10 sm:px-6">
      <PageHeader title="Workspace setup">
        Ten minutes of setup, then every session is: open the editor, filter to one kata, make the tests pass.
      </PageHeader>

      <div className="mt-10 space-y-12">
        <section>
          <SectionHeading>1. Install the .NET 10 SDK</SectionHeading>
          <p className="mb-4 text-sm text-muted">
            .NET 10 is the current long-term-support release and ships C# 14, which the Level 2 and Level 5 katas use
            (<code className="font-mono text-[0.85em]">field</code>, extension members). Any editor works: Visual Studio
            2026, VS Code with the C# Dev Kit extension, or JetBrains Rider.
          </p>
          <CommandBlock html={install} code={snippets.install} label="Shell" />
        </section>

        <section>
          <SectionHeading>2. Download the workspace</SectionHeading>
          <p className="mb-4 text-sm text-muted">
            The workspace is a single MSTest project containing every starter as{" "}
            <code className="font-mono text-[0.85em]">Kata.cs</code>, its <code className="font-mono text-[0.85em]">Tests.cs</code>,
            and a README per kata ({katas.length} katas, {totalTests} tests). Reference solutions stay on this site so they
            are not one click away while you work.
          </p>
          <a
            href={`/download/${WORKSPACE_ZIP}`}
            className="mb-4 inline-flex items-center gap-2 rounded-lg bg-accent px-4 py-2.5 text-sm font-semibold text-accent-fg hover:bg-accent-hover"
          >
            <DownloadIcon size={16} /> {WORKSPACE_ZIP}
          </a>
          <CommandBlock html={unzip} code={snippets.unzip} />
          <p className="mt-3 text-xs text-subtle">
            Per-level and per-kata downloads are on each <Link href="/levels" className="underline underline-offset-2">level</Link> and
            kata page if you prefer smaller projects.
          </p>
        </section>

        <section>
          <SectionHeading>3. Work on one kata at a time</SectionHeading>
          <p className="mb-4 text-sm text-muted">
            Every kata has its own namespace (<code className="font-mono text-[0.85em]">Katas.L&lt;level&gt;.&lt;Name&gt;</code>),
            so a test filter isolates it. Watch mode rebuilds and re-runs the filtered tests each time you save.
          </p>
          <CommandBlock html={one} code={snippets.one} />
        </section>

        <section>
          <SectionHeading>4. Repeat katas</SectionHeading>
          <p className="mb-4 text-sm text-muted">
            A kata is meant to be done more than once; the second and third reps are where the idioms stick. Commit the
            starting point once, then restore a kata whenever the site suggests it as a review rep.
          </p>
          <CommandBlock html={reset} code={snippets.reset} />
        </section>

        <section>
          <SectionHeading>How the project is configured</SectionHeading>
          <div className="space-y-4 text-sm text-muted">
            <p>
              The project uses the <code className="font-mono text-[0.85em]">MSTest.Sdk</code> project SDK, which brings in MSTest{" "}
              {MSTEST_SDK_VERSION}, its analyzers, and Microsoft.Testing.Platform (MTP) without separate package references.
            </p>
            <CommandBlock html={csproj} code={snippets.csproj} label="DotnetKatas.csproj" />
            <p>
              On the .NET 10 SDK, <code className="font-mono text-[0.85em]">dotnet test</code> only runs MTP-based projects after you
              opt in through <code className="font-mono text-[0.85em]">global.json</code>. Without it you get the error
              &ldquo;Testing with VSTest target is no longer supported by Microsoft.Testing.Platform on .NET 10 SDK and
              later.&rdquo;
            </p>
            <CommandBlock html={globalJson} code={snippets.globalJson} label="global.json" />
          </div>
        </section>

        <section>
          <SectionHeading>Making it a habit</SectionHeading>
          <ul className="list-disc space-y-3 pl-5 text-sm text-muted">
            <li>
              <strong className="text-fg">Fix the cue, not the motivation.</strong> A concrete &ldquo;when-then&rdquo; plan
              (&ldquo;after my first coffee, I open the kata workspace&rdquo;) improves follow-through; a meta-analysis of 94
              studies of these implementation intentions found a medium-to-large effect on goal attainment (d = 0.65).{" "}
              <a
                className="text-accent-text underline underline-offset-2"
                href="https://doi.org/10.1016/S0065-2601(06)38002-1"
                target="_blank"
                rel="noreferrer noopener"
              >
                Gollwitzer &amp; Sheeran, 2006
              </a>
            </li>
            <li>
              <strong className="text-fg">Expect about two months.</strong> In a 12-week study of 96 people, a daily behavior took
              a median of 66 days to reach 95% of its peak automaticity (range 18-254 days), and missing a single day did not
              materially affect habit formation.{" "}
              <a
                className="text-accent-text underline underline-offset-2"
                href="https://doi.org/10.1002/ejsp.674"
                target="_blank"
                rel="noreferrer noopener"
              >
                Lally et al., 2010
              </a>
            </li>
            <li>
              <strong className="text-fg">Keep the timebox.</strong> Stop when the timer runs out, read the hints and solution,
              log the rep, and come back to the same kata later. Short, frequent sessions beat occasional marathons.
            </li>
            <li>
              <strong className="text-fg">Alternate new and review.</strong> The home page suggests the next new kata and the
              completed kata you practiced least recently. A simple rhythm: new katas on weekdays, one review rep at the
              weekend.
            </li>
          </ul>
        </section>
      </div>
    </div>
  );
}
