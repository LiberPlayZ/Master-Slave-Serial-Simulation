interface ExportControlsProps {
  onExportDistances: () => void;
  onExportPositions: () => void;
  disabled: boolean;
}

export default function ExportControls({ onExportDistances, onExportPositions, disabled }: ExportControlsProps) {
  return (
    <div className="toolbar-group">
      <div className="toolbar-title">Export</div>
      <div className="toolbar-actions">
        <button className="pill" onClick={onExportDistances} disabled={disabled}>
          Distance CSV
        </button>
        <button className="pill" onClick={onExportPositions} disabled={disabled}>
          Positions CSV
        </button>
      </div>
    </div>
  );
}
