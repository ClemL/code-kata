import type { ProgressData } from "./progress";

export type KataRef = { slug: string; level: number; order: number; title: string; minutes: number };

/** YYYY-MM-DD in the viewer's local time zone. */
export function localDateKey(date: Date): string {
  const y = date.getFullYear();
  const m = String(date.getMonth() + 1).padStart(2, "0");
  const d = String(date.getDate()).padStart(2, "0");
  return `${y}-${m}-${d}`;
}

export function addDays(date: Date, days: number): Date {
  const copy = new Date(date);
  copy.setDate(copy.getDate() + days);
  return copy;
}

export function activityByDay(progress: ProgressData): Map<string, number> {
  const days = new Map<string, number>();
  for (const completions of Object.values(progress.completions)) {
    for (const completion of completions) {
      const key = localDateKey(new Date(completion.at));
      days.set(key, (days.get(key) ?? 0) + 1);
    }
  }
  return days;
}

/** Consecutive days with practice, ending today, or yesterday if today has no practice yet. */
export function currentStreak(progress: ProgressData, today = new Date()): number {
  const days = activityByDay(progress);
  let cursor = days.has(localDateKey(today)) ? today : addDays(today, -1);
  let streak = 0;
  while (days.has(localDateKey(cursor))) {
    streak++;
    cursor = addDays(cursor, -1);
  }
  return streak;
}

export function longestStreak(progress: ProgressData): number {
  const keys = [...activityByDay(progress).keys()].sort();
  let best = 0;
  let run = 0;
  let previous: Date | undefined;

  for (const key of keys) {
    const [y, m, d] = key.split("-").map(Number);
    const date = new Date(y, m - 1, d);
    run = previous && localDateKey(addDays(previous, 1)) === key ? run + 1 : 1;
    best = Math.max(best, run);
    previous = date;
  }

  return best;
}

export function isCompleted(progress: ProgressData, slug: string): boolean {
  return (progress.completions[slug]?.length ?? 0) > 0;
}

export function reps(progress: ProgressData, slug: string): number {
  return progress.completions[slug]?.length ?? 0;
}

export function lastCompletion(progress: ProgressData, slug: string): Date | undefined {
  const list = progress.completions[slug];
  return list && list.length > 0 ? new Date(list[list.length - 1].at) : undefined;
}

export function bestSeconds(progress: ProgressData, slug: string): number | undefined {
  const times = (progress.completions[slug] ?? []).map((c) => c.seconds).filter((s): s is number => !!s);
  return times.length > 0 ? Math.min(...times) : undefined;
}

export function totals(progress: ProgressData) {
  let repsTotal = 0;
  let seconds = 0;
  for (const completions of Object.values(progress.completions)) {
    repsTotal += completions.length;
    seconds += completions.reduce((sum, c) => sum + (c.seconds ?? 0), 0);
  }
  return { reps: repsTotal, minutes: Math.round(seconds / 60), katas: Object.keys(progress.completions).length };
}

/** First kata in curriculum order that has never been completed. */
export function nextUp<T extends KataRef>(katas: T[], progress: ProgressData): T | undefined {
  return katas.find((kata) => !isCompleted(progress, kata.slug));
}

/** The completed kata practiced least recently (and not today): the best candidate for another rep. */
export function reviewPick<T extends KataRef>(katas: T[], progress: ProgressData, today = new Date()): T | undefined {
  const todayKey = localDateKey(today);
  return katas
    .filter((kata) => isCompleted(progress, kata.slug))
    .map((kata) => ({ kata, last: lastCompletion(progress, kata.slug)! }))
    .filter(({ last }) => localDateKey(last) !== todayKey)
    .sort((a, b) => a.last.getTime() - b.last.getTime())[0]?.kata;
}

export function formatDuration(totalSeconds: number): string {
  const s = Math.max(0, Math.floor(totalSeconds));
  const hours = Math.floor(s / 3600);
  const minutes = Math.floor((s % 3600) / 60);
  const seconds = s % 60;
  const mm = String(minutes).padStart(hours > 0 ? 2 : 1, "0");
  const ss = String(seconds).padStart(2, "0");
  return hours > 0 ? `${hours}:${mm}:${ss}` : `${mm}:${ss}`;
}

export function formatRelativeDay(date: Date, today = new Date()): string {
  const key = localDateKey(date);
  if (key === localDateKey(today)) return "today";
  if (key === localDateKey(addDays(today, -1))) return "yesterday";
  const days = Math.round((startOfDay(today).getTime() - startOfDay(date).getTime()) / 86_400_000);
  if (days < 7) return `${days} days ago`;
  return date.toLocaleDateString(undefined, { month: "short", day: "numeric", year: days > 300 ? "numeric" : undefined });
}

function startOfDay(date: Date): Date {
  return new Date(date.getFullYear(), date.getMonth(), date.getDate());
}
