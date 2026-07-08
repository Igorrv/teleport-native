#!/usr/bin/env bash
# Build iOS local (macOS) ou referencia para CI.
# Uso: ./scripts/build_ios.sh
set -euo pipefail
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_VERSION="${UNITY_VERSION:-6000.5.2f1}"
UNITY_BIN="${UNITY_BIN:-/Applications/Unity/Hub/Editor/${UNITY_VERSION}/Unity.app/Contents/MacOS/Unity}"

cd "$ROOT"

if [[ "$(uname)" != "Darwin" ]]; then
  echo "iOS build exige macOS. No Windows use GitHub Actions (ios-build.yml)."
  exit 1
fi

echo "[build_ios] Unity export..."
"$UNITY_BIN" -batchmode -nographics -quit \
  -projectPath "$ROOT" \
  -executeMethod TeleportNative.Editor.BuildiOS.Build \
  -logFile "$ROOT/Build/unity-export.log"

echo "[build_ios] pod install..."
cd ios && pod install

echo "[build_ios] xcodebuild (unsigned)..."
xcodebuild \
  -workspace Unity-iPhone.xcworkspace \
  -scheme Unity-iPhone \
  -configuration Release \
  -destination 'generic/platform=iOS' \
  -derivedDataPath "$ROOT/build/DerivedData" \
  build \
  CODE_SIGNING_ALLOWED=NO CODE_SIGNING_REQUIRED=NO

APP=$(find "$ROOT/build/DerivedData" -name "*.app" -type d | grep -v PlugIns | head -1)
mkdir -p "$ROOT/build/ios/Payload"
cp -R "$APP" "$ROOT/build/ios/Payload/"
cd "$ROOT/build/ios"
zip -qr TeleportNative-unsigned.ipa Payload
rm -rf Payload

echo "[build_ios] OK: build/ios/TeleportNative-unsigned.ipa"
