# Serial Simulation

This repo contains a simple master/slave C# simulation that communicates over a serial link. The master periodically asks the slave for distance data, and the slave replies with a simulated distance based on a moving pilot point and static anchors.

## Projects
- `SimulationMaster`: master process that sends `GET_DISTANCE` and `GET_PILOT_POSITION` requests and logs responses to CSV.
- `SimulatorProject`: slave process that listens on a serial port, simulates movement, and responds with distances and pilot position.
- `SharedConfig`: shared configuration loader (reads `.env` or environment variables).
- `virtual-ports-launch`: helper script to create a pair of virtual serial ports using `socat`.

## How it works
- The master cycles anchor ids `1 -> 3` and sends `GET_DISTANCE:<id>` every 5 seconds.
- After each full cycle (3 distance requests), the master sends a `GET_PILOT_POSITION` request.
- The slave reads the request, advances the pilot position on the X axis based on time elapsed, computes Euclidean distance to the requested anchor, waits `RESPONSE_DELAY`, and replies.
- The master writes distance responses to one CSV and pilot position responses to another.

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
```

Notes:
- `SLAVE_PORT_NAME` and `MASTER_PORT_NAME` must match your serial device names (virtual or physical).
- `PILOT_SPEED_TYPE` supports `mps`, `kph`, or `mph`.
- `LOGS_PATH` is relative to the process working directory.
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
  - Pilot log: `LOGS_PATH` + `PILOT_CSV_NAME`
- Distance CSV header: `Timestamp,Time,Distance,Id`
- Pilot CSV header: `Timestamp,X,Y,Z`

## Notes and limitations
- The master currently cycles only anchor ids `1..3`. If you add more anchors, update `SimulationMaster/services/SerialService.cs`.
- The slave moves the pilot along the X axis only; adjust `SimulationService.SetNewPilotCordinate` if you want more complex motion.
- If you run the apps from a different working directory, adjust `LOGS_PATH` and `config/SimulationConfig.json` paths accordingly.
