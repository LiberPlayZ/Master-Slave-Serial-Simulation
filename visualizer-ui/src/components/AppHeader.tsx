interface AppHeaderProps {
  status: string;
}

export default function AppHeader({ status }: AppHeaderProps) {
  const label = status.charAt(0).toUpperCase() + status.slice(1);
  return (
    <header className="app-header">
      <div>
        <p className="eyebrow">Serial Telemetry</p>
        <h1>Flight Link Visualizer</h1>
      </div>
      <div className={`status-pill ${status}`}>{label}</div>
    </header>
  );
}
