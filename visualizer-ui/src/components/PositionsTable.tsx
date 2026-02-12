import type { PositionMessage } from "../models/streamModels";

interface PositionsTableProps {
  rows: PositionMessage[];
}

export default function PositionsTable({ rows }: PositionsTableProps) {
  return (
    <div className="table-wrap">
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
          {rows.length === 0 ? (
            <tr>
              <td colSpan={7} className="empty-row">
                No position updates yet
              </td>
            </tr>
          ) : (
            rows.map((row) => (
              <tr key={`${row.role}-${row.id ?? ""}`}>
                <td>{row.role}</td>
                <td>{row.id}</td>
                <td>{row.x ?? "n/a"}</td>
                <td>{row.y ?? "n/a"}</td>
                <td>{row.z ?? "n/a"}</td>
                <td>{row.timer}</td>
                <td>{row.requestId}</td>
              </tr>
            ))
          )}
        </tbody>
      </table>
    </div>
  );
}
