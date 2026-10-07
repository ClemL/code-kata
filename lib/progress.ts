"use client";

import { useSyncExternalStore } from "react";

export type Completion = {
  /** ISO timestamp. */
  at: string;
  /** Seconds on the timer when the kata was marked complete, if the timer was used. */
  seconds?: number;
};

export type ProgressData = {
  version: 1;
  completions: Record<string, Completion[]>;
  hintsRevealed: Record<string, number>;
  solutionViewed: Record<string, boolean>;
};

const STORAGE_KEY = "dotnet-katas:progress:v1";
const CHANGE_EVENT = "dotnet-katas:progress-change";

export const EMPTY_PROGRESS: ProgressData = Object.freeze({
  version: 1,
  completions: {},
  hintsRevealed: {},
  solutionViewed: {},
}) as ProgressData;

let cachedRaw: string | null | undefined;
let cachedValue: ProgressData = EMPTY_PROGRESS;
/** Used when localStorage is unavailable (private windows, blocked storage). */
let memoryFallback: ProgressData | undefined;

function normalize(value: unknown): ProgressData {
  if (!value || typeof value !== "object") return EMPTY_PROGRESS;
  const v = value as Partial<ProgressData>;
  return {
    version: 1,
    completions: v.completions && typeof v.completions === "object" ? v.completions : {},
    hintsRevealed: v.hintsRevealed && typeof v.hintsRevealed === "object" ? v.hintsRevealed : {},
    solutionViewed: v.solutionViewed && typeof v.solutionViewed === "object" ? v.solutionViewed : {},
  };
}

function read(): ProgressData {
  if (memoryFallback) return memoryFallback;

  let raw: string | null;
  try {
    raw = window.localStorage.getItem(STORAGE_KEY);
  } catch {
    return cachedValue;
  }

  if (raw === cachedRaw) return cachedValue;

  cachedRaw = raw;
  try {
    cachedValue = raw ? normalize(JSON.parse(raw)) : EMPTY_PROGRESS;
  } catch {
    cachedValue = EMPTY_PROGRESS;
  }
  return cachedValue;
}

function write(next: ProgressData): void {
  try {
    window.localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
    memoryFallback = undefined;
  } catch {
    memoryFallback = next;
  }
  window.dispatchEvent(new Event(CHANGE_EVENT));
}

function update(mutator: (draft: ProgressData) => void): void {
  const current = read();
  const draft: ProgressData = structuredClone(current);
  mutator(draft);
  write(draft);
}

function subscribe(callback: () => void): () => void {
  window.addEventListener("storage", callback);
  window.addEventListener(CHANGE_EVENT, callback);
  return () => {
    window.removeEventListener("storage", callback);
    window.removeEventListener(CHANGE_EVENT, callback);
  };
}

/** Progress for the current browser. Renders as empty on the server and during hydration. */
export function useProgress(): ProgressData {
  return useSyncExternalStore(subscribe, read, () => EMPTY_PROGRESS);
}

/** True after hydration; lets components avoid flashing "0 completed" before storage is read. */
export function useHydrated(): boolean {
  return useSyncExternalStore(
    () => () => {},
    () => true,
    () => false,
  );
}

export function completeKata(slug: string, seconds?: number): void {
  update((draft) => {
    const entry: Completion = { at: new Date().toISOString() };
    if (seconds && seconds > 0) entry.seconds = Math.round(seconds);
    (draft.completions[slug] ??= []).push(entry);
  });
}

export function undoLastCompletion(slug: string): void {
  update((draft) => {
    draft.completions[slug]?.pop();
    if (draft.completions[slug]?.length === 0) delete draft.completions[slug];
  });
}

export function revealHints(slug: string, count: number): void {
  update((draft) => {
    draft.hintsRevealed[slug] = Math.max(draft.hintsRevealed[slug] ?? 0, count);
  });
}

export function markSolutionViewed(slug: string): void {
  update((draft) => {
    draft.solutionViewed[slug] = true;
  });
}

export function resetProgress(): void {
  write({ version: 1, completions: {}, hintsRevealed: {}, solutionViewed: {} });
}

export function exportProgress(): string {
  return JSON.stringify(read(), null, 2);
}

export function importProgress(json: string): { ok: true } | { ok: false; error: string } {
  try {
    const parsed = JSON.parse(json) as unknown;
    if (!parsed || typeof parsed !== "object" || !("completions" in parsed)) {
      return { ok: false, error: "That file does not look like a progress export." };
    }
    write(normalize(parsed));
    return { ok: true };
  } catch (error) {
    return { ok: false, error: error instanceof Error ? error.message : "Could not read the file." };
  }
}

/* ---------- Timer (per kata, survives reloads) ---------- */

type TimerState = { startedAt: number | null; accumulatedMs: number };

const timerKey = (slug: string) => `dotnet-katas:timer:${slug}`;
const IDLE_TIMER: TimerState = Object.freeze({ startedAt: null, accumulatedMs: 0 }) as TimerState;
const timerCache = new Map<string, { raw: string | null; value: TimerState }>();
const timerMemory = new Map<string, TimerState>();

function readTimer(slug: string): TimerState {
  if (timerMemory.has(slug)) return timerMemory.get(slug)!;

  let raw: string | null = null;
  try {
    raw = window.localStorage.getItem(timerKey(slug));
  } catch {
    return IDLE_TIMER;
  }

  const cached = timerCache.get(slug);
  if (cached && cached.raw === raw) return cached.value;

  let value = IDLE_TIMER;
  try {
    if (raw) value = JSON.parse(raw) as TimerState;
  } catch {
    value = IDLE_TIMER;
  }
  timerCache.set(slug, { raw, value });
  return value;
}

function writeTimer(slug: string, state: TimerState): void {
  try {
    if (state.startedAt === null && state.accumulatedMs === 0) window.localStorage.removeItem(timerKey(slug));
    else window.localStorage.setItem(timerKey(slug), JSON.stringify(state));
    timerMemory.delete(slug);
  } catch {
    timerMemory.set(slug, state);
  }
  window.dispatchEvent(new Event(CHANGE_EVENT));
}

export function useTimer(slug: string): TimerState {
  return useSyncExternalStore(
    subscribe,
    () => readTimer(slug),
    () => IDLE_TIMER,
  );
}

export function elapsedMs(state: TimerState, now: number): number {
  return state.accumulatedMs + (state.startedAt === null ? 0 : now - state.startedAt);
}

export function startTimer(slug: string): void {
  const state = readTimer(slug);
  if (state.startedAt !== null) return;
  writeTimer(slug, { startedAt: Date.now(), accumulatedMs: state.accumulatedMs });
}

export function pauseTimer(slug: string): void {
  const state = readTimer(slug);
  if (state.startedAt === null) return;
  writeTimer(slug, { startedAt: null, accumulatedMs: elapsedMs(state, Date.now()) });
}

export function resetTimer(slug: string): void {
  writeTimer(slug, { startedAt: null, accumulatedMs: 0 });
}
