import type { DistanceMessage, PositionMessage } from "../models/streamModels";

function escapeValue(value: string | number | null | undefined): string {
  if (value === null || value === undefined) return "";
  const text = String(value);
  if (text.includes(",") || text.includes("\n") || text.includes("\"")) {
    return `"${text.replace(/\"/g, '""')}"`;
  }
  return text;
}

function buildCsv(headers: string[], rows: Array<Array<string | number | null | undefined>>): string {
  const headerLine = headers.join(",");
  const body = rows.map((row) => row.map(escapeValue).join(",")).join("\n");
  return `${headerLine}\n${body}`;
}

export function buildDistanceCsv(messages: DistanceMessage[]): string {
  const headers = ["Timestamp", "RequestId", "Timer", "Distance", "AnchorId"];
  const rows = messages.map((msg) => [
    msg.timestamp,
    msg.requestId,
    msg.timer,
    msg.distance ?? "NA",
    msg.anchorId,
  ]);
  return buildCsv(headers, rows);
}

export function buildPositionsCsv(messages: PositionMessage[]): string {
  const headers = ["Timestamp", "RequestId", "Timer", "Role", "Id", "X", "Y", "Z"];
  const rows = messages.map((msg) => [
    msg.timestamp,
    msg.requestId,
    msg.timer,
    msg.role,
    msg.id ?? "",
    msg.x ?? "NA",
    msg.y ?? "NA",
    msg.z ?? "NA",
  ]);
  return buildCsv(headers, rows);
}

export function downloadCsv(filename: string, content: string): void {
  const blob = new Blob([content], { type: "text/csv;charset=utf-8" });
  const link = document.createElement("a");
  const url = URL.createObjectURL(blob);
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
