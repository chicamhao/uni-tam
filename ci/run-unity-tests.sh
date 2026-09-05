#!/usr/bin/env bash
# Runs all Unity tests headlessly. Usage: ./ci/run-unity-tests.sh [editmode|playmode|all]
set -euo pipefail
MODE="${1:-all}"
UNITY_PATH="${UNITY_PATH:-$(which unityhub 2>/dev/null && unityhub --version-path 6000.0.23f1 || echo '/Applications/Unity/Hub/Editor/6000.0.23f1/Unity.app/Contents/MacOS/Unity')}"
PROJECT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
TEST_MODE_FLAG=""
case "$MODE" in
  editmode) TEST_MODE_FLAG="--test-mode EditMode" ;;
  playmode) TEST_MODE_FLAG="--test-mode PlayMode" ;;
  all)      TEST_MODE_FLAG="--test-mode EditMode --test-mode PlayMode" ;;
  *) echo "Usage: $0 [editmode|playmode|all]" && exit 1 ;;
esac
"$UNITY_PATH" -batchmode -nographics -projectPath "$PROJECT_DIR" -runTests -testPlatform StandaloneWindows64 $TEST_MODE_FLAG -logFile "$PROJECT_DIR/ci/test-results.log"
echo "Tests complete. Results in ci/test-results.log"