@echo Off
cd %~dp0
setlocal EnableDelayedExpansion

echo %~nx0: Restoring...
dotnet restore || exit /b

pushd BullseyeTests
echo %~nx0: Building and testing...
dotnet xunit -configuration Release || exit /b
popd

echo %~nx0: Smoke testing...
@echo On
dotnet run -c Release --project BullseyeSmokeTester -- --unknown-option --another-unknown-option
dotnet run -c Release --project BullseyeSmokeTester -- --help || exit /b
dotnet run -c Release --project BullseyeSmokeTester -- --list-targets || exit /b
dotnet run -c Release --project BullseyeSmokeTester -- --list-dependencies || exit /b
dotnet run -c Release --project BullseyeSmokeTester -- || exit /b
dotnet run -c Release --project BullseyeSmokeTester -- --dry-run || exit /b
dotnet run -c Release --project BullseyeSmokeTester -- --skip-dependencies || exit /b
dotnet run -c Release --project BullseyeSmokeTester -- --dry-run --skip-dependencies || exit /b
