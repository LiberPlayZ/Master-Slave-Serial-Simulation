interface StreamControlsProps {
  replayEnabled: boolean;
  replayPlaying: boolean;
  replaySpeed: number;
  replayCursor: number;
  replayTotal: number;
  onToggleReplay: () => void;
  onTogglePlay: () => void;
  onReset: () => void;
  onSpeedChange: (value: number) => void;
  livePaused: boolean;
  onToggleLivePause: () => void;
}

export default function StreamControls({
  replayEnabled,
  replayPlaying,
  replaySpeed,
  replayCursor,
  replayTotal,
  onToggleReplay,
  onTogglePlay,
  onReset,
  onSpeedChange,
  livePaused,
  onToggleLivePause,
}: StreamControlsProps) {
  return (
    <div className="toolbar-group">
      <div className="toolbar-title">Stream</div>
      <div className="toolbar-actions">
        <button className={`pill ${!replayEnabled ? "active" : ""}`} onClick={onToggleReplay}>
          {replayEnabled ? "Replay" : "Live"}
        </button>
        {!replayEnabled ? (
          <button className={`pill ${livePaused ? "active" : ""}`} onClick={onToggleLivePause}>
            {livePaused ? "Resume" : "Pause"}
          </button>
        ) : null}
        {replayEnabled ? (
          <>
            <button className="pill" onClick={onTogglePlay}>
              {replayPlaying ? "Pause" : "Play"}
            </button>
            <button className="pill" onClick={onReset}>Reset</button>
            <label className="select-pill">
              <span>Speed</span>
              <select value={replaySpeed} onChange={(event) => onSpeedChange(Number(event.target.value))}>
                <option value={0.5}>0.5x</option>
                <option value={1}>1x</option>
                <option value={1.5}>1.5x</option>
                <option value={2}>2x</option>
              </select>
            </label>
            <span className="toolbar-meta">
              {replayCursor}/{replayTotal}
            </span>
          </>
        ) : null}
      </div>
    </div>
  );
}
