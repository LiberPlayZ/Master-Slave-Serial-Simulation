import { useMemo } from "react";
import { isDistanceMessage, isPositionMessage } from "../models/streamGuards";
import type { DistanceMessage, PositionMessage, StreamMessage } from "../models/streamModels";

interface TelemetryFilters {
  selectedAnchors: string[];
  positionRole: string;
  positionId: string;
  maxSeriesSamples: number;
}

interface TelemetryState {
  distanceMessages: DistanceMessage[];
  positionMessages: PositionMessage[];
  anchorOptions: string[];
  distanceSeries: Array<{ anchorId: string; points: Array<{ value: number; requestId: string; receivedAt: Date }> }>;
  latestDistance: DistanceMessage | undefined;
  positions: PositionMessage[];
  filteredPositions: PositionMessage[];
  visibleDistanceMessages: DistanceMessage[];
  visibleSeriesCount: number;
  lastDistanceAt: Date | null;
  lastPositionAt: Date | null;
}

export function useTelemetryData(activeMessages: StreamMessage[], filters: TelemetryFilters): TelemetryState {
  const { selectedAnchors, positionRole, positionId, maxSeriesSamples } = filters;

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

  const distanceSeries = useMemo(() => {
    const seriesMap = new Map<string, Array<{ value: number; requestId: string; receivedAt: Date }>>();

    distanceMessages.forEach((msg) => {
      const entry = seriesMap.get(msg.anchorId) ?? [];
      entry.push({ value: msg.distance ?? 0, requestId: msg.requestId, receivedAt: msg.receivedAt });
      seriesMap.set(msg.anchorId, entry);
    });

    return Array.from(seriesMap.entries()).map(([anchorId, points]) => ({
      anchorId,
      points: points.slice(-maxSeriesSamples),
    }));
  }, [distanceMessages, maxSeriesSamples]);

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

  return {
    distanceMessages,
    positionMessages,
    anchorOptions,
    distanceSeries,
    latestDistance,
    positions,
    filteredPositions,
    visibleDistanceMessages,
    visibleSeriesCount,
    lastDistanceAt,
    lastPositionAt,
  };
}
