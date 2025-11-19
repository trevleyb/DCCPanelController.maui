#!/usr/bin/env bash
PROJECT_PATH="./"          # path to your .csproj or solution directory
OUTPUT_ROOT="./artifacts"  # where to copy the .ipa

echo "== Cleaning output =="
rm -rf "$OUTPUT_ROOT"
mkdir -p "$OUTPUT_ROOT"

echo "== dotnet clean =="
dotnet clean "$PROJECT_PATH" -c "$CONFIGURATION"
rm -rf "$PROJECT_PATH/bin"
rm -rf "$PROJECT_PATH/obj"

echo "== dotnet restore =="
dotnet restore "$PROJECT_PATH"

echo "== Done =="
