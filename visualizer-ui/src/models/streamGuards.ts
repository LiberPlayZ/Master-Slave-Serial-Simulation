import { StreamMessageType } from "./streamType";
import type { DistanceMessage, PositionMessage, StreamMessage } from "./streamModels";

export function isDistanceMessage(message: StreamMessage): message is DistanceMessage {
  return message.type === StreamMessageType.Distance;
}

export function isPositionMessage(message: StreamMessage): message is PositionMessage {
  return message.type === StreamMessageType.Position;
}
