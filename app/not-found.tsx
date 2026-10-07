import Link from "next/link";

export default function NotFound() {
  return (
    <div className="mx-auto max-w-2xl px-4 py-24 text-center sm:px-6">
      <p className="font-mono text-sm text-accent-text">KeyNotFoundException</p>
      <h1 className="mt-3 text-3xl font-semibold tracking-tight">That page does not exist.</h1>
      <p className="mt-3 text-muted">The kata or level you are looking for may have been renamed.</p>
      <Link
        href="/levels"
        className="mt-6 inline-flex rounded-lg bg-accent px-4 py-2 text-sm font-semibold text-accent-fg hover:bg-accent-hover"
      >
        Browse levels
      </Link>
    </div>
  );
}
