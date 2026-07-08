#!/usr/bin/env bash
# Assina e exporta IPA iOS (development) apos export Unity/Xcode.
# Requer: ios/Unity-iPhone.xcworkspace, secrets Apple no ambiente.
set -euo pipefail

ROOT="${GITHUB_WORKSPACE:-$(cd "$(dirname "$0")/../.." && pwd)}"
IOS_DIR="$ROOT/ios"
BUILD_DIR="$ROOT/build"
ARCHIVE_PATH="$BUILD_DIR/TeleportNative.xcarchive"
EXPORT_DIR="$BUILD_DIR/ipa"
IPA_OUT="$BUILD_DIR/TeleportNative.ipa"
KEYCHAIN="${RUNNER_TEMP:-/tmp}/teleport-signing.keychain-db"

: "${APPLE_TEAM_ID:?Defina APPLE_TEAM_ID}"
: "${APPLE_SIGNING_IDENTITY:?Defina APPLE_SIGNING_IDENTITY}"
: "${KEYCHAIN_PASSWORD:?Defina KEYCHAIN_PASSWORD}"
: "${APPLE_CERTIFICATE_BASE64:?Defina APPLE_CERTIFICATE_BASE64}"
: "${APPLE_CERTIFICATE_PASSWORD:?Defina APPLE_CERTIFICATE_PASSWORD}"
: "${APPLE_PROVISIONING_PROFILE_BASE64:?Defina APPLE_PROVISIONING_PROFILE_BASE64}"

mkdir -p "$BUILD_DIR" "$EXPORT_DIR"
CERT_PATH="$BUILD_DIR/apple_cert.p12"
PROFILE_PATH="$BUILD_DIR/profile.mobileprovision"

echo "$APPLE_CERTIFICATE_BASE64" | base64 --decode > "$CERT_PATH"
echo "$APPLE_PROVISIONING_PROFILE_BASE64" | base64 --decode > "$PROFILE_PATH"

PROFILE_UUID=$(security cms -D -i "$PROFILE_PATH" 2>/dev/null | plutil -extract UUID raw - 2>/dev/null || true)
if [ -z "$PROFILE_UUID" ]; then
  PROFILE_UUID=$(grep -aA1 '<key>UUID</key>' "$PROFILE_PATH" | grep -oE '[0-9A-Fa-f-]{36}' | head -1)
fi
if [ -z "$PROFILE_UUID" ]; then
  echo "::error::Nao foi possivel extrair UUID do provisioning profile"
  exit 1
fi
echo "Provisioning profile UUID: $PROFILE_UUID"
export APPLE_PROVISIONING_PROFILE_UUID="$PROFILE_UUID"

PP_DIR="$HOME/Library/MobileDevice/Provisioning Profiles"
mkdir -p "$PP_DIR"
cp "$PROFILE_PATH" "$PP_DIR/${PROFILE_UUID}.mobileprovision"

security delete-keychain "$KEYCHAIN" 2>/dev/null || true
security create-keychain -p "$KEYCHAIN_PASSWORD" "$KEYCHAIN"
security set-keychain-settings -lut 21600 "$KEYCHAIN"
security unlock-keychain -p "$KEYCHAIN_PASSWORD" "$KEYCHAIN"
security import "$CERT_PATH" -k "$KEYCHAIN" -P "$APPLE_CERTIFICATE_PASSWORD" -A -T /usr/bin/codesign -T /usr/bin/security
security set-key-partition-list -S apple-tool:,apple:,codesign: -s -k "$KEYCHAIN_PASSWORD" "$KEYCHAIN"
security list-keychain -d user -s "$KEYCHAIN" login.keychain

cd "$IOS_DIR"
test -d Unity-iPhone.xcworkspace || { echo "::error::Unity-iPhone.xcworkspace ausente"; exit 1; }

if [ ! -f ExportOptions.plist ] || ! grep -q "$APPLE_TEAM_ID" ExportOptions.plist 2>/dev/null; then
  cat > ExportOptions.plist <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>method</key>
  <string>development</string>
  <key>teamID</key>
  <string>${APPLE_TEAM_ID}</string>
  <key>signingStyle</key>
  <string>manual</string>
  <key>compileBitcode</key>
  <false/>
  <key>provisioningProfiles</key>
  <dict>
    <key>com.teleportnative.app</key>
    <string>${PROFILE_UUID}</string>
    <key>com.teleportnative.app.UnityFramework</key>
    <string>${PROFILE_UUID}</string>
  </dict>
</dict>
</plist>
EOF
fi

echo "=== xcodebuild archive ==="
xcodebuild \
  -workspace Unity-iPhone.xcworkspace \
  -scheme Unity-iPhone \
  -configuration Release \
  -destination 'generic/platform=iOS' \
  -archivePath "$ARCHIVE_PATH" \
  archive \
  DEVELOPMENT_TEAM="$APPLE_TEAM_ID" \
  CODE_SIGN_STYLE=Manual \
  CODE_SIGN_IDENTITY="$APPLE_SIGNING_IDENTITY" \
  PROVISIONING_PROFILE_SPECIFIER="$PROFILE_UUID" \
  OTHER_CODE_SIGN_FLAGS="--keychain $KEYCHAIN"

echo "=== xcodebuild -exportArchive ==="
xcodebuild -exportArchive \
  -archivePath "$ARCHIVE_PATH" \
  -exportPath "$EXPORT_DIR" \
  -exportOptionsPlist ExportOptions.plist

EXPORTED_IPA=$(find "$EXPORT_DIR" -maxdepth 1 -name "*.ipa" | head -1)
test -n "$EXPORTED_IPA" || { echo "::error::IPA nao gerado em $EXPORT_DIR"; ls -la "$EXPORT_DIR"; exit 1; }
cp "$EXPORTED_IPA" "$IPA_OUT"

echo "=== Validando assinatura do IPA ==="
VERIFY_DIR="$BUILD_DIR/ipa-verify"
rm -rf "$VERIFY_DIR"
mkdir -p "$VERIFY_DIR"
unzip -q "$IPA_OUT" -d "$VERIFY_DIR"
APP=$(find "$VERIFY_DIR/Payload" -maxdepth 1 -name "*.app" -type d | head -1)
test -n "$APP" || { echo "::error::.app ausente no IPA"; exit 1; }

codesign --verify --deep --strict --verbose=2 "$APP"
codesign -dv --verbose=2 "$APP" 2>&1 | head -20

for fw in "$APP/Frameworks/"*.framework; do
  [ -d "$fw" ] || continue
  echo "Verificando $(basename "$fw")..."
  codesign --verify --verbose=2 "$fw"
done

if [ -d "$APP/Frameworks/UnityRuntime.framework" ]; then
  codesign --verify --verbose=2 "$APP/Frameworks/UnityRuntime.framework"
  echo "UnityRuntime.framework: OK"
fi

ls -la "$IPA_OUT"
echo "IPA assinado: $IPA_OUT"
