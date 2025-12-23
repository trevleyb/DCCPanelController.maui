#!/usr/bin/env bash
set -euo pipefail

# ========== CONFIG ==========
FRAMEWORK="net10.0-ios"
CONFIGURATION="Release"
PROJECT_PATH="."          # path to your .csproj or solution directory
OUTPUT_ROOT="./artifacts"  # where to copy the .ipa
XCODESWITCH="/Applications/Xcode.app/Contents/Developer" 

CODESIGN_KEY='Apple Distribution: TREVOR GRANT LEYBOURNE (3FBC48TFF3)'
CODESIGN_PROVISION='a425e3b2-0ea0-41ab-98cc-4b5e4c28f954'
    
# Optional signing params (uncomment and set if you need explicit values)
# CODESIGN_KEY='Apple Distribution: Your Company Name'
# CODESIGN_PROVISION='Your Provisioning Profile Name Or UUID'
# ========== END CONFIG ==========

#echo "== Switching Xcode =="
#sudo xcode-select -s "$XCODESWITCH"

echo "== Cleaning output =="
rm -rf "$OUTPUT_ROOT"
mkdir -p "$OUTPUT_ROOT"

echo "== Remove all obj and bin =="
pwd
rm -rf "$PROJECT_PATH/bin"
rm -rf "$PROJECT_PATH/obj"

echo "== dotnet clean =="
dotnet clean "$PROJECT_PATH" -c "$CONFIGURATION"

echo "== dotnet restore =="
dotnet restore "$PROJECT_PATH"

echo "== Publishing iOS (ArchiveOnBuild) =="
PUBLISH_ARGS=(
  "$PROJECT_PATH"
  -f "$FRAMEWORK"
  -c "$CONFIGURATION"
  -p:ArchiveOnBuild=true
  -p:BuildIpa=true
  -p:CodesignKey="$CODESIGN_KEY"
  -p:CodesignProvision="$CODESIGN_PROVISION"
)

dotnet publish "${PUBLISH_ARGS[@]}"

echo "== Locating generated .ipa =="
# Typical MAUI iOS IPA output path pattern
IPA_PATH=$(find "$PROJECT_PATH" -type f -path "*/bin/$CONFIGURATION/$FRAMEWORK/ios-arm64/publish/*.ipa" -print -quit)

if [[ -z "${IPA_PATH:-}" ]]; then
  # Fallback search if layout differs
  IPA_PATH=$(find "$PROJECT_PATH" -type f -name "*.ipa" -print -quit)
fi

if [[ -z "${IPA_PATH:-}" ]]; then
  echo "ERROR: Could not locate the .ipa file. Check publish output paths."
  exit 1
fi

echo "Found IPA: $IPA_PATH"

echo "== Copying IPA to artifacts =="
DEST_IPA="$OUTPUT_ROOT/$(basename "$IPA_PATH")"
cp "$IPA_PATH" "$DEST_IPA"

codesign -d --entitlements :- artifacts/DCCPanelController.ipa

echo "== Done =="
echo "IPA ready for Transporter:"
echo "$DEST_IPA"
