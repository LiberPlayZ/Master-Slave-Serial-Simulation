import { StreamMessageType } from "../models/streamType";
import type { DistanceMessage, PositionMessage, StreamMessage } from "../models/streamModels";

function normalizeDistance(raw: Record<string, unknown>): DistanceMessage {
  const distanceValue = Number(raw.distance);
  return {
    type: StreamMessageType.Distance,
    timestamp: String(raw.timestamp ?? ""),
    requestId: String(raw.requestId ?? ""),
    timer: String(raw.timer ?? ""),
    distance: Number.isFinite(distanceValue) ? distanceValue : null,
    anchorId: String(raw.anchorId ?? ""),
    receivedAt: new Date(),
  };
}

function normalizePosition(raw: Record<string, unknown>): PositionMessage {
  const x = Number(raw.x);
  const y = Number(raw.y);
  const z = Number(raw.z);
  return {
    type: StreamMessageType.Position,
    timestamp: String(raw.timestamp ?? ""),
    requestId: String(raw.requestId ?? ""),
    timer: String(raw.timer ?? ""),
    role: String(raw.role ?? ""),
    id: raw.id === undefined ? null : String(raw.id ?? ""),
    x: Number.isFinite(x) ? x : null,
    y: Number.isFinite(y) ? y : null,
    z: Number.isFinite(z) ? z : null,
    receivedAt: new Date(),
  };
}

export function normalizeMessage(raw: unknown): StreamMessage | null {
  if (!raw || typeof raw !== "object") return null;
  const record = raw as Record<string, unknown>;

  if (record.type === StreamMessageType.Distance) {
    return normalizeDistance(record);
  }

  if (record.type === StreamMessageType.Position) {
    return normalizePosition(record);
  }

  return null;
}
