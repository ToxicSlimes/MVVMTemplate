#!/usr/bin/env bash
set -euo pipefail

UNITY_VERSION="${UNITY_VERSION:-6000.3.17f1}"
PROJECT_PATH="$(cd "$(dirname "$0")/.." && pwd)"
LOG_FILE="$PROJECT_PATH/Logs/ci-compile.log"

native_path() {
  local path="$1"
  if command -v cygpath >/dev/null 2>&1; then
    cygpath -w "$path"
  else
    echo "$path"
  fi
}

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
PROJECT_PATH_NATIVE="$(native_path "$PROJECT_PATH")"
LOG_FILE_NATIVE="$(native_path "$LOG_FILE")"
mkdir -p "$(dirname "$LOG_FILE")"

echo "Project: $PROJECT_PATH_NATIVE"
echo "Unity:   $UNITY_EXE"
echo "Log:     $LOG_FILE_NATIVE"

"$UNITY_EXE" \
  -batchmode \
  -nographics \
  -quit \
  -projectPath "$PROJECT_PATH_NATIVE" \
  -executeMethod Core.Editor.CI.UnityCompileCheck.Run \
  -logFile "$LOG_FILE_NATIVE"

echo "Unity compile check passed."
