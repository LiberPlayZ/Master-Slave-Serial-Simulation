# Serial Simulation

This repo contains a simple master/slave C# simulation that communicates over a serial link. The master periodically asks the slave for distance data, and the slave replies with a simulated distance based on a moving pilot point and static anchors.

## Projects
- `SimulationMaster`: master process that sends `GET_DISTANCE` and `GET_PILOT_POSITION` requests and logs responses to CSV.
- `SimulatorProject`: slave process that listens on a serial port, simulates movement, and responds with distances and pilot position.
- `SharedConfig`: shared configuration loader (reads `.env` or environment variables).
- `Visualizer`: lightweight web UI that streams data from CSV logs over WebSockets.
- `virtual-ports-launch`: helper script to create a pair of virtual serial ports using `socat`.

## How it works
- The master runs in interactive mode. You type commands like `distance 2`, `pilot`, `anchor 1`, or `status`.
- Each request is wrapped with a request id (`REQ,<id>,...`) so responses can be correlated. The master uses UUIDs for ids.
- The slave ACKs each request (`ACK,<id>`) before sending the data response.
- ACKs make the link more reliable: if the master doesn’t receive an ACK in time, it retries the request.
- The slave advances the pilot position on the X axis based on time elapsed, computes the requested values, waits `RESPONSE_DELAY`, and replies.
- The master writes distance responses to one CSV and position responses to another.

## Wire protocol (current)
Requests (master → slave):
- `REQ,<id>,GET_DISTANCE,<anchorId>` (id is a UUID string)
- `REQ,<id>,GET_PILOT_POSITION` (id is a UUID string)
- `REQ,<id>,GET_ANCHOR_POSITION,<anchorId>` (id is a UUID string)
- `REQ,<id>,GET_STATUS` (id is a UUID string)
- `REQ,<id>,SET_NOISE,<min>,<max>` (id is a UUID string)
- `REQ,<id>,SET_JITTER,<ms>` (id is a UUID string)

ACK (slave → master):
- `ACK,<id>` (id is a UUID string)

Responses (slave → master):
- `DISTANCE,<id>,<timer>,<distance>,<anchorId>` (id is a UUID string)
- `PILOT_POSITION,<id>,<timer>,<x>,<y>,<z>` (id is a UUID string)
- `ANCHOR_POSITION,<id>,<timer>,<anchorId>,<x>,<y>,<z>` (id is a UUID string)
- `STATUS_PILOT,<id>,<timer>,<x>,<y>,<z>` (id is a UUID string)
- `STATUS_ANCHOR,<id>,<timer>,<anchorId>,<x>,<y>,<z>` (id is a UUID string)
- `CONFIG_NOISE,<id>,<min>,<max>` (id is a UUID string)
- `CONFIG_JITTER,<id>,<ms>` (id is a UUID string)

## Prerequisites
- .NET 9 SDK
- `socat` (for virtual serial ports on Linux)
- Bash (to run the provided script)

## Configuration

### `.env`
The shared config comes from `.env` or environment variables. The default `.env` in this repo is:

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
- `SLAVE_PORT_NAME` and `MASTER_PORT_NAME` must match your serial device names (virtual or physical).
- `PILOT_SPEED_TYPE` supports `mps`, `kph`, or `mph`.
- `LOGS_PATH` is relative to the process working directory.
- `ACK_MAX_RETRIES` and `ACK_TIMEOUT_MS` control how long the master waits for ACKs before retrying.
- `DISTANCE_NOISE_MIN` / `DISTANCE_NOISE_MAX` add uniform noise to distance responses (in the same units as distance).
- `RESPONSE_JITTER_MS` adds random response delay on top of `RESPONSE_DELAY`.
- Noise simulates sensor error: each distance is adjusted by a random value between `DISTANCE_NOISE_MIN` and `DISTANCE_NOISE_MAX`.
- Jitter simulates real device variability (processing time, OS scheduling, buffering), so responses are not perfectly uniform.
- `noise off` (master command) sends `SET_NOISE,0,0` to the slave, which disables distance noise and returns exact distances.
- `jitter off` (master command) sends `SET_JITTER,0` to the slave, which disables extra random delay.
- `VISUALIZER_HOST` / `VISUALIZER_PORT` control where the Visualizer gateway serves the WebSocket.
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

## Running locally

### One command (tmux)
Use the tmux launcher to start virtual ports, the slave, and the master in one window with splits:

```bash
bash scripts/run-all.sh
```

Reset (kills existing session and relaunches):

```bash
bash scripts/run-all.sh --reset
```

The tmux layout:
- Left pane: virtual ports (`socat`)
- Right top: slave
- Right bottom: master (interactive input)

### Visualizer (gateway)
Start the gateway in a separate terminal:

```bash
cd Visualizer
dotnet run
```

The gateway reads the CSV logs and streams updates to WebSocket clients.

### Mock responses (no slave)
If you want to test the master logging without running the slave, send mock status responses:

```bash
bash scripts/mock-responses.sh
```

You can pass a custom port and request id:

```bash
bash scripts/mock-responses.sh /tmp/ttyV1 mytestid
```

Why use this:
- It lets you test the master CSV parsing and logging without the simulator running.
- It is useful for debugging the master UI and CSV output format quickly.

How it works:
- It writes a fake ACK and a few `STATUS_*` lines directly to the master port.
- The master treats them like real responses and logs them to `positions.log.csv`.

### Or manual running

### 1) Create virtual serial ports (Linux)
In one terminal:

```bash
bash virtual-ports-launch/start-virtual-serial.sh
```

This creates `/tmp/ttyV0` and `/tmp/ttyV1` and keeps the process running.

### 2) Start the slave (SimulatorProject)
In a second terminal:

```bash
cd SimulatorProject
dotnet run
```

### 3) Start the master (SimulationMaster)
In a third terminal:

```bash
cd SimulationMaster
dotnet run
```

## Output
- The slave prints incoming requests and outgoing replies.
- The master prints each received response and writes CSV rows to:
  - Distance log: `LOGS_PATH` + `DISTANCE_CSV_NAME`
  - Positions log: `LOGS_PATH` + `POSITIONS_CSV_NAME`
- Distance CSV header: `Timestamp,RequestId,Timer,Distance,Id`
- Positions CSV header: `Timestamp,RequestId,Timer,Type,Id,X,Y,Z`

## Notes and limitations
- The master is interactive. Use `distance <id>`, `pilot`, `anchor <id>`, `status`, `noise <min> <max>`, and `jitter <ms>` from the console.
- The slave moves the pilot along the X axis only; adjust `SimulationService.SetNewPilotCordinate` if you want more complex motion.
- If you run the apps from a different working directory, adjust `LOGS_PATH` and `config/SimulationConfig.json` paths accordingly.

## Visualizer UI (React)
To run the React frontend (WebSocket client):

```bash
cd visualizer-ui
npm run dev
```

Then open:

```
http://localhost:5173
```

The React UI connects to the Visualizer gateway WebSocket at `ws://localhost:5080/ws`.
Make sure the Visualizer backend is running:

```bash
cd Visualizer
dotnet run
```
