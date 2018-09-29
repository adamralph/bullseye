#!/usr/bin/env bash
set -euo pipefail

echo "${0##*/}": Building...
dotnet build --configuration Release

echo "${0##*/}": Testing...
dotnet test ./BullseyeTests/BullseyeTests.csproj --configuration Release --no-build --framework netcoreapp2.1

echo "${0##*/}": Smoke testing...
trap `set +x` EXIT
set -x
dotnet run -c Release --project BullseyeSmokeTester -- --help
dotnet run -c Release --project BullseyeSmokeTester -- --list-targets
dotnet run -c Release --project BullseyeSmokeTester -- --list-dependencies
dotnet run -c Release --project BullseyeSmokeTester -- --list-inputs
dotnet run -c Release --project BullseyeSmokeTester -- --list-dependencies --list-inputs
dotnet run -c Release --project BullseyeSmokeTester --
dotnet run -c Release --project BullseyeSmokeTester -- --parallel
dotnet run -c Release --project BullseyeSmokeTester -- --dry-run
dotnet run -c Release --project BullseyeSmokeTester -- --skip-dependencies
dotnet run -c Release --project BullseyeSmokeTester -- --dry-run --skip-dependencies
dotnet run -c Release --project BullseyeSmokeTester -- --verbose
dotnet run -c Release --project BullseyeSmokeTester -- -h --verbose
dotnet run -c Release --project BullseyeSmokeTester -- -h --verbose --no-color
