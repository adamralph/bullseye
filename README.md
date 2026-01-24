# Bullseye

![Bullseye](https://raw.githubusercontent.com/adamralph/bullseye/958af18a096239e9a040ff22c5d8bf08f0fc2466/assets/bullseye.svg)

_[![NuGet version](https://img.shields.io/nuget/v/Bullseye.svg?style=flat)](https://www.nuget.org/packages/Bullseye)_

_[![CI](https://github.com/adamralph/bullseye/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/adamralph/bullseye/actions/workflows/ci.yml?query=branch%3Amain)_
_[![CodeQL analysis](https://github.com/adamralph/bullseye/actions/workflows/codeql-analysis.yml/badge.svg?branch=main)](https://github.com/adamralph/bullseye/actions/workflows/codeql-analysis.yml?query=branch%3Amain)_
_[![InferSharp](https://github.com/adamralph/bullseye/actions/workflows/infer-sharp.yml/badge.svg?branch=main)](https://github.com/adamralph/bullseye/actions/workflows/infer-sharp.yml?query=branch%3Amain)_
_[![Lint](https://github.com/adamralph/bullseye/actions/workflows/lint.yml/badge.svg?branch=main)](https://github.com/adamralph/bullseye/actions/workflows/lint.yml?query=branch%3Amain)_
_[![Spell check](https://github.com/adamralph/bullseye/actions/workflows/spell-check.yml/badge.svg?branch=main)](https://github.com/adamralph/bullseye/actions/workflows/spell-check.yml?query=branch%3Amain)_

Bullseye is a [.NET library](https://www.nuget.org/packages/Bullseye), developed by humans, for running a target dependency graph.

Bullseye is typically used as a build tool for .NET projects, and is usually used together with [SimpleExec](https://github.com/adamralph/simple-exec), but Bullseye targets can do anything. They are not restricted to building .NET projects. Bullseye is useful for any program which performs a series of operations.

Platform support: [.NET 8.0 and later](https://dot.net).

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

- Next to an existing .NET solution (`.sln`/`.slnx` file), add a .NET console app named `Targets` — `dotnet new console --name Targets`<sup>1</sup>
- Change to the new directory — `cd Targets`
- Add a reference to [Bullseye](https://www.nuget.org/packages/Bullseye) — `dotnet add package Bullseye`
- Add a reference to [SimpleExec](https://www.nuget.org/packages/SimpleExec) — `dotnet add package SimpleExec`
- Replace the contents of `Targets/Program.cs` with:
  ```c#
  using static Bullseye.Targets;
  using static SimpleExec.Command;

  Target("build", () => RunAsync("dotnet", "build --configuration Release"));
  Target("test", dependsOn: ["build"], () => RunAsync("dotnet", "test --configuration Release --no-build"));
  Target("default", dependsOn: ["test"]);

  await RunTargetsAndExitAsync(args, ex => ex is SimpleExec.ExitCodeException);
  ```
- Change to the solution directory — `cd ..`
- Run the `Targets` project — `dotnet run --project Targets`.

Voilà! You just wrote and ran your first Bullseye build program.

For help, run `dotnet run --project Targets -- --help`.

<sup>1. You could use a [file-based C# program](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/tutorials/file-based-programs) instead, but, at the time of writing, IDE support for file-based C# programs is not complete, so if you want full debugging, Intellisense, etc., it's a better to use a .NET console app.</sup>

## Sample wrapper scripts

### `build` (Linux and macOS)

```shell
#!/usr/bin/env bash
set -euo pipefail
dotnet run --project Targets -- "$@"
```

### `build.cmd` (Windows)

```batchfile
@echo Off
dotnet run --project Targets -- %*
```

## Enumerable inputs

For example, you may want to run your test projects one by one, so that the timing of each one and which one, if any, failed, is displayed in the Bullseye build summary:

```c#
Target(
    "test",
    dependsOn: ["build"],
    forEach: ["./FooTests.Acceptance", "./FooTests.Performance"],
    project => RunAsync($"dotnet", $"test {project} --configuration Release --no-build"));
```

```shell
dotnet run --project Targets -- test
```

## Command-line arguments

Generally, all the command-line arguments passed to `Program.cs` should be passed along to Bullseye, as shown in the quick start above (`RunTargetsAndExitAsync(args);`). This is because Bullseye effectively provides a command-line interface (CLI), with options for displaying a list of targets, performing dry runs, suppressing colour, and more. For full details of the command-line options, run your targets project supplying the `--help` (`-h`/`-?`) option:

```shell
dotnet run --project Targets -- --help
```

```shell
./build --help
```

```batchfile
./build.cmd --help
```

You can also handle custom arguments in `Program.cs`, but you should ensure that only valid arguments are passed along to Bullseye and that the help text contains both your custom arguments and the arguments supported by Bullseye. A good way to do this is to use a command-line parsing package to define your custom arguments, and to provide translation between the package and Bullseye. For example, see the test projects for:

- [`System.CommandLine`](BullseyeSmokeTester.CommandLine/Program.cs)
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
- [Fluxzy](https://github.com/haga-rak/fluxzy.core)
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
