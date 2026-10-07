export function anchorFor(prefix: string, term: string): string {
  const slug = term
    .toLowerCase()
    .replace(/[<>]/g, "-")
    .replace(/[^a-z0-9#+]+/g, "-")
    .replace(/#/g, "sharp")
    .replace(/\+/g, "plus")
    .replace(/^-+|-+$/g, "");
  return `${prefix}-${slug || "symbol"}-${hash(term)}`;
}

/** Short stable hash so terms that slugify identically (e.g. "?." and "??") still get unique anchors. */
function hash(value: string): string {
  let h = 0;
  for (let i = 0; i < value.length; i++) h = (h * 31 + value.charCodeAt(i)) | 0;
  return (h >>> 0).toString(36).slice(0, 4);
}
