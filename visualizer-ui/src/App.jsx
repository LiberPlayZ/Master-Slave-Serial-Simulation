import { useEffect, useMemo, useState } from "react";
import "./App.css";

const MAX_SAMPLES = 200;

function useWebSocket(url) {
  const [status, setStatus] = useState("Connecting...");
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    const socket = new WebSocket(url);

    socket.addEventListener("open", () => setStatus("Connected"));
    socket.addEventListener("close", () => setStatus("Disconnected"));
    socket.addEventListener("error", () => setStatus("Error"));

    socket.addEventListener("message", (event) => {
      try {
        const data = JSON.parse(event.data);
        setMessages((prev) => [...prev.slice(-MAX_SAMPLES), data]);
      } catch (err) {
        console.error("Bad message", err);
      }
    });

    return () => socket.close();
  }, [url]);

  return { status, messages };
}

function DistanceChart({ samples }) {
  const canvasRef = useMemo(() => ({ current: null }), []);

  useEffect(() => {
    const canvas = document.getElementById("distanceCanvas");
    if (!canvas) return;
    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    if (samples.length === 0) {
      ctx.fillStyle = "#6b7280";
      ctx.font = "14px Segoe UI";
      ctx.fillText("No distance samples yet", 20, 30);
      return;
    }

    const values = samples.map((s) => s.value);
    const min = Math.min(...values);
    const max = Math.max(...values);
    const padding = 20;
    const width = canvas.width - padding * 2;
    const height = canvas.height - padding * 2;

    ctx.strokeStyle = "#2c6bed";
    ctx.lineWidth = 2;
    ctx.beginPath();

    samples.forEach((point, index) => {
      const x = padding + (index / Math.max(samples.length - 1, 1)) * width;
      const norm = max === min ? 0.5 : (point.value - min) / (max - min);
      const y = padding + (1 - norm) * height;
      if (index === 0) {
        ctx.moveTo(x, y);
      } else {
        ctx.lineTo(x, y);
      }
    });

    ctx.stroke();

    ctx.fillStyle = "#1b1b1b";
    ctx.font = "12px Segoe UI";
    ctx.fillText(`min ${min.toFixed(2)}`, padding, canvas.height - 6);
    ctx.fillText(`max ${max.toFixed(2)}`, padding, 14);
  }, [samples]);

  return <canvas id="distanceCanvas" width="900" height="320" ref={canvasRef} />;
}

export default function App() {
  const wsUrl = `${window.location.protocol === "https:" ? "wss" : "ws"}://localhost:5080/ws`;
  const { status, messages } = useWebSocket(wsUrl);

  const distanceSamples = useMemo(() => {
    return messages
      .filter((m) => m.type === "distance")
      .map((m) => ({
        value: Number(m.distance),
        anchorId: m.anchorId,
      }))
      .slice(-MAX_SAMPLES);
  }, [messages]);

  const positions = useMemo(() => {
    const map = new Map();
    messages
      .filter((m) => m.type === "position")
      .forEach((m) => {
        const key = `${m.role}-${m.id ?? ""}`;
        map.set(key, m);
      });
    return Array.from(map.values()).sort((a, b) =>
      a.role.localeCompare(b.role) || (a.id ?? "").localeCompare(b.id ?? "")
    );
  }, [messages]);

  const latest = distanceSamples.at(-1);

  return (
    <div className="app">
      <header>
        <h1>Serial Visualizer</h1>
        <span className={`status ${status.toLowerCase()}`}>{status}</span>
      </header>

      <main>
        <section className="card">
          <h2>Distance Over Time</h2>
          <DistanceChart samples={distanceSamples} />
          <div className="chart-meta">
            <span>
              Latest: {latest ? `${latest.value.toFixed(3)} (anchor ${latest.anchorId})` : "n/a"}
            </span>
            <span>Samples: {distanceSamples.length}</span>
          </div>
        </section>

        <section className="card">
          <h2>Positions</h2>
          <table>
            <thead>
              <tr>
                <th>Role</th>
                <th>Id</th>
                <th>X</th>
                <th>Y</th>
                <th>Z</th>
                <th>Timer</th>
                <th>Request</th>
              </tr>
            </thead>
            <tbody>
              {positions.map((row) => (
                <tr key={`${row.role}-${row.id ?? ""}`}>
                  <td>{row.role}</td>
                  <td>{row.id}</td>
                  <td>{row.x}</td>
                  <td>{row.y}</td>
                  <td>{row.z}</td>
                  <td>{row.timer}</td>
                  <td>{row.requestId}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>
      </main>
    </div>
  );
}
