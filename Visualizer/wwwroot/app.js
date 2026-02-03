const statusEl = document.getElementById("status");
const canvas = document.getElementById("distanceChart");
const ctx = canvas.getContext("2d");
const distanceLatest = document.getElementById("distanceLatest");
const distanceCount = document.getElementById("distanceCount");
const positionsBody = document.getElementById("positionsBody");

const maxSamples = 200;
const distanceSamples = [];
const positions = new Map();

function updateStatus(text, ok = true) {
  statusEl.textContent = text;
  statusEl.style.background = ok ? "#d9f6e5" : "#f6d9d9";
}

function addDistanceSample(sample) {
  const value = parseFloat(sample.distance);
  if (Number.isNaN(value)) return;

  distanceSamples.push({
    value,
    timestamp: sample.timestamp,
  });

  if (distanceSamples.length > maxSamples) {
    distanceSamples.shift();
  }

  distanceLatest.textContent = `Latest: ${value.toFixed(3)} (anchor ${sample.anchorId})`;
  distanceCount.textContent = `Samples: ${distanceSamples.length}`;
  drawChart();
}

function updatePosition(sample) {
  const key = `${sample.role}-${sample.id || ""}`;
  positions.set(key, sample);
  renderPositions();
}

function renderPositions() {
  const rows = Array.from(positions.values())
    .sort((a, b) => a.role.localeCompare(b.role) || (a.id || "").localeCompare(b.id || ""));

  positionsBody.innerHTML = rows
    .map((row) => {
      return `
        <tr>
          <td>${row.role}</td>
          <td>${row.id || ""}</td>
          <td>${row.x}</td>
          <td>${row.y}</td>
          <td>${row.z}</td>
          <td>${row.timer}</td>
          <td>${row.requestId}</td>
        </tr>
      `;
    })
    .join("");
}

function drawChart() {
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  if (distanceSamples.length === 0) {
    ctx.fillStyle = "#6b7280";
    ctx.font = "14px Segoe UI";
    ctx.fillText("No distance samples yet", 20, 30);
    return;
  }

  const values = distanceSamples.map((p) => p.value);
  const min = Math.min(...values);
  const max = Math.max(...values);
  const padding = 20;
  const width = canvas.width - padding * 2;
  const height = canvas.height - padding * 2;

  ctx.strokeStyle = "#2c6bed";
  ctx.lineWidth = 2;
  ctx.beginPath();

  distanceSamples.forEach((point, index) => {
    const x = padding + (index / Math.max(distanceSamples.length - 1, 1)) * width;
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
}

function connect() {
  const wsUrl = `${location.origin.replace("http", "ws")}/ws`;
  const socket = new WebSocket(wsUrl);

  socket.addEventListener("open", () => updateStatus("Connected", true));
  socket.addEventListener("close", () => updateStatus("Disconnected", false));
  socket.addEventListener("error", () => updateStatus("Error", false));

  socket.addEventListener("message", (event) => {
    try {
      const data = JSON.parse(event.data);
      if (data.type === "distance") {
        addDistanceSample(data);
      } else if (data.type === "position") {
        updatePosition(data);
      }
    } catch (err) {
      console.error("Bad message", err);
    }
  });
}

connect();
