#!/usr/bin/env bash

set -e
if [[ "$#" -ne 1 ]]; then
  echo "Usage: $0 VERSION" >&2
  exit 1
fi

echo Creating packages "$1"
dotnet build -c Release ./Zlepper.RimWorld.ModSdk
dotnet build -c Release ./Zlepper.RimWorld.ModSdk.Testing
dotnet pack "-p:Version=$1" -c Release ./Zlepper.RimWorld.ModSdk
dotnet pack "-p:Version=$1" -c Release ./Zlepper.RimWorld.ModSdk.Testing

dotnet nuget push "./Zlepper.RimWorld.ModSdk/bin/Release/Zlepper.RimWorld.ModSdk.$1.nupkg" -s nuget.org
dotnet nuget push "./Zlepper.RimWorld.ModSdk.Testing/bin/Release/Zlepper.RimWorld.ModSdk.Testing.$1.nupkg" -s nuget.org