#!/usr/bin/env bash
# Gera IPA limpo para Sideloadly/AltStore (sem assinatura embarcada do CI).
set -euo pipefail

ROOT="${GITHUB_WORKSPACE:-$(cd "$(dirname "$0")/../.." && pwd)}"
IOS_DIR="$ROOT/ios"
BUILD_DIR="$ROOT/build"
DERIVED="$BUILD_DIR/DerivedData"
IPA_OUT="$BUILD_DIR/TeleportNative-unsigned.ipa"

cd "$IOS_DIR"
test -d Unity-iPhone.xcworkspace || { echo "::error::xcworkspace ausente"; exit 1; }

echo "=== xcodebuild (sem assinatura) ==="
xcodebuild \
  -workspace Unity-iPhone.xcworkspace \
  -scheme Unity-iPhone \
  -configuration Release \
  -destination 'generic/platform=iOS' \
  -derivedDataPath "$DERIVED" \
  build \
  CODE_SIGNING_ALLOWED=NO \
  CODE_SIGNING_REQUIRED=NO \
  CODE_SIGN_IDENTITY="" \
  DEVELOPMENT_TEAM="" \
  PROVISIONING_PROFILE="" \
  PROVISIONING_PROFILE_SPECIFIER=""

APP=$(find "$DERIVED/Build/Products" -path "*Release-iphoneos/*.app" -type d | head -1)
test -n "$APP" || { echo "::error::.app nao encontrado"; find "$DERIVED" -name "*.app"; exit 1; }
echo "App: $APP"

echo "=== Removendo assinaturas residuais (Sideloadly reassina) ==="
# Remove _CodeSignature folders e reassinatura ad-hoc que confunde o Sideloadly
find "$APP" -name "_CodeSignature" -type d -exec rm -rf {} + 2>/dev/null || true
find "$APP" -name "CodeResources" -type f -delete 2>/dev/null || true
while IFS= read -r -d '' bin; do
  codesign --remove-signature "$bin" 2>/dev/null || true
done < <(find "$APP" \( -name "*.dylib" -o -name "*.framework" -o -perm -111 \) -print0 2>/dev/null)
codesign --remove-signature "$APP" 2>/dev/null || true

# Limpa metadata de provisioning embarcado
find "$APP" -name "embedded.mobileprovision" -delete 2>/dev/null || true

STAGE="$BUILD_DIR/Payload"
rm -rf "$STAGE" "$IPA_OUT"
mkdir -p "$STAGE"
cp -R "$APP" "$STAGE/"
(cd "$BUILD_DIR" && zip -qr "$(basename "$IPA_OUT")" Payload)
rm -rf "$STAGE"

ls -la "$IPA_OUT"
echo "IPA pronto para Sideloadly: $IPA_OUT"
