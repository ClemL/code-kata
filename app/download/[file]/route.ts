import { getAllKatas, getKatasForLevel } from "@/lib/katas";
import { getLevel, LEVELS } from "@/lib/levels";
import { buildZip, kataZipName, levelZipName, WORKSPACE_ZIP } from "@/lib/workspace";

export const dynamic = "force-static";
export const dynamicParams = false;

export function generateStaticParams() {
  return [
    { file: WORKSPACE_ZIP },
    ...LEVELS.map((level) => ({ file: levelZipName(level.number) })),
    ...getAllKatas().map((kata) => ({ file: kataZipName(kata.slug) })),
  ];
}

function zipFor(file: string): Uint8Array | undefined {
  if (file === WORKSPACE_ZIP) {
    return buildZip(getAllKatas(), "dotnet-katas", "all levels");
  }

  for (const level of LEVELS) {
    if (file === levelZipName(level.number)) {
      return buildZip(getKatasForLevel(level.number), `dotnet-katas-level-${level.number}`, `Level ${level.number} · ${getLevel(level.number)?.title}`);
    }
  }

  const kata = getAllKatas().find((k) => file === kataZipName(k.slug));
  return kata ? buildZip([kata], `kata-${kata.slug}`, kata.title) : undefined;
}

export async function GET(_request: Request, { params }: { params: Promise<{ file: string }> }) {
  const { file } = await params;
  const bytes = zipFor(file);

  if (!bytes) {
    return new Response("Not found", { status: 404 });
  }

  return new Response(new Uint8Array(bytes), {
    headers: {
      "Content-Type": "application/zip",
      "Content-Disposition": `attachment; filename="${file}"`,
      "Cache-Control": "public, max-age=3600",
    },
  });
}
