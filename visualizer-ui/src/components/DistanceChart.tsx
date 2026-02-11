import { useEffect, useMemo, useRef } from "react";

const COLORS = ["#f97316", "#38bdf8", "#a3e635", "#f472b6", "#facc15", "#60a5fa"];

interface DistancePoint {
  value: number;
  requestId: string;
  receivedAt: Date;
}

interface DistanceSeries {
  anchorId: string;
  points: DistancePoint[];
}

interface DistanceChartProps {
  series: DistanceSeries[];
  anchors: string[];
  activeAnchors: string[];
  onToggleAnchor: (anchor: string) => void;
}

function getColor(anchorId: string, index: number): string {
  if (anchorId && anchorId.length > 0) {
    const hash = anchorId.split("").reduce((acc, char) => acc + char.charCodeAt(0), 0);
    return COLORS[hash % COLORS.length];
  }
  return COLORS[index % COLORS.length];
}

function isActiveAnchor(activeAnchors: string[], anchorId: string): boolean {
  return activeAnchors.length === 0 || activeAnchors.includes(anchorId);
}

export default function DistanceChart({ series, anchors, activeAnchors, onToggleAnchor }: DistanceChartProps) {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);

  const flattened = useMemo(() => {
    return series.flatMap((item) => item.points.map((point) => point.value));
  }, [series]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (series.length === 0) {
      ctx.fillStyle = "#9aa3b2";
      ctx.font = "14px 'Space Grotesk', sans-serif";
      ctx.fillText("No distance samples yet", 24, 36);
      return;
    }

    const min = Math.min(...flattened);
    const max = Math.max(...flattened);
    const padding = 28;
    const width = canvas.width - padding * 2;
    const height = canvas.height - padding * 2;

    ctx.strokeStyle = "rgba(85, 96, 130, 0.2)";
    ctx.lineWidth = 1;
    for (let i = 0; i <= 4; i += 1) {
      const y = padding + (i / 4) * height;
      ctx.beginPath();
      ctx.moveTo(padding, y);
      ctx.lineTo(padding + width, y);
      ctx.stroke();
    }

    series.forEach((item, index) => {
      if (!isActiveAnchor(activeAnchors, item.anchorId)) return;
      const points = item.points;
      if (points.length === 0) return;
      ctx.strokeStyle = getColor(item.anchorId, index);
      ctx.lineWidth = 2.2;
      ctx.beginPath();

      points.forEach((point, pointIndex) => {
        const x = padding + (pointIndex / Math.max(points.length - 1, 1)) * width;
        const norm = max === min ? 0.5 : (point.value - min) / (max - min);
        const y = padding + (1 - norm) * height;
        if (pointIndex === 0) {
          ctx.moveTo(x, y);
        } else {
          ctx.lineTo(x, y);
        }
      });

      ctx.stroke();
    });

    ctx.fillStyle = "#101625";
    ctx.font = "12px 'JetBrains Mono', monospace";
    ctx.fillText(`min ${min.toFixed(2)}`, padding, canvas.height - 10);
    ctx.fillText(`max ${max.toFixed(2)}`, padding, 18);
  }, [series, flattened, activeAnchors]);

  return (
    <div className="chart-wrapper">
      <canvas ref={canvasRef} width={900} height={320} />
      <div className="legend">
        {anchors.map((anchor, index) => {
          const active = isActiveAnchor(activeAnchors, anchor);
          return (
            <button
              type="button"
              className={`legend-item ${active ? "" : "muted"}`}
              key={anchor}
              onClick={() => onToggleAnchor(anchor)}
            >
              <span
                className="legend-dot"
                style={{ backgroundColor: getColor(anchor, index) }}
              />
              <span>Anchor {anchor}</span>
            </button>
          );
        })}
      </div>
    </div>
  );
}
