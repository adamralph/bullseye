<img src="assets/bullseye.png" width="100px" />

# Bullseye

_[![NuGet version](https://img.shields.io/nuget/v/Bullseye.svg?style=flat)](https://www.nuget.org/packages/Bullseye)_

_[![Build status](https://github.com/adamralph/bullseye/workflows/.github/workflows/ci.yml/badge.svg)](https://github.com/adamralph/bullseye/actions?query=workflow%3A.github%2Fworkflows%2Fci.yml)_
_[![CodeQL analysis](https://github.com/adamralph/bullseye/workflows/.github/workflows/codeql-analysis.yml/badge.svg)](https://github.com/adamralph/bullseye/actions?query=workflow%3A.github%2Fworkflows%2Fcodeql-analysis.yml)_
_[![Spell check](https://github.com/adamralph/bullseye/workflows/.github/workflows/spell-check.yml/badge.svg)](https://github.com/adamralph/bullseye/actions?query=workflow%3A.github%2Fworkflows%2Fspell-check.yml)_

_[![Appveyor smoke test status](https://img.shields.io/appveyor/ci/adamralph/bullseye/master.svg?logo=appveyor&label=AppVeyor)](https://ci.appveyor.com/project/adamralph/bullseye/branch/master)_
_[![Azure DevOps smoke test status](https://img.shields.io/azure-devops/build/adamralph/9b2238c8-fcb0-4618-a3ef-0ecab48ea345/1/master.svg?logo=azuredevops&label=Azure%20DevOps)](https://adamralph.visualstudio.com/bullseye/_build/latest?definitionId=1&branchName=master)_
_[![CircleCI smoke test status](https://img.shields.io/circleci/build/github/adamralph/bullseye/master?logo=circleci&label=CircleCI)](https://circleci.com/gh/adamralph/bullseye/tree/master)_
_[![Cirrus CI smoke test status](https://img.shields.io/cirrus/github/adamralph/bullseye/master?logo=cirrus-ci&label=Cirrus%20CI)](https://cirrus-ci.com/github/adamralph/bullseye)_
_[![GitLab CI/CD smoke test status](https://img.shields.io/gitlab/pipeline/adamralph/bullseye/master.svg?logo=gitlab&label=GitLab+CI%2fCD)](https://gitlab.com/adamralph/bullseye/-/jobs)_
_[![Travis CI smoke test status](https://img.shields.io/travis/adamralph/bullseye/master.svg?logo=travis&label=Travis%20CI)](https://travis-ci.org/adamralph/bullseye/branches)_

Bullseye is a [.NET library](https://www.nuget.org/packages/Bullseye) that runs a target dependency graph.

Bullseye targets can do anything. They are not restricted to building .NET projects.

Platform support: [.NET Standard 2.0 and upwards](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

<!-- spell-checker:disable -->
- [Bullseye](#bullseye)
  - [Quick start](#quick-start)
  - [Defining dependencies](#defining-dependencies)
  - [Enumerable inputs](#enumerable-inputs)
  - [Sample wrapper scripts](#sample-wrapper-scripts)
  - [Command line arguments](#command-line-arguments)
  - [Non-static API](#non-static-api)
  - [FAQ](#faq)
    - [Can I force a pause before exiting when debugging in Visual Studio 2017 (or earlier)?](#can-i-force-a-pause-before-exiting-when-debugging-in-visual-studio-2017-or-earlier)
  - [Who's using Bullseye?](#whos-using-bullseye)
<!-- spell-checker:enable -->

## Quick start

- Create a .NET console app named `targets` and add a reference to [Bullseye](https://www.nuget.org/packages/Bullseye).
- Replace the contents of `Program.cs` with:

  ```C#
  using static Bullseye.Targets;

  class Program
  {
      static void Main(string[] args)
      {
          Target("default", () => System.Console.WriteLine("Hello, world!"));
          RunTargetsAndExit(args);
      }
  }
  ```

- Run the app. E.g. `dotnet run` or F5 in Visual Studio:

Voilà! You've just written and run your first Bullseye program. You will see output similar to:

<img src="https://user-images.githubusercontent.com/677704/93706096-506d7800-fb23-11ea-8154-d8c8c90bada5.png" width="314px" />

For help, run `dotnet run -- --help`.

Also see the [async quick start](https://github.com/adamralph/bullseye/wiki/Async-quick-start).

## Defining dependencies

```C#
Target("make-tea", () => Console.WriteLine("Tea made."));
Target("drink-tea", DependsOn("make-tea"), () => Console.WriteLine("Ahh... lovely!"));
Target("walk-dog", () => Console.WriteLine("Walkies!"));
Target("default", DependsOn("drink-tea", "walk-dog"));
```

<img src="https://user-images.githubusercontent.com/677704/93706154-c96ccf80-fb23-11ea-926a-9e3836e79f06.png" width="325px" />

## Enumerable inputs

```C#
Target(
    "eat-biscuits",
    ForEach("digestives", "chocolate hobnobs"),
    biscuits => Console.WriteLine($"Mmm...{biscuits}! Nom nom."));
```

```PowerShell
dotnet run -- eat-biscuits
```

<img src="https://user-images.githubusercontent.com/677704/95656205-f7cf4080-0b0c-11eb-9f82-a4fb706ae33b.png" width="444px" />

## Sample wrapper scripts

- `build.cmd`

  ```Batchfile
  @echo Off
  dotnet run --project targets -- %*
  ```

- `build.sh`

  ```Shell
  #!/usr/bin/env bash
  set -euo pipefail
  dotnet run --project targets -- "$@"
  ```

- `build.ps1`

  ```PowerShell
  $ErrorActionPreference = "Stop";
  dotnet run --project targets -- $args
  ```

## Command line arguments

Generally, all the command line arguments passed to `Program.cs` should be passed along to Bullseye, as shown in the quick start above (`RunTargetsAndExit(args);`). This is because Bullseye effectively provides a command line interface, with options for displaying a list of targets, performing dry runs, suppressing colour, and more. For full details of the command line options, run your targets project supplying the `--help` (`-h`/`-?`) option:

```PowerShell
dotnet run --project targets -- --help
./build.cmd --help
./build.sh -h
./build.ps1 -?
```

You can also handle custom arguments in `Program.cs`, but you should ensure that only valid arguments are passed along to Bullseye and that the help text contains both your custom arguments and the arguments supported by Bullseye. A good way to do this is to use a command line parsing package to define your custom arguments, and to provide translation between the package and Bullseye. For example, see the test projects for:

- [System.CommandLine](BullseyeSmokeTester.CommandLine/Program.cs)
- [System.CommandLine.DragonFruit](BullseyeSmokeTester.DragonFruit/Program.cs)
- [McMaster.Extensions.CommandLineUtils](BullseyeSmokeTester.McMaster/Program.cs)

## Non-static API

For most cases, the static API described above is sufficient. For more complex scenarios where a number of target collections are required, the non-static API may be used.

```C#
var targets1 = new Targets();
targets1.Add("foo", () => Console.Out.WriteLine("foo1"));

var targets2 = new Targets();
targets2.Add("foo", () => Console.Out.WriteLine("foo2"));

targets1.RunWithoutExiting(args);
targets2.RunWithoutExiting(args);
```

## NO_COLOR

Bullseye supports [NO_COLOR](https://no-color.org/).

## FAQ

### Can I force a pause before exiting when debugging in Visual Studio 2017 (or earlier)?

Yes! Add the following line anywhere before calling `RunTargetsAndExit`/`RunTargetsAndExitAsync`:

```c#
AppDomain.CurrentDomain.ProcessExit += (s, e) => Console.ReadKey();
```

Note that the common way to do this for .NET console apps is to add a line such as the following before the end of the `Program.Main` method:

```c#
Console.ReadKey();
```

This does not work after calling `RunTargetsAndExit`/`RunTargetsAndExit` because that is the final statement that will be executed.

In Visual Studio 2019 and later, .NET console apps pause before exiting by default, so none of this is required.

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
- [LiteGuard](https://github.com/adamralph/liteguard)
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
- [TemplatedConfiguration](https://github.com/Erwinvandervalk/TemplatedConfiguration)
- [xBehave.net](https://github.com/xbehave)

Feel free to send a pull request to add your repo or organisation to this list!

---

<sub>[Target](https://thenounproject.com/term/target/345443) by [Franck Juncker](https://thenounproject.com/franckjuncker/) from [the Noun Project](https://thenounproject.com/).</sub>
