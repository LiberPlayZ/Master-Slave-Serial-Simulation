import { useEffect, useState } from "react";
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
import { getGatewayBaseUrl, getGatewayWsUrl } from "./config/gateway";
import { useReplay } from "./hooks/useReplay";
import { useTelemetryData } from "./hooks/useTelemetryData";
import { useVisualizerConfig } from "./hooks/useVisualizerConfig";
import { useWebSocket } from "./services/useWebSocket";
import { parseCsvMessages } from "./utils/csvParse";

const MAX_SERIES_SAMPLES = 200;

export default function App() {
  const gatewayBaseUrl = getGatewayBaseUrl();
  const wsUrl = getGatewayWsUrl();

  const [livePaused, setLivePaused] = useState(false);
  const { status, messages, lastMessageAt } = useWebSocket(wsUrl, { paused: livePaused });

  const [selectedAnchors, setSelectedAnchors] = useState<string[]>([]);
  const [positionRole, setPositionRole] = useState("all");
  const [positionId, setPositionId] = useState("");

  const replay = useReplay(messages);
  const config = useVisualizerConfig(gatewayBaseUrl);

  const telemetry = useTelemetryData(replay.activeMessages, {
    selectedAnchors,
    positionRole,
    positionId,
    maxSeriesSamples: MAX_SERIES_SAMPLES,
  });

  useEffect(() => {
    setSelectedAnchors((current) => current.filter((anchor) => telemetry.anchorOptions.includes(anchor)));
  }, [telemetry.anchorOptions]);

  const latestDistanceLabel =
    telemetry.latestDistance && telemetry.latestDistance.distance !== null
      ? `${telemetry.latestDistance.distance.toFixed(3)} m`
      : "n/a";

  const summary = {
    statusLabel: status.charAt(0).toUpperCase() + status.slice(1),
    lastMessageAt,
    latestDistanceLabel,
    latestDistanceHint: telemetry.latestDistance
      ? `Anchor ${telemetry.latestDistance.anchorId} · timer ${telemetry.latestDistance.timer}`
      : "Waiting for samples",
    anchorCount: telemetry.anchorOptions.length,
    distanceSeriesCount: telemetry.visibleSeriesCount,
    positionCount: telemetry.filteredPositions.length,
    lastPositionAt: telemetry.lastPositionAt,
    lastDistanceAt: telemetry.lastDistanceAt,
    noiseLabel: config.noise,
    jitterLabel: config.jitter,
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
    const csv = buildDistanceCsv(telemetry.visibleDistanceMessages);
    downloadCsv(`distance-${Date.now()}.csv`, csv);
  };

  const handleExportPositions = () => {
    const csv = buildPositionsCsv(telemetry.filteredPositions);
    downloadCsv(`positions-${Date.now()}.csv`, csv);
  };

  const handleLoadFiles = async (files: File[]) => {
    const contents = await Promise.all(files.map((file) => file.text()));
    const parsed = contents.flatMap((content) => parseCsvMessages(content));
    replay.setReplayMessages(parsed);
    replay.setReplayEnabled(true);
  };

  const handleLoadGateway = async () => {
    const [distanceCsv, positionsCsv] = await Promise.all([
      fetch(`${gatewayBaseUrl}/replay/distance`).then((response) => response.text()),
      fetch(`${gatewayBaseUrl}/replay/positions`).then((response) => response.text()),
    ]);
    const parsed = [
      ...parseCsvMessages(distanceCsv),
      ...parseCsvMessages(positionsCsv),
    ];
    replay.setReplayMessages(parsed);
    replay.setReplayEnabled(true);
  };

  const replayTotal = replay.activeSource.length;

  return (
    <div className="app">
      <AppHeader status={status} />
      <StatusSummary summary={summary} />

      <div className="toolbar">
        <StreamControls
          replayEnabled={replay.replayEnabled}
          replayPlaying={replay.replayPlaying}
          replaySpeed={replay.replaySpeed}
          replayCursor={replay.replayCursor}
          replayTotal={replayTotal}
          onToggleReplay={() => replay.setReplayEnabled((current) => !current)}
          onTogglePlay={() => replay.setReplayPlaying((current) => !current)}
          onReset={() => replay.setReplayCursor(0)}
          onSpeedChange={replay.setReplaySpeed}
          livePaused={livePaused}
          onToggleLivePause={() => setLivePaused((current) => !current)}
        />
        <ExportControls
          onExportDistances={handleExportDistances}
          onExportPositions={handleExportPositions}
          disabled={telemetry.distanceMessages.length === 0 && telemetry.positionMessages.length === 0}
        />
      </div>

      <div className="toolbar">
        <ReplayUpload onLoadFiles={handleLoadFiles} onLoadGateway={handleLoadGateway} disabled={false} />
      </div>

      <main className="layout">
        <SectionCard
          title="Distance Over Time"
          subtitle="Streaming range measurements grouped by anchor"
          actions={
            <Filters
              anchorOptions={telemetry.anchorOptions}
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
            series={telemetry.distanceSeries}
            anchors={telemetry.anchorOptions}
            activeAnchors={selectedAnchors}
            onToggleAnchor={handleToggleAnchor}
          />
          <div className="chart-meta">
            <span>
              Latest: {telemetry.latestDistance && telemetry.latestDistance.distance !== null
                ? `${telemetry.latestDistance.distance.toFixed(3)} (anchor ${telemetry.latestDistance.anchorId})`
                : "n/a"}
            </span>
            <span>Series: {telemetry.visibleSeriesCount}</span>
            <span>Samples: {telemetry.visibleDistanceMessages.length}</span>
          </div>
        </SectionCard>

        <SectionCard title="Positions" subtitle="Most recent location per role and id">
          <PositionsTable rows={telemetry.filteredPositions} />
        </SectionCard>
      </main>
    </div>
  );
}
