@echo Off
cd %~dp0

echo %~nx0: Restoring...
dotnet restore || goto :error

echo %~nx0: Building and testing...
pushd BullseyeTests
dotnet xunit -configuration Release || goto :error
popd

echo %~nx0: Smoke testing...
@echo On
dotnet run -c Release --project BullseyeSmokeTester -- --help || goto :error
dotnet run -c Release --project BullseyeSmokeTester -- --list-targets || goto :error
dotnet run -c Release --project BullseyeSmokeTester -- --list-dependencies || goto :error
dotnet run -c Release --project BullseyeSmokeTester -- || goto :error
dotnet run -c Release --project BullseyeSmokeTester -- --dry-run || goto :error
dotnet run -c Release --project BullseyeSmokeTester -- --skip-dependencies || goto :error
dotnet run -c Release --project BullseyeSmokeTester -- --dry-run --skip-dependencies || goto :error
@echo Off

goto :EOF
:error
@echo Off
exit /b %errorlevel%
