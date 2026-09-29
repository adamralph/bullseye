@echo Off

call :begin_group restore
@echo On
dotnet restore || goto :error
@echo Off
call :end_group

call :begin_group build
@echo On
dotnet build --configuration Release --no-restore || goto :error
@echo Off
call :end_group

call :begin_group test
@echo On
dotnet test --configuration Release --no-build || goto :error
@echo Off
call :end_group

call :begin_group smoke-test
@echo On
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --help || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --list-targets || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --list-dependencies || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --list-inputs || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --list-dependencies --list-inputs || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --list-tree --list-inputs || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --parallel || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --dry-run || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --skip-dependencies || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --dry-run --skip-dependencies || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- --verbose || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- -h --verbose || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- -h --verbose --no-color || goto :error

dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- large-graph --verbose --parallel || goto :error

dotnet run -c Release --no-build --project smoke-tests\SmokeTests.CommandLine -- --help || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests.CommandLine -- --foo bar --verbose build || goto :error

dotnet run -c Release --no-build --project smoke-tests\SmokeTests.McMaster -- --help || goto :error
dotnet run -c Release --no-build --project smoke-tests\SmokeTests.McMaster -- --foo bar --verbose build || goto :error

dotnet run -c Release --no-build --project smoke-tests\SmokeTests.Parallel || goto :error

set NO_COLOR=1
dotnet run -c Release --no-build --project smoke-tests\SmokeTests -- -h --verbose || goto :error

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
