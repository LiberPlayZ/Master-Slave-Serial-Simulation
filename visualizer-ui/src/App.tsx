import { useEffect, useMemo, useState } from "react";
import "./App.css";
import AppHeader from "./components/AppHeader";
import DistanceChart from "./components/DistanceChart";
import Filters from "./components/Filters";
import PositionsTable from "./components/PositionsTable";
import SectionCard from "./components/SectionCard";
import StatusSummary from "./components/StatusSummary";
import ExportControls from "./components/ExportControls";
import StreamControls from "./components/StreamControls";
import ReplayUpload from "./components/ReplayUpload";
import { buildDistanceCsv, buildPositionsCsv, downloadCsv } from "./formatters/csvExport";
import { isDistanceMessage, isPositionMessage } from "./models/stream";
import type { StreamMessage } from "./models/stream";
import { useWebSocket } from "./services/useWebSocket";
import { parseCsvMessages } from "./utils/csvParse";

const MAX_SERIES_SAMPLES = 200;

export default function App() {
  const wsUrl = `${window.location.protocol === "https:" ? "wss" : "ws"}://localhost:5080/ws`;

  const [livePaused, setLivePaused] = useState(false);
  const { status, messages, lastMessageAt } = useWebSocket(wsUrl, { paused: livePaused });

  const [selectedAnchors, setSelectedAnchors] = useState<string[]>([]);
  const [positionRole, setPositionRole] = useState("all");
  const [positionId, setPositionId] = useState("");

  const [replayEnabled, setReplayEnabled] = useState(false);
  const [replayPlaying, setReplayPlaying] = useState(false);
  const [replaySpeed, setReplaySpeed] = useState(1);
  const [replayCursor, setReplayCursor] = useState(0);
  const [replayMessages, setReplayMessages] = useState<StreamMessage[]>([]);

  const activeSource = replayMessages.length > 0 ? replayMessages : messages;

  useEffect(() => {
    if (!replayEnabled) {
      setReplayPlaying(false);
      setReplayCursor(activeSource.length);
      return;
    }
    setReplayCursor(0);
    setReplayPlaying(true);
  }, [replayEnabled, activeSource.length]);

  useEffect(() => {
    if (!replayEnabled || !replayPlaying) return;

    const step = Math.max(1, Math.ceil(replaySpeed * 2));
    const interval = setInterval(() => {
      setReplayCursor((current) => Math.min(current + step, activeSource.length));
    }, 500);

    return () => clearInterval(interval);
  }, [replayEnabled, replayPlaying, replaySpeed, activeSource.length]);

  useEffect(() => {
    if (replayEnabled && replayCursor >= activeSource.length) {
      setReplayPlaying(false);
    }
  }, [replayEnabled, replayCursor, activeSource.length]);

  const activeMessages = useMemo(() => {
    if (!replayEnabled) return activeSource;
    return activeSource.slice(0, replayCursor);
  }, [activeSource, replayEnabled, replayCursor]);

  const distanceMessages = useMemo(
    () => activeMessages.filter(isDistanceMessage).filter((msg) => msg.distance !== null),
    [activeMessages]
  );

  const positionMessages = useMemo(
    () => activeMessages.filter(isPositionMessage),
    [activeMessages]
  );

  const anchorOptions = useMemo(() => {
    const anchors = new Set<string>();
    distanceMessages.forEach((msg) => anchors.add(msg.anchorId));
    return Array.from(anchors).sort();
  }, [distanceMessages]);

  useEffect(() => {
    setSelectedAnchors((current) => current.filter((anchor) => anchorOptions.includes(anchor)));
  }, [anchorOptions]);

  const distanceSeries = useMemo(() => {
    const seriesMap = new Map<string, Array<{ value: number; requestId: string; receivedAt: Date }>>();

    distanceMessages.forEach((msg) => {
      const entry = seriesMap.get(msg.anchorId) ?? [];
      entry.push({ value: msg.distance ?? 0, requestId: msg.requestId, receivedAt: msg.receivedAt });
      seriesMap.set(msg.anchorId, entry);
    });

    return Array.from(seriesMap.entries()).map(([anchorId, points]) => ({
      anchorId,
      points: points.slice(-MAX_SERIES_SAMPLES),
    }));
  }, [distanceMessages]);

  const latestDistance = distanceMessages.at(-1);

  const positions = useMemo(() => {
    const map = new Map<string, PositionMessage>();
    positionMessages.forEach((msg) => {
      const key = `${msg.role}-${msg.id ?? ""}`;
      map.set(key, msg);
    });
    return Array.from(map.values()).sort((a, b) =>
      a.role.localeCompare(b.role) || (a.id ?? "").localeCompare(b.id ?? "")
    );
  }, [positionMessages]);

  const filteredPositions = useMemo(() => {
    return positions.filter((row) => {
      if (positionRole !== "all" && row.role !== positionRole) return false;
      if (positionId && !String(row.id ?? "").includes(positionId)) return false;
      return true;
    });
  }, [positions, positionRole, positionId]);

  const visibleDistanceMessages = useMemo(() => {
    if (selectedAnchors.length === 0) return distanceMessages;
    return distanceMessages.filter((msg) => selectedAnchors.includes(msg.anchorId));
  }, [distanceMessages, selectedAnchors]);

  const visibleSeriesCount = useMemo(() => {
    if (selectedAnchors.length === 0) return distanceSeries.length;
    return distanceSeries.filter((item) => selectedAnchors.includes(item.anchorId)).length;
  }, [distanceSeries, selectedAnchors]);

  const lastDistanceAt = latestDistance?.receivedAt ?? null;
  const lastPositionAt = positionMessages.at(-1)?.receivedAt ?? null;

  const latestDistanceLabel =
    latestDistance && latestDistance.distance !== null
      ? `${latestDistance.distance.toFixed(3)} m`
      : "n/a";

  const summary = {
    statusLabel: status.charAt(0).toUpperCase() + status.slice(1),
    lastMessageAt,
    latestDistanceLabel,
    latestDistanceHint: latestDistance
      ? `Anchor ${latestDistance.anchorId} · timer ${latestDistance.timer}`
      : "Waiting for samples",
    anchorCount: anchorOptions.length,
    distanceSeriesCount: visibleSeriesCount,
    positionCount: filteredPositions.length,
    lastPositionAt,
    lastDistanceAt,
  };

  const handleToggleAnchor = (anchor: string) => {
    setSelectedAnchors((current) => {
      if (current.includes(anchor)) {
        return current.filter((item) => item !== anchor);
      }
      return [...current, anchor];
    });
  };

  const handleExportDistances = () => {
    const csv = buildDistanceCsv(visibleDistanceMessages);
    downloadCsv(`distance-${Date.now()}.csv`, csv);
  };

  const handleExportPositions = () => {
    const csv = buildPositionsCsv(filteredPositions);
    downloadCsv(`positions-${Date.now()}.csv`, csv);
  };

  const handleLoadFiles = async (files: File[]) => {
    const contents = await Promise.all(files.map((file) => file.text()));
    const parsed = contents.flatMap((content) => parseCsvMessages(content));
    setReplayMessages(parsed);
    setReplayEnabled(true);
  };

  return (
    <div className="app">
      <AppHeader status={status} />
      <StatusSummary summary={summary} />

      <div className="toolbar">
        <StreamControls
          replayEnabled={replayEnabled}
          replayPlaying={replayPlaying}
          replaySpeed={replaySpeed}
          replayCursor={replayCursor}
          replayTotal={activeSource.length}
          onToggleReplay={() => setReplayEnabled((current) => !current)}
          onTogglePlay={() => setReplayPlaying((current) => !current)}
          onReset={() => setReplayCursor(0)}
          onSpeedChange={setReplaySpeed}
          livePaused={livePaused}
          onToggleLivePause={() => setLivePaused((current) => !current)}
        />
        <ExportControls
          onExportDistances={handleExportDistances}
          onExportPositions={handleExportPositions}
          disabled={distanceMessages.length === 0 && positionMessages.length === 0}
        />
      </div>

      <div className="toolbar">
        <ReplayUpload onLoadFiles={handleLoadFiles} disabled={false} />
      </div>

      <main className="layout">
        <SectionCard
          title="Distance Over Time"
          subtitle="Streaming range measurements grouped by anchor"
          actions={
            <Filters
              anchorOptions={anchorOptions}
              selectedAnchors={selectedAnchors}
              onToggleAnchor={handleToggleAnchor}
              onClearAnchors={() => setSelectedAnchors([])}
              positionRole={positionRole}
              onPositionRoleChange={setPositionRole}
              positionId={positionId}
              onPositionIdChange={setPositionId}
            />
          }
        >
          <DistanceChart
            series={distanceSeries}
            anchors={anchorOptions}
            activeAnchors={selectedAnchors}
            onToggleAnchor={handleToggleAnchor}
          />
          <div className="chart-meta">
            <span>
              Latest: {latestDistance && latestDistance.distance !== null
                ? `${latestDistance.distance.toFixed(3)} (anchor ${latestDistance.anchorId})`
                : "n/a"}
            </span>
            <span>Series: {visibleSeriesCount}</span>
            <span>Samples: {visibleDistanceMessages.length}</span>
          </div>
        </SectionCard>

        <SectionCard title="Positions" subtitle="Most recent location per role and id">
          <PositionsTable rows={filteredPositions} />
        </SectionCard>
      </main>
    </div>
  );
}
