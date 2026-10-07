"use client";

import { useHydrated, useProgress } from "@/lib/progress";
import { activityByDay, addDays, localDateKey } from "@/lib/stats";

const LEVEL_COLORS = ["var(--heat-0)", "var(--heat-1)", "var(--heat-2)", "var(--heat-3)", "var(--heat-4)"];

export function ActivityHeatmap({ weeks = 18 }: { weeks?: number }) {
  const hydrated = useHydrated();
  const progress = useProgress();
  const activity = activityByDay(progress);

  const today = new Date();
  // Columns are weeks (Sunday first), the last column contains today.
  const end = addDays(today, 6 - today.getDay());
  const start = addDays(end, -(weeks * 7 - 1));

  const columns: { key: string; count: number; future: boolean; label: string }[][] = [];
  for (let w = 0; w < weeks; w++) {
    const column = [];
    for (let d = 0; d < 7; d++) {
      const date = addDays(start, w * 7 + d);
      const key = localDateKey(date);
      const count = hydrated ? (activity.get(key) ?? 0) : 0;
      column.push({
        key,
        count,
        future: date > today,
        label: `${date.toLocaleDateString(undefined, { weekday: "short", month: "short", day: "numeric" })}: ${count} rep${count === 1 ? "" : "s"}`,
      });
    }
    columns.push(column);
  }

  return (
    <div className="w-max max-w-full">
      {/* Right-to-left flow (with columns reversed) keeps the newest weeks visible when the grid has to scroll. */}
      <div
        className="flex gap-[3px] overflow-x-auto pb-1 [direction:rtl]"
        role="img"
        aria-label={`Practice activity over the last ${weeks} weeks`}
      >
        {[...columns].reverse().map((column, w) => (
          <div key={w} className="flex flex-col gap-[3px]">
            {column.map((cell) => (
              <span
                key={cell.key}
                title={cell.future ? undefined : cell.label}
                className="block h-3 w-3 rounded-[3px]"
                style={{
                  background: cell.future ? "transparent" : LEVEL_COLORS[Math.min(cell.count, 4)],
                }}
              />
            ))}
          </div>
        ))}
      </div>
      <div className="mt-2 flex items-center justify-end gap-1 text-[11px] text-subtle">
        <span>Less</span>
        {LEVEL_COLORS.map((color) => (
          <span key={color} className="block h-2.5 w-2.5 rounded-[2px]" style={{ background: color }} />
        ))}
        <span>More</span>
      </div>
    </div>
  );
}
