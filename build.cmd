@echo Off
pwsh -NoProfile -Command "Import-Module psake; Invoke-psake .\psakefile.ps1 %*; if (-not $psake.build_success) { exit 1 }"
