#!/usr/bin/env bash
PROJECT_PATH="./"          # path to your .csproj or solution directory
dotnet build -t:InstallAndroidDependencies \
  -f net10.0-android \
  -p:AndroidSdkDirectory=/Users/trevor/Library/Android/sdk \
  -p:AcceptAndroidSdkLicenses=true
  
echo "== Done =="
