#!/usr/bin/env bash
set -euo pipefail

echo "${0##*/}": Building...
dotnet build --configuration Release --nologo

echo "${0##*/}": Testing...
dotnet test --configuration Release --no-build --nologo --logger GitHubActions

echo "${0##*/}": Smoke testing...
trap '$(set +x)' EXIT
set -x
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --help
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-targets
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-dependencies
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-inputs
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-dependencies --list-inputs
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-tree --list-inputs
dotnet run -c Release --no-build -p BullseyeSmokeTester --
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --parallel
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --dry-run
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --skip-dependencies
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --dry-run --skip-dependencies
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --verbose
dotnet run -c Release --no-build -p BullseyeSmokeTester -- -h --verbose
dotnet run -c Release --no-build -p BullseyeSmokeTester -- -h --verbose --no-color

dotnet run -c Release --no-build -p BullseyeSmokeTester -- large-graph --verbose --parallel

dotnet run -c Release --no-build -p BullseyeSmokeTester.DragonFruit  -- --help
dotnet run -c Release --no-build -p BullseyeSmokeTester.DragonFruit  -- --foo bar --verbose --targets build

# https://github.com/dotnet/command-line-api/issues/1306
if [ "${CIRCLECI:-false}" == "false" ]; then
  dotnet run -c Release --no-build -p BullseyeSmokeTester.CommandLine  -- --help
  dotnet run -c Release --no-build -p BullseyeSmokeTester.CommandLine  -- --foo bar --verbose build
fi

dotnet run -c Release --no-build -p BullseyeSmokeTester.McMaster     -- --help
dotnet run -c Release --no-build -p BullseyeSmokeTester.McMaster     -- --foo bar --verbose build

env NO_COLOR=1 dotnet run -c Release --no-build -p BullseyeSmokeTester -- -h --verbose
