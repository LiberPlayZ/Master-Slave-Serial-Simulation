#!/bin/bash
set -euo pipefail

SESSION_NAME="Master_slave_project"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if ! command -v tmux >/dev/null 2>&1; then
  echo "tmux is required but not installed."
  exit 1
fi

if [[ "${1:-}" == "--reset" ]]; then
  if tmux has-session -t "$SESSION_NAME" 2>/dev/null; then
    tmux kill-session -t "$SESSION_NAME"
  fi
fi

if tmux has-session -t "$SESSION_NAME" 2>/dev/null; then
  echo "tmux session '$SESSION_NAME' already exists."
  echo "Attach with: tmux attach -t $SESSION_NAME"
  exit 0
fi

tmux new-session -d -s "$SESSION_NAME" -n "ports"
tmux send-keys -t "$SESSION_NAME":0 "bash \"$ROOT_DIR/scripts/start-virtual-serial.sh\"" C-m

tmux split-window -h -t "$SESSION_NAME":0
tmux split-window -v -t "$SESSION_NAME":0.1
tmux send-keys -t "$SESSION_NAME":0.1 "cd \"$ROOT_DIR/SimulatorProject\" && dotnet run" C-m
tmux send-keys -t "$SESSION_NAME":0.2 "cd \"$ROOT_DIR/SimulationMaster\" && dotnet run" C-m
tmux select-pane -t "$SESSION_NAME":0.2

if command -v x-terminal-emulator >/dev/null 2>&1; then
  x-terminal-emulator -e tmux attach -t "$SESSION_NAME"
elif command -v gnome-terminal >/dev/null 2>&1; then
  gnome-terminal -- tmux attach -t "$SESSION_NAME"
elif command -v xfce4-terminal >/dev/null 2>&1; then
  xfce4-terminal -e "tmux attach -t $SESSION_NAME"
elif command -v xterm >/dev/null 2>&1; then
  xterm -e tmux attach -t "$SESSION_NAME"
else
  echo "No supported terminal emulator found. Attach with: tmux attach -t $SESSION_NAME"
fi
