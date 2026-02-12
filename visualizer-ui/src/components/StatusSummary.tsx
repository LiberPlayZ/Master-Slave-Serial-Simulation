import { formatAge, formatTime } from "../utils/time";

interface SummaryData {
  statusLabel: string;
  lastMessageAt: Date | null;
  latestDistanceLabel: string;
  latestDistanceHint: string;
  anchorCount: number;
  distanceSeriesCount: number;
  positionCount: number;
  lastPositionAt: Date | null;
  noiseLabel: string;
  jitterLabel: string;
}

interface StatusSummaryProps {
  summary: SummaryData;
}

function StatCard({ label, value, hint }: { label: string; value: string | number; hint?: string }) {
  return (
    <div className="stat-card">
      <p className="stat-label">{label}</p>
      <p className="stat-value">{value}</p>
      {hint ? <p className="stat-hint">{hint}</p> : null}
    </div>
  );
}

export default function StatusSummary({ summary }: StatusSummaryProps) {
  return (
    <section className="summary-grid">
      <StatCard
        label="Connection"
        value={summary.statusLabel}
        hint={`Last message ${formatAge(summary.lastMessageAt)}`}
      />
      <StatCard
        label="Latest Distance"
        value={summary.latestDistanceLabel}
        hint={summary.latestDistanceHint}
      />
      <StatCard
        label="Anchors Seen"
        value={summary.anchorCount}
        hint={`Distance streams: ${summary.distanceSeriesCount}`}
      />
      <StatCard
        label="Positions"
        value={summary.positionCount}
        hint={`Last update ${formatTime(summary.lastPositionAt)}`}
      />
      <StatCard
        label="Noise"
        value={summary.noiseLabel}
        hint="Distance noise range"
      />
      <StatCard
        label="Jitter"
        value={summary.jitterLabel}
        hint="Response delay variance"
      />
    </section>
  );
}
