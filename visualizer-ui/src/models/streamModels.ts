import { StreamMessageType } from "./streamType";

export interface DistanceMessage {
  type: StreamMessageType.Distance;
  timestamp: string;
  requestId: string;
  timer: string;
  distance: number | null;
  anchorId: string;
  receivedAt: Date;
}

export interface PositionMessage {
  type: StreamMessageType.Position;
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
