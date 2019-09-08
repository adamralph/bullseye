#!/usr/bin/env bash
set -euo pipefail

dotnet nuget push $(find -name "*.nupkg") --source https://www.myget.org/F/adamralph-ci/api/v2/package --api-key $1
