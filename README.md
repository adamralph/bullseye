<img src="assets/bullseye.png" width="100px" />

# Bullseye

_[![NuGet version](https://img.shields.io/nuget/v/Bullseye.svg?style=flat)](https://www.nuget.org/packages/Bullseye)_
_[![Build status](https://ci.appveyor.com/api/projects/status/9qrp4gp31oy4ixh2/branch/master?svg=true)](https://ci.appveyor.com/project/adamralph/bullseye/branch/master)_
_[![Build Status](https://travis-ci.org/adamralph/bullseye.svg?branch=master)](https://travis-ci.org/adamralph/bullseye)_

Bullseye is a [.NET package](https://www.nuget.org/packages/Bullseye) for describing and running targets and their dependencies.

Bullseye can be used to write targets that do anything. It is not coupled to building .NET projects.

Platform support: [.NET Standard 2.0 and upwards](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

- [Quick start](#quick-start)
- [Defining dependencies](#defining-dependencies)
- [Enumerable inputs](#enumerable-inputs)
- [Sample wrapper scripts](#sample-wrapper-scripts)
- [Command line arguments](#command-line-arguments)
- [Who's using Bullseye?](#whos-using-bullseye)

## Quick start

- Install the [.NET Core SDK](https://dot.net).
- In a console:
  ```PowerShell
  mkdir targets
  cd targets
  dotnet new console
  dotnet add package Bullseye
  ```
- Using your favourite text editor or IDE, replace the contents of `Program.cs` with:
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
- Back in your console:
  ```PowerShell
  dotnet run
  ```
  <img src="https://user-images.githubusercontent.com/677704/46696376-4beb7580-cc12-11e8-9e79-cad6f49e05d7.png" width="341px" />
- For help:
  ```PowerShell
  dotnet run -- --help
  ```

Also see the [async quick start](https://github.com/adamralph/bullseye/wiki/Async-quick-start).

## Defining dependencies

```C#
Target("default", DependsOn("drink-tea", "walk-dog"));
Target("make-tea", () => Console.WriteLine("Tea made."));
Target("drink-tea", DependsOn("make-tea"), () => Console.WriteLine("Ahh... lovely!"));
Target("walk-dog", () => Console.WriteLine("Walkies!"));
```

<img src="https://user-images.githubusercontent.com/677704/46696573-c61bfa00-cc12-11e8-834a-e0dd4a5d8831.png" width="342px" />

## Enumerable inputs

```C#
Target(
    "eat-biscuits",
    ForEach("digestives", "chocolate hob nobs"),
    biscuits => Console.WriteLine($"Mmm...{biscuits}! Nom nom."));
```

<img src="https://user-images.githubusercontent.com/677704/46696786-522e2180-cc13-11e8-8d91-bb31f80dcac8.png" width="511px" />

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

You can also handle custom arguments in `Program.cs`, but you should ensure that only valid arguments are passed along to Bullseye. A good way to do this is to use [McMaster.Extensions.CommandLineUtils](https://www.nuget.org/packages/McMaster.Extensions.CommandLineUtils/) to parse your custom arguments, and pass the [remaining arguments](https://natemcmaster.github.io/CommandLineUtils/docs/arguments.html?tabs=using-attributes#remaining-arguments) to Bullseye. See this [gist](https://gist.github.com/adamralph/d6a3167c8fe0d4e24721d8d2b9c02989) as an example.

## Who's using Bullseye?

To name a few:

- [ConfigR](https://github.com/config-r)
- [FakeItEasy](https://github.com/FakeItEasy)
- [Ibento](https://github.com/pgermishuys/Ibento)
- [LiteGuard](https://github.com/adamralph/liteguard)
- [LittleForker](https://github.com/damianh/LittleForker)
- [Marten](https://github.com/JasperFx/marten)
- [MinVer](https://github.com/adamralph/minver)
- [Particular](https://github.com/Particular)
- [Radical Framework](https://github.com/RadicalFx)
- [SelfInitializingFakes](https://github.com/blairconrad/SelfInitializingFakes)
- [SendComics](https://github.com/blairconrad/SendComics)
- [SQLStreamStore](https://github.com/SQLStreamStore)
- [Statik](https://github.com/pauldotknopf/statik)
- [xBehave.net](https://github.com/xbehave)

Feel free to send a pull request to add your repo or organisation to this list!

---

<sub>[Target](https://thenounproject.com/term/target/345443) by [Franck Juncker](https://thenounproject.com/franckjuncker/) from [the Noun Project](https://thenounproject.com/).</sub>
