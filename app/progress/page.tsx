import type { Metadata } from "next";
import { ProgressDashboard } from "@/components/ProgressDashboard";
import { PageHeader } from "@/components/ui";
import { getAllKatas, toSummary } from "@/lib/katas";
import { LEVELS } from "@/lib/levels";

export const metadata: Metadata = { title: "Progress" };

export default function ProgressPage() {
  return (
    <div className="mx-auto max-w-6xl px-4 py-10 sm:px-6">
      <PageHeader title="Progress">
        Streaks, reps, and best times for every kata. A rep is one completed attempt; repeating katas is the point.
      </PageHeader>
      <div className="mt-8">
        <ProgressDashboard katas={getAllKatas().map(toSummary)} levels={LEVELS} />
      </div>
    </div>
  );
}
