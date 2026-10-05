Task 'default' -Depends 'format', 'test', 'smoke-test', 'pack'

Task 'restore' {
    Begin-Group 'restore'
    Exec { dotnet restore }
    End-Group
}

Task 'format' -Depends 'restore' {
    Begin-Group 'format'
    Exec { dotnet format --verify-no-changes --no-restore }
    End-Group
}

Task 'build' -Depends 'restore' {
    Begin-Group 'build'
    Exec { dotnet build --configuration Release --no-restore }
    End-Group
}

Task 'test' -Depends 'build' {
    Begin-Group 'test'
    Exec { dotnet test --configuration Release --no-build }
    End-Group
}

Task 'smoke-test' -Depends 'build' {
    Begin-Group 'smoke-test'
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --help }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-targets }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-dependencies }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-inputs }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-dependencies --list-inputs }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-tree --list-inputs }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --parallel }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --dry-run }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --skip-dependencies }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --dry-run --skip-dependencies }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --verbose }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- -h --verbose }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- -h --verbose --no-color }

    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- large-graph --verbose --parallel }

    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests.CommandLine -- --help }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests.CommandLine -- --foo bar --verbose build }

    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests.McMaster -- --help }
    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests.McMaster -- --foo bar --verbose build }

    Exec { dotnet run -c Release --no-build --project smoke-tests/SmokeTests.Parallel }

    Exec { cmd /c "set NO_COLOR=1&& dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- -h --verbose" }
    End-Group
}

Task 'pack' -Depends 'build' {
    Begin-Group 'pack'
    Exec { dotnet pack --configuration Release --output artifacts --no-build }
    End-Group
}

function Begin-Group([string] $name) {
    if ($env:GITHUB_ACTIONS -eq 'true') { Write-Host "::group::$name" }
}

function End-Group {
    if ($env:GITHUB_ACTIONS -eq 'true') { Write-Host '::endgroup::' }
}
