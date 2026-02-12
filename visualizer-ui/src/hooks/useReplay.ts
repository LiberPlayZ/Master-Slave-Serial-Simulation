import { useEffect, useMemo, useState } from "react";
import type { Dispatch, SetStateAction } from "react";
import type { StreamMessage } from "../models/streamModels";

interface ReplayState {
  replayEnabled: boolean;
  replayPlaying: boolean;
  replaySpeed: number;
  replayCursor: number;
  activeSource: StreamMessage[];
  activeMessages: StreamMessage[];
  replayMessages: StreamMessage[];
  setReplayEnabled: Dispatch<SetStateAction<boolean>>;
  setReplayPlaying: Dispatch<SetStateAction<boolean>>;
  setReplaySpeed: Dispatch<SetStateAction<number>>;
  setReplayCursor: Dispatch<SetStateAction<number>>;
  setReplayMessages: Dispatch<SetStateAction<StreamMessage[]>>;
}

export function useReplay(sourceMessages: StreamMessage[]): ReplayState {
  const [replayEnabled, setReplayEnabled] = useState(false);
  const [replayPlaying, setReplayPlaying] = useState(false);
  const [replaySpeed, setReplaySpeed] = useState(1);
  const [replayCursor, setReplayCursor] = useState(0);
  const [replayMessages, setReplayMessages] = useState<StreamMessage[]>([]);

  const activeSource = replayMessages.length > 0 ? replayMessages : sourceMessages;

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

  return {
    replayEnabled,
    replayPlaying,
    replaySpeed,
    replayCursor,
    activeSource,
    activeMessages,
    replayMessages,
    setReplayEnabled,
    setReplayPlaying,
    setReplaySpeed,
    setReplayCursor,
    setReplayMessages,
  };
}
