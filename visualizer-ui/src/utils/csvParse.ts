import { StreamMessageType } from "../models/streamType";
import type { DistanceMessage, PositionMessage, StreamMessage } from "../models/streamModels";

function safeNumber(value: string | undefined): number | null {
  const num = Number(value);
  return Number.isFinite(num) ? num : null;
}

function isDistanceHeader(headers: string[]): boolean {
  return headers.includes("Distance") && headers.includes("AnchorId");
}

function isPositionHeader(headers: string[]): boolean {
  return headers.includes("Role") && headers.includes("X") && headers.includes("Y") && headers.includes("Z");
}

function parseLine(line: string): string[] {
  return line.split(",").map((part) => part.trim());
}

function parseDistance(parts: string[]): DistanceMessage | null {
  if (parts.length < 5) return null;
  return {
    type: StreamMessageType.Distance,
    timestamp: parts[0],
    requestId: parts[1],
    timer: parts[2],
    distance: safeNumber(parts[3]),
    anchorId: parts[4],
    receivedAt: new Date(),
  };
}

function parsePosition(parts: string[]): PositionMessage | null {
  if (parts.length < 8) return null;
  return {
    type: StreamMessageType.Position,
    timestamp: parts[0],
    requestId: parts[1],
    timer: parts[2],
    role: parts[3],
    id: parts[4] || null,
    x: safeNumber(parts[5]),
    y: safeNumber(parts[6]),
    z: safeNumber(parts[7]),
    receivedAt: new Date(),
  };
}

export function parseCsvMessages(text: string): StreamMessage[] {
  if (!text) return [];

  const lines = text.split(/\r?\n/).filter((line) => line.trim().length > 0);
  if (lines.length < 2) return [];

  const header = parseLine(lines[0]);
  const isDistance = isDistanceHeader(header);
  const isPosition = isPositionHeader(header);

  if (!isDistance && !isPosition) return [];

  const messages: StreamMessage[] = [];

  for (let i = 1; i < lines.length; i += 1) {
    const parts = parseLine(lines[i]);
    if (isDistance) {
      const msg = parseDistance(parts);
      if (msg) messages.push(msg);
    }

    if (isPosition) {
      const msg = parsePosition(parts);
      if (msg) messages.push(msg);
    }
  }

  return messages;
}
