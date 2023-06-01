# Bullseye

![Bullseye](https://raw.githubusercontent.com/adamralph/bullseye/958af18a096239e9a040ff22c5d8bf08f0fc2466/assets/bullseye.svg)

_[![NuGet version](https://img.shields.io/nuget/v/Bullseye.svg?style=flat)](https://www.nuget.org/packages/Bullseye)_

_[![Build status](https://github.com/adamralph/bullseye/workflows/.github/workflows/ci.yml/badge.svg)](https://github.com/adamralph/bullseye/actions/workflows/ci.yml?query=branch%3Amain)_
_[![CodeQL analysis](https://github.com/adamralph/bullseye/workflows/.github/workflows/codeql-analysis.yml/badge.svg)](https://github.com/adamralph/bullseye/actions/workflows/codeql-analysis.yml?query=branch%3Amain)_
_[![Lint](https://github.com/adamralph/bullseye/workflows/.github/workflows/lint.yml/badge.svg)](https://github.com/adamralph/bullseye/actions/workflows/lint.yml?query=branch%3Amain)_
_[![Spell check](https://github.com/adamralph/bullseye/workflows/.github/workflows/spell-check.yml/badge.svg)](https://github.com/adamralph/bullseye/actions/workflows/spell-check.yml?query=branch%3Amain)_

_[![AppVeyor smoke test status](https://img.shields.io/appveyor/ci/adamralph/bullseye/main.svg?logo=appveyor&label=AppVeyor)](https://ci.appveyor.com/project/adamralph/bullseye/branch/main)_
_[![CircleCI smoke test status](https://img.shields.io/circleci/build/github/adamralph/bullseye/main?logo=circleci&label=CircleCI)](https://circleci.com/gh/adamralph/bullseye/tree/main)_
_[![Cirrus CI smoke test status](https://img.shields.io/cirrus/github/adamralph/bullseye/main?logo=cirrus-ci&label=Cirrus%20CI)](https://cirrus-ci.com/github/adamralph/bullseye)_
_[![GitLab CI/CD smoke test status](https://img.shields.io/gitlab/pipeline/adamralph/bullseye/main.svg?logo=gitlab&label=GitLab+CI%2fCD)](https://gitlab.com/adamralph/bullseye/-/jobs)_

Bullseye is a [.NET library](https://www.nuget.org/packages/Bullseye) that runs a target dependency graph.

Bullseye is primarily designed as a build tool for .NET projects, and is usually used together with [SimpleExec](https://gitlab.com/adamralph/simple-exec), but Bullseye targets can do anything. They are not restricted to building .NET projects.

Platform support: [.NET 6.0 and later](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

<!-- spell-checker:disable -->
- [Quick start](#quick-start)
- [Sample wrapper scripts](#sample-wrapper-scripts)
- [Enumerable inputs](#enumerable-inputs)
- [Command-line arguments](#command-line-arguments)
- [Non-static API](#non-static-api)
- [NO\_COLOR](#no_color)
- [Who's using Bullseye?](#whos-using-bullseye)
<!-- spell-checker:enable -->

## Quick start

- Next to an existing .NET solution (`.sln` file), add a .NET console app named `targets` — `dotnet new console --name targets`
- Change to the new directory — `cd targets`
- Add a reference to [Bullseye](https://www.nuget.org/packages/Bullseye) — `dotnet add targets package Bullseye`
- Add a reference to [SimpleExec](https://www.nuget.org/packages/SimpleExeNew) — `dotnet add targets package SimpleExec`
- Replace the contents of `targets/Program.cs` with:

  ```c#
  using static Bullseye.Targets;
  using static SimpleExec.Command;

  Target("build", () => RunAsync("dotnet", "build"));
  Target("test", DependsOn("build"), () => RunAsync("dotnet", "test"));
  Target("default", DependsOn("test"));

  await RunTargetsAndExitAsync(args, ex => ex is SimpleExec.ExitCodeException);
  ```

- Change to the solution directory — `cd ..`
- Run the targets project — `dotnet run --project targets`.

Voilà! You've just written and run your first Bullseye build program. You will see output similar to:

<img src="https://user-images.githubusercontent.com/677704/147760642-36018691-4710-41be-bd65-5dcfac121fc5.png" width="357px" />

For help, run `dotnet run --project targets --help`.

## Sample wrapper scripts

- `build` (Linux and macOS)

  ```shell
  #!/usr/bin/env bash
  set -euo pipefail
  dotnet run --project targets -- "$@"
  ```

- `build.cmd` (Windows)

  ```batchfile
  @echo Off
  dotnet run --project targets -- %*
  ```

## Enumerable inputs

For example, you may want to run your test projects one by one, so that the timing of each one and which one, if any, failed, is displayed in the Bullseye build summary:

```c#
Target(
    "test",
    ForEach("MySolutionTests1", "MySolutionTests2"),
    project => RunAsync($"dotnet test {project}")));
```

```shell
dotnet run -- test
```

<img src="https://user-images.githubusercontent.com/677704/147761855-76d3a77a-4342-4b00-913b-a52188a65793.png" width="491px" />

## Command-line arguments

Generally, all the command-line arguments passed to `Program.cs` should be passed along to Bullseye, as shown in the quick start above (`RunTargetsAndExitAsync(args);`). This is because Bullseye effectively provides a command-line interface, with options for displaying a list of targets, performing dry runs, suppressing colour, and more. For full details of the command-line options, run your targets project supplying the `--help` (`-h`/`-?`) option:

```shell
dotnet run --project targets -- --help
```

```shell
./build --help
```

```batchfile
./build.cmd --help
```

You can also handle custom arguments in `Program.cs`, but you should ensure that only valid arguments are passed along to Bullseye and that the help text contains both your custom arguments and the arguments supported by Bullseye. A good way to do this is to use a command-line parsing package to define your custom arguments, and to provide translation between the package and Bullseye. For example, see the test projects for:

- [`System.CommandLine`](BullseyeSmokeTester.CommandLine/Program.cs)
- [`System.CommandLine.DragonFruit`](BullseyeSmokeTester.DragonFruit/Program.cs)
- [`McMaster.Extensions.CommandLineUtils`](BullseyeSmokeTester.McMaster/Program.cs)

## Non-static API

For most cases, the static API described above is sufficient. For more complex scenarios where a number of target collections are required, the non-static API may be used.

```c#
var targets1 = new Targets();
targets1.Add("foo", () => Console.Out.WriteLine("foo1"));

var targets2 = new Targets();
targets2.Add("foo", () => Console.Out.WriteLine("foo2"));

await targets1.RunWithoutExitingAsync(args);
await targets2.RunWithoutExitingAsync(args);
```

## NO_COLOR

Bullseye supports [NO_COLOR](https://no-color.org/).

## Who's using Bullseye?

To name a few:

- [AspNetCore.AsyncInitialization](https://github.com/thomaslevesque/AspNetCore.AsyncInitialization)
- [Config.SqlStreamStore](https://github.com/Erwinvandervalk/Config.SqlStreamStore)
- [ConfigR](https://github.com/config-r)
- [Elastic](https://github.com/elastic)
- [EssentialMVVM](https://github.com/thomaslevesque/EssentialMVVM)
- [FakeItEasy](https://github.com/FakeItEasy)
- [HumanBytes](https://github.com/thomaslevesque/HumanBytes)
- [Ibento](https://github.com/pgermishuys/Ibento)
- [IdentityModel](https://github.com/IdentityModel)
- [IdentityServer](https://github.com/IdentityServer)
- [Iso8601DurationHelper](https://github.com/thomaslevesque/Iso8601DurationHelper)
- [Linq.Extras](https://github.com/thomaslevesque/Linq.Extras)
- [LittleForker](https://github.com/damianh/LittleForker)
- [LykkeOSS](https://github.com/LykkeOSS)
- [Marten](https://github.com/JasperFx/marten)
- [MinVer](https://github.com/adamralph/minver)
- [Particular](https://github.com/Particular)
- [ProxyKit](https://github.com/damianh/ProxyKit)
- [PseudoLocalizer](https://github.com/bymyslf/PseudoLocalizer)
- [Radical Framework](https://github.com/RadicalFx)
- [RealWorld](https://github.com/gothinkster/aspnetcore-realworld-example-app)
- [SelfInitializingFakes](https://github.com/blairconrad/SelfInitializingFakes)
- [SendComics](https://github.com/blairconrad/SendComics)
- [SqlStreamStore.Locking](https://github.com/Erwinvandervalk/SqlStreamStore.Locking)
- [SQLStreamStore](https://github.com/SQLStreamStore)
- [Statik](https://github.com/pauldotknopf/statik)
- [Tasty](https://github.com/xenial-io/Tasty)
- [TemplatedConfiguration](https://github.com/Erwinvandervalk/TemplatedConfiguration)
- [Xenial](https://github.com/xenial-io)

Feel free to send a pull request to add your repository or organisation to this list!

---

<sub>[Target](https://thenounproject.com/term/target/345443) by [Franck Juncker](https://thenounproject.com/franckjuncker/) from [the Noun Project](https://thenounproject.com/).</sub>
