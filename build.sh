#!/usr/bin/env bash
set -euo pipefail

echo "${0##*/}": Building...
dotnet build --configuration Release --nologo

echo "${0##*/}": Testing...
dotnet test --configuration Release --no-build --nologo "${1:-}" "${2:-}"

echo "${0##*/}": Smoke testing...
trap '$(set +x)' EXIT
set -x
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --help
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-targets
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-dependencies
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-inputs
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-dependencies --list-inputs
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-tree --list-inputs
dotnet run -c Release --no-build --project BullseyeSmokeTester --
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --parallel
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --dry-run
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --skip-dependencies
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --dry-run --skip-dependencies
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --verbose
dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose
dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose --no-color

dotnet run -c Release --no-build --project BullseyeSmokeTester -- large-graph --verbose --parallel

dotnet run -c Release --no-build --project BullseyeSmokeTester.DragonFruit  -- --help
dotnet run -c Release --no-build --project BullseyeSmokeTester.DragonFruit  -- --foo bar --verbose --targets build

dotnet run -c Release --no-build --project BullseyeSmokeTester.CommandLine  -- --help
dotnet run -c Release --no-build --project BullseyeSmokeTester.CommandLine  -- --foo bar --verbose build

dotnet run -c Release --no-build --project BullseyeSmokeTester.McMaster     -- --help
dotnet run -c Release --no-build --project BullseyeSmokeTester.McMaster     -- --foo bar --verbose build

env NO_COLOR=1 dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose
