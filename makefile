default: format test smoke-test pack

.PHONY: format build

restore:
	$(call begin_group,$@)
	dotnet restore
	$(call end_group)

format: restore
	$(call begin_group,$@)
	dotnet format --verify-no-changes --no-restore
	$(call end_group)

build: restore
	$(call begin_group,$@)
	dotnet build --configuration Release --no-restore
	$(call end_group)

test: build
	$(call begin_group,$@)
	dotnet test --configuration Release --no-build
	$(call end_group)

smoke-test: build
	$(call begin_group,$@)
	trap '$(set +x)' EXIT
	set -x
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --help
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-targets
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-dependencies
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-inputs
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-dependencies --list-inputs
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --list-tree --list-inputs
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests --
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --parallel
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --dry-run
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --skip-dependencies
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --dry-run --skip-dependencies
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- --verbose
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- -h --verbose
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- -h --verbose --no-color

	dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- large-graph --verbose --parallel

	dotnet run -c Release --no-build --project smoke-tests/SmokeTests.CommandLine -- --help
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests.CommandLine -- --foo bar --verbose build

	dotnet run -c Release --no-build --project smoke-tests/SmokeTests.McMaster -- --help
	dotnet run -c Release --no-build --project smoke-tests/SmokeTests.McMaster -- --foo bar --verbose build

	dotnet run -c Release --no-build --project smoke-tests/SmokeTests.Parallel

	env NO_COLOR=1 dotnet run -c Release --no-build --project smoke-tests/SmokeTests -- -h --verbose
	$(call end_group)

pack: build
	$(call begin_group,$@)
	dotnet pack --configuration Release --output artifacts --no-build
	$(call end_group)

# macros
define begin_group
	@if [ "$$GITHUB_ACTIONS" = "true" ]; then echo "::group::$(1)"; fi
endef

define end_group
	@if [ "$$GITHUB_ACTIONS" = "true" ]; then echo "::endgroup::"; fi
endef
