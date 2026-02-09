# Serial Simulation

Master/slave serial simulation with CSV logging, ACK/retry protocol, and a live Visualizer (gateway + React UI).

## Requirements
- .NET 9 SDK
- Node.js 20+ (for the React UI)
- `socat` (Linux virtual serial ports)
- Bash
- `tmux` (only if you use the `run-all.sh` launcher)

## Projects
- `SimulationMaster`: master process; interactive commands; logs responses to CSV.
- `SimulatorProject`: slave process; simulates movement and responds over serial.
- `SharedConfig`: shared configuration loader (`.env` or environment variables).
- `Visualizer`: ASP.NET minimal API + WebSocket gateway that streams CSV data as JSON.
- `visualizer-ui`: React (Vite) UI that renders distance chart + positions table.
- `virtual-ports-launch`: helper script to create a pair of virtual serial ports.

## How It Works
- Master sends requests like `GET_DISTANCE`, `GET_PILOT_POSITION`, `GET_ANCHOR_POSITION`, and `GET_STATUS`.
- Each request is wrapped with a request id: `REQ,<id>,...` (UUID).
- Slave ACKs each request (`ACK,<id>`) before sending the data response.
- Master retries if the ACK is not received in time.
- Slave advances the pilot position over time, computes responses, applies optional noise/jitter, then replies.
- Master writes distance and position responses to separate CSV files.

## Quick Start (tmux launcher)
Starts virtual ports + slave + master in one tmux session, and the gateway + React UI in a second tmux session.

```bash
bash scripts/run-all.sh
```

Reset (kills existing sessions and relaunches):

```bash
bash scripts/run-all.sh --reset
```

Tmux sessions:
- `Master_slave_project`: virtual ports + slave + master.
- `Visualizer_project`: gateway + React UI.

Switch sessions with `Ctrl+b` then `)`.

## Manual Run

### 1) Virtual serial ports (Linux)
```bash
bash virtual-ports-launch/start-virtual-serial.sh
```

### 2) Slave
```bash
cd SimulatorProject
dotnet run
```

### 3) Master
```bash
cd SimulationMaster
dotnet run
```

### 4) Visualizer gateway
```bash
cd Visualizer
dotnet run
```

Health check:

```
http://localhost:5080/health
```

### 5) React UI
```bash
cd visualizer-ui
npm run dev
```

Open:

```
http://localhost:5173
```

The React UI connects to the gateway WebSocket at `ws://localhost:5080/ws`.

## Tests
Run the automated protocol tests:

```bash
dotnet test serial-sim.sln
```

## Interactive Master Commands
- `distance <id>`
- `pilot`
- `anchor <id>`
- `status`
- `noise <min> <max>`
- `noise off`
- `jitter <ms>`
- `jitter off`
- `help`
- `exit`

## Wire Protocol (current)
Requests (master → slave):
- `REQ,<id>,GET_DISTANCE,<anchorId>`
- `REQ,<id>,GET_PILOT_POSITION`
- `REQ,<id>,GET_ANCHOR_POSITION,<anchorId>`
- `REQ,<id>,GET_STATUS`
- `REQ,<id>,SET_NOISE,<min>,<max>`
- `REQ,<id>,SET_JITTER,<ms>`

ACK (slave → master):
- `ACK,<id>`

Responses (slave → master):
- `DISTANCE,<id>,<timer>,<distance>,<anchorId>`
- `PILOT_POSITION,<id>,<timer>,<x>,<y>,<z>`
- `ANCHOR_POSITION,<id>,<timer>,<anchorId>,<x>,<y>,<z>`
- `STATUS_PILOT,<id>,<timer>,<x>,<y>,<z>`
- `STATUS_ANCHOR,<id>,<timer>,<anchorId>,<x>,<y>,<z>`
- `CONFIG_NOISE,<id>,<min>,<max>`
- `CONFIG_JITTER,<id>,<ms>`

## Configuration

### `.env`
Default `.env`:

```ini
BAUD_RATE = 9600
MAX_RANGE = 0.5
MIN_RANGE = -0.5
SLAVE_PORT_NAME = /tmp/ttyV0
MASTER_PORT_NAME = /tmp/ttyV1
RESPONSE_DELAY = 5.5
PILOT_SPEED = 10
PILOT_SPEED_TYPE = mps
LOGS_PATH = logs/output/
DISTANCE_CSV_NAME=distance.log.csv
POSITIONS_CSV_NAME = positions.log.csv
ACK_MAX_RETRIES = 3
ACK_TIMEOUT_MS = 2000
DISTANCE_NOISE_MIN = 0
DISTANCE_NOISE_MAX = 0
RESPONSE_JITTER_MS = 0
VISUALIZER_HOST = localhost
VISUALIZER_PORT = 5080
```

Notes:
- `SLAVE_PORT_NAME` and `MASTER_PORT_NAME` must match your serial device names.
- `PILOT_SPEED_TYPE` supports `mps`, `kph`, or `mph`.
- `LOGS_PATH` is relative to the process working directory.
- `ACK_MAX_RETRIES` and `ACK_TIMEOUT_MS` control ACK retry behavior.
- `DISTANCE_NOISE_MIN` / `DISTANCE_NOISE_MAX` add uniform noise to distance responses.
- `RESPONSE_JITTER_MS` adds random response delay on top of `RESPONSE_DELAY`.
- `noise off` sends `SET_NOISE,0,0` to the slave.
- `jitter off` sends `SET_JITTER,0` to the slave.
- `VISUALIZER_HOST` / `VISUALIZER_PORT` control where the gateway serves the WebSocket.
- `MIN_RANGE` and `MAX_RANGE` are currently defined but not used by the code.

### `SimulatorProject/config/SimulationConfig.json`
Defines initial anchor locations and the pilot position in 3D space:

```json
{
  "Anchors": [
    { "Id": "1", "point": { "X": 1, "Y": 1, "Z": 1 } },
    { "Id": "2", "point": { "X": 2, "Y": 2, "Z": 3 } },
    { "Id": "3", "point": { "X": 3, "Y": 1, "Z": 2 } }
  ],
  "Pilot": { "point": { "X": 7, "Y": 4, "Z": 3 } }
}
```

## Output
- Distance CSV: `LOGS_PATH` + `DISTANCE_CSV_NAME`
- Positions CSV: `LOGS_PATH` + `POSITIONS_CSV_NAME`
- Distance CSV header: `Timestamp,RequestId,Timer,Distance,Id`
- Positions CSV header: `Timestamp,RequestId,Timer,Type,Id,X,Y,Z`

## Mock Responses (no slave)
Send fake ACK and status lines to test master logging without the simulator.

```bash
bash scripts/mock-responses.sh
```

Custom port and request id:

```bash
bash scripts/mock-responses.sh /tmp/ttyV1 mytestid
```

Why use this:
- Test CSV parsing and logging quickly without running the slave.
- Useful for debugging the master UI and CSV output.
