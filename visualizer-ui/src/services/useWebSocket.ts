import { useEffect, useMemo, useState } from "react";
import { normalizeMessage } from "./streamNormalizer";
import type { StreamMessage } from "../models/streamModels";

const MAX_SAMPLES = 1200;

interface WebSocketOptions {
  paused?: boolean;
}

interface WebSocketState {
  status: "connecting" | "connected" | "disconnected" | "error";
  messages: StreamMessage[];
  lastMessageAt: Date | null;
}

export function useWebSocket(url: string, options: WebSocketOptions = {}): WebSocketState {
  const { paused = false } = options;
  const [status, setStatus] = useState<WebSocketState["status"]>("connecting");
  const [messages, setMessages] = useState<StreamMessage[]>([]);
  const [lastMessageAt, setLastMessageAt] = useState<Date | null>(null);

  useEffect(() => {
    const socket = new WebSocket(url);

    const handleOpen = () => setStatus("connected");
    const handleClose = () => setStatus("disconnected");
    const handleError = () => setStatus("error");

    socket.addEventListener("open", handleOpen);
    socket.addEventListener("close", handleClose);
    socket.addEventListener("error", handleError);

    socket.addEventListener("message", (event) => {
      if (paused) return;
      try {
        const raw = JSON.parse(event.data);
        const normalized = normalizeMessage(raw);
        if (!normalized) return;
        setMessages((prev) => [...prev.slice(-MAX_SAMPLES), normalized]);
        setLastMessageAt(new Date());
      } catch (err) {
        console.error("Bad message", err);
      }
    });

    return () => {
      socket.removeEventListener("open", handleOpen);
      socket.removeEventListener("close", handleClose);
      socket.removeEventListener("error", handleError);
      socket.close();
    };
  }, [url, paused]);

  return useMemo(
    () => ({
      status,
      messages,
      lastMessageAt,
    }),
    [status, messages, lastMessageAt]
  );
}
