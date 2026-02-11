export interface DistanceMessage {
  type: "distance";
  timestamp: string;
  requestId: string;
  timer: string;
  distance: number | null;
  anchorId: string;
  receivedAt: Date;
}

export interface PositionMessage {
  type: "position";
  timestamp: string;
  requestId: string;
  timer: string;
  role: string;
  id: string | null;
  x: number | null;
  y: number | null;
  z: number | null;
  receivedAt: Date;
}

export type StreamMessage = DistanceMessage | PositionMessage;

function normalizeDistance(raw: Record<string, unknown>): DistanceMessage {
  const distanceValue = Number(raw.distance);
  return {
    type: "distance",
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
    type: "position",
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

  if (record.type === "distance") {
    return normalizeDistance(record);
  }

  if (record.type === "position") {
    return normalizePosition(record);
  }

  return null;
}

export function isDistanceMessage(message: StreamMessage): message is DistanceMessage {
  return message.type === "distance";
}

export function isPositionMessage(message: StreamMessage): message is PositionMessage {
  return message.type === "position";
}
