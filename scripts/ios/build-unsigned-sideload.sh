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
  CODE_SIGN_IDENTITY="-" \
  DEVELOPMENT_TEAM="" \
  PROVISIONING_PROFILE="" \
  PROVISIONING_PROFILE_SPECIFIER="" \
  EXPANDED_CODE_SIGN_IDENTITY="" \
  CODE_SIGNING_ENTITLEMENTS=""

APP=$(find "$DERIVED/Build/Products" -path "*Release-iphoneos/*.app" -type d | head -1)
test -n "$APP" || { echo "::error::.app nao encontrado"; find "$DERIVED" -name "*.app"; exit 1; }
echo "App: $APP"

strip_all_signatures() {
  local root="$1"
  echo "=== Strip assinaturas em $root ==="
  find "$root" -name "_CodeSignature" -type d -exec rm -rf {} + 2>/dev/null || true
  find "$root" -name "CodeResources" -type f -delete 2>/dev/null || true
  find "$root" -name "*.mobileprovision" -delete 2>/dev/null || true
  find "$root" -name "SC_Info" -type d -exec rm -rf {} + 2>/dev/null || true
  xattr -cr "$root" 2>/dev/null || true

  while IFS= read -r -d '' f; do
    if file "$f" 2>/dev/null | grep -qE 'Mach-O|executable'; then
      codesign --remove-signature "$f" 2>/dev/null || true
    fi
  done < <(find "$root" -type f -print0)

  # Frameworks: remove assinatura do bundle e do binario interno
  find "$root" -name "*.framework" -type d | while read -r fw; do
    codesign --remove-signature "$fw" 2>/dev/null || true
    name=$(basename "$fw" .framework)
    if [ -f "$fw/$name" ]; then
      codesign --remove-signature "$fw/$name" 2>/dev/null || true
    fi
  done
}

strip_all_signatures "$APP"

echo "=== Verificacao (nao deve listar assinatura valida) ==="
codesign -dv "$APP" 2>&1 || echo "(app sem assinatura — OK para Sideloadly)"
if [ -d "$APP/Frameworks/UnityRuntime.framework" ]; then
  codesign -dv "$APP/Frameworks/UnityRuntime.framework" 2>&1 || echo "(UnityRuntime sem assinatura — OK)"
fi

STAGE="$BUILD_DIR/Payload"
rm -rf "$STAGE" "$IPA_OUT"
mkdir -p "$STAGE"
ditto "$APP" "$STAGE/$(basename "$APP")"
(cd "$BUILD_DIR" && zip -qr "$(basename "$IPA_OUT")" Payload)
rm -rf "$STAGE"

ls -la "$IPA_OUT"
echo "IPA pronto para Sideloadly: $IPA_OUT"
