default: format test smoke-test

.PHONY: format build

format:
	$(call begin_group,$@)
	dotnet format --verify-no-changes
	$(call end_group)

build:
	$(call begin_group,$@)
	dotnet build --configuration Release --nologo
	$(call end_group)

test: build
	$(call begin_group,$@)
	dotnet test --configuration Release --no-build
	$(call end_group)

smoke-test: build
	$(call begin_group,$@)
	trap '$(set +x)' EXIT
	set -x
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --help
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-targets
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-dependencies
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-inputs
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-dependencies --list-inputs
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --list-tree --list-inputs
	dotnet run -c Release --no-build --project BullseyeSmokeTester --
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --parallel
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --dry-run
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --skip-dependencies
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --dry-run --skip-dependencies
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- --verbose
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose
	dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose --no-color

	dotnet run -c Release --no-build --project BullseyeSmokeTester -- large-graph --verbose --parallel

	dotnet run -c Release --no-build --project BullseyeSmokeTester.CommandLine -- --help
	dotnet run -c Release --no-build --project BullseyeSmokeTester.CommandLine -- --foo bar --verbose build

	dotnet run -c Release --no-build --project BullseyeSmokeTester.McMaster -- --help
	dotnet run -c Release --no-build --project BullseyeSmokeTester.McMaster -- --foo bar --verbose build

	dotnet run -c Release --no-build --project BullseyeSmokeTester.Parallel

	env NO_COLOR=1 dotnet run -c Release --no-build --project BullseyeSmokeTester -- -h --verbose
	$(call end_group)

# macros
define begin_group
	@if [ "$$GITHUB_ACTIONS" = "true" ]; then echo "::group::$(1)"; fi
endef

define end_group
	@if [ "$$GITHUB_ACTIONS" = "true" ]; then echo "::endgroup::"; fi
endef
