@echo Off

echo %~nx0: Building...
dotnet build --configuration Release /nologo || goto :error

echo %~nx0: Testing...
dotnet test --configuration Release --no-build /nologo || goto :error

echo %~nx0: Smoke testing...
@echo On
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --help || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-targets || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-dependencies || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-inputs || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-dependencies --list-inputs || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --list-tree --list-inputs || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --parallel || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --dry-run || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --skip-dependencies || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --dry-run --skip-dependencies || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- --verbose || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- -h --verbose || goto :error
dotnet run -c Release --no-build -p BullseyeSmokeTester -- -h --verbose --no-color || goto :error
@echo Off

goto :EOF
:error
@echo Off
exit /b %errorlevel%
