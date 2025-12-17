@echo Off

echo %~nx0: Formatting...
dotnet format --verify-no-changes || goto :error

echo %~nx0: Building...
dotnet build --configuration Release --nologo || goto :error

echo %~nx0: Testing...
dotnet test --configuration Release --no-build --nologo %1 %2 || goto :error

echo %~nx0: Smoke testing...
@echo On
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --help || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-targets || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-dependencies || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-inputs || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-dependencies --list-inputs || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-tree --list-inputs || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --parallel || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --dry-run || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --skip-dependencies || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --dry-run --skip-dependencies || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- --verbose || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose --no-color || goto :error

dotnet run -c Release --no-build --project BullseyeSmokeTester -- large-graph --verbose --parallel || goto :error

dotnet run -c Release --no-build --project BullseyeSmokeTester.CommandLine -- --help || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester.CommandLine -- --foo bar --verbose build || goto :error

dotnet run -c Release --no-build --project BullseyeSmokeTester.McMaster -- --help || goto :error
dotnet run -c Release --no-build --project BullseyeSmokeTester.McMaster -- --foo bar --verbose build || goto :error

dotnet run -c Release --no-build --project BullseyeSmokeTester.Parallel || goto :error

set NO_COLOR=1
dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose || goto :error

@echo Off

goto :EOF
:error
@echo Off
exit /b %errorlevel%
