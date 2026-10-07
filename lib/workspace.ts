import "server-only";
import { strToU8, zipSync, type Zippable } from "fflate";
import type { Kata } from "./katas";
import { getLevel } from "./levels";

export const MSTEST_SDK_VERSION = "4.4.1";
export const WORKSPACE_ZIP = "dotnet-katas-workspace.zip";

export const levelZipName = (level: number) => `dotnet-katas-level-${level}.zip`;
export const kataZipName = (slug: string) => `kata-${slug}.zip`;

/** Test filter for one kata. The trailing dot stops Katas.L3.Linq from also matching Katas.L3.LinqJoins. */
export const kataFilter = (kata: Pick<Kata, "namespace">) => `FullyQualifiedName~${kata.namespace}.`;
export const levelFilter = (level: number) => `FullyQualifiedName~Katas.L${level}.`;

export const kataFolder = (kata: Pick<Kata, "level" | "order" | "slug">) =>
  `Level${kata.level}/${String(kata.order).padStart(2, "0")}-${kata.slug}`;

const csproj = `<Project Sdk="MSTest.Sdk/${MSTEST_SDK_VERSION}">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <!-- Starters leave parameters unread (CS9113) and events unraised (CS0067) until you implement them. -->
    <NoWarn>$(NoWarn);CS9113;CS0067</NoWarn>
  </PropertyGroup>
</Project>
`;

const globalJson = `{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
`;

const gitignore = `bin/
obj/
TestResults/
.vs/
.idea/
*.user
`;

function kataScript(katas: Kata[]): string {
  const defaultKata = katas[0]?.slug ?? "pattern-matching";
  const width = Math.max(...katas.map((k) => k.slug.length)) + 2;
  const table = katas
    .map((k) => `    ${`"${k.slug}"`.padEnd(width)} = "${k.namespace}"`)
    .join("\n");

  return `<#
.SYNOPSIS
    Runs the MSTest suite for one kata or one level of this workspace.

.EXAMPLE
    ./kata.ps1 -Kata ${defaultKata}

.EXAMPLE
    ./kata.ps1 -Kata ${defaultKata} -Watch

.EXAMPLE
    ./kata.ps1 -Level 1
#>
param(
    [string]$Kata = "${defaultKata}",
    [int]$Level = 0,
    [switch]$Watch
)

$namespaces = [ordered]@{
${table}
}

if ($Level -gt 0) {
    $filter = "FullyQualifiedName~Katas.L$Level."
}
elseif ($namespaces.Contains($Kata)) {
    # The trailing dot keeps similarly named katas (Linq vs LinqJoins) apart.
    $filter = "FullyQualifiedName~$($namespaces[$Kata])."
}
else {
    Write-Error "Unknown kata '$Kata'. Known katas: $($namespaces.Keys -join ', ')"
    exit 1
}

Push-Location $PSScriptRoot
try {
    if ($Watch) {
        dotnet watch test --filter $filter
    }
    else {
        dotnet test --filter $filter
    }
}
finally {
    Pop-Location
}
`;
}

function kataReadme(kata: Kata): string {
  const level = getLevel(kata.level);
  const hints = kata.hintsMd.map((h) => `- ${h}`).join("\n");
  const docs = kata.docs.map((d) => `- [${d.title}](${d.url})`).join("\n");

  return `# ${kata.title}

Level ${kata.level} (${level?.title ?? ""}) · about ${kata.minutes} minutes · ${kata.testCount} tests

> ${kata.summary}

Edit \`Kata.cs\` until every test in \`Tests.cs\` passes:

\`\`\`powershell
dotnet test --filter "${kataFilter(kata)}"
\`\`\`

${kata.descriptionMd}

<details>
<summary>Hints</summary>

${hints}

</details>

## Further reading

${docs}
`;
}

function workspaceReadme(katas: Kata[], scope: string): string {
  const rows = katas
    .map((k) => `| ${k.level} | [${k.title}](${kataFolder(k)}/README.md) | ${k.minutes} min | \`${kataFilter(k)}\` |`)
    .join("\n");

  return `# .NET Katas: ${scope}

A test-driven practice workspace for C# 14 / .NET 10. Every kata has a \`Kata.cs\` starter
(members throw \`NotImplementedException\`) and an MSTest \`Tests.cs\`. All tests fail until you implement them.

## Prerequisites

- .NET 10 SDK (\`winget install Microsoft.DotNet.SDK.10\`, or https://dotnet.microsoft.com/download)
- Any editor: Visual Studio 2026, VS Code with C# Dev Kit, or JetBrains Rider

## Run

\`\`\`powershell
dotnet test                                                  # everything (expect red at first)
dotnet test --filter "${katas[0] ? kataFilter(katas[0]) : "FullyQualifiedName~Katas.L1."}"   # one kata
dotnet watch test --filter "${katas[0] ? kataFilter(katas[0]) : "FullyQualifiedName~Katas.L1."}"   # re-run on save
./kata.ps1 -Kata ${katas[0]?.slug ?? "pattern-matching"} -Watch                     # same, via the helper script
\`\`\`

\`global.json\` opts \`dotnet test\` into Microsoft.Testing.Platform, which MSTest 4 requires on the .NET 10 SDK.

## Katas

| Level | Kata | Time | Filter |
| --- | --- | --- | --- |
${rows}

Reference solutions, notes, and progress tracking live on the website.
`;
}

export function buildZip(katas: Kata[], rootName: string, scope: string): Uint8Array {
  const files: Zippable = {
    [`${rootName}/README.md`]: strToU8(workspaceReadme(katas, scope)),
    [`${rootName}/DotnetKatas.csproj`]: strToU8(csproj),
    [`${rootName}/global.json`]: strToU8(globalJson),
    [`${rootName}/.gitignore`]: strToU8(gitignore),
    [`${rootName}/kata.ps1`]: strToU8(kataScript(katas)),
  };

  for (const kata of katas) {
    const folder = `${rootName}/${kataFolder(kata)}`;
    files[`${folder}/Kata.cs`] = strToU8(kata.starter);
    files[`${folder}/Tests.cs`] = strToU8(kata.tests);
    files[`${folder}/README.md`] = strToU8(kataReadme(kata));
  }

  return zipSync(files, { level: 9, mtime: new Date("2026-01-01T00:00:00Z") });
}
