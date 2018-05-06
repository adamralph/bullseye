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
:try
dotnet run -c Release --project BullseyeSmokeTester -- --help || goto :catch
dotnet run -c Release --project BullseyeSmokeTester -- --list-targets || goto :catch
dotnet run -c Release --project BullseyeSmokeTester -- --list-dependencies || goto :catch
dotnet run -c Release --project BullseyeSmokeTester -- || goto :catch
dotnet run -c Release --project BullseyeSmokeTester -- --dry-run || goto :catch
dotnet run -c Release --project BullseyeSmokeTester -- --skip-dependencies || goto :catch
dotnet run -c Release --project BullseyeSmokeTester -- --dry-run --skip-dependencies || goto :catch
@echo Off
goto :finally

:catch
@echo Off
exit /b %errorlevel%

:finally
