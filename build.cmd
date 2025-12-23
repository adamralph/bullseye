@echo Off

call :begin_group Formatting
@echo On
dotnet format --verify-no-changes || goto :error
@echo Off
call :end_group

call :begin_group Building
@echo On
dotnet build --configuration Release --nologo || goto :error
@echo Off
call :end_group

call :begin_group Testing
@echo On
dotnet test --configuration Release --no-build || goto :error
@echo Off
call :end_group

call :begin_group "Smoke testing"
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
call :end_group

goto :EOF

:begin_group
if "%GITHUB_ACTIONS%"=="true" echo ::group::%~1
exit /b 0

:end_group
if "%GITHUB_ACTIONS%"=="true" echo ::endgroup::
exit /b 0

:error
@echo Off
exit /b %errorlevel%
