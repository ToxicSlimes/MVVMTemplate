#!/usr/bin/env bash
set -euo pipefail

UNITY_VERSION="${UNITY_VERSION:-6000.3.17f1}"
PROJECT_PATH="$(cd "$(dirname "$0")/.." && pwd)"
LOG_FILE="$PROJECT_PATH/Logs/ci-compile.log"

find_unity() {
  if [[ -n "${UNITY_PATH:-}" && -x "$UNITY_PATH" ]]; then
    echo "$UNITY_PATH"
    return
  fi

  local hub_linux="/opt/unity/Editor/Unity"
  if [[ -x "$hub_linux" ]]; then
    echo "$hub_linux"
    return
  fi

  local hub_mac="/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity"
  if [[ -x "$hub_mac" ]]; then
    echo "$hub_mac"
    return
  fi

  local hub_win="/c/Program Files/Unity/Hub/Editor/${UNITY_VERSION}/Editor/Unity.exe"
  if [[ -x "$hub_win" ]]; then
    echo "$hub_win"
    return
  fi

  echo "Unity Editor not found. Set UNITY_PATH or install Unity ${UNITY_VERSION}." >&2
  exit 1
}

UNITY_EXE="$(find_unity)"
mkdir -p "$(dirname "$LOG_FILE")"

echo "Project: $PROJECT_PATH"
echo "Unity:   $UNITY_EXE"
echo "Log:     $LOG_FILE"

"$UNITY_EXE" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$PROJECT_PATH" \
  -executeMethod Core.Editor.CI.UnityCompileCheck.Run \
  -logFile "$LOG_FILE"

echo "Unity compile check passed."
