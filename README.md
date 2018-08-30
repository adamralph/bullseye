<img src="assets/bullseye.png" width="100px" />

# Bullseye

_[![NuGet version](https://img.shields.io/nuget/v/Bullseye.svg?style=flat)](https://www.nuget.org/packages/Bullseye)_
_[![Build status](https://ci.appveyor.com/api/projects/status/9qrp4gp31oy4ixh2/branch/master?svg=true)](https://ci.appveyor.com/project/adamralph/bullseye/branch/master)_

Bullseye is a [.NET package](https://www.nuget.org/packages/Bullseye) for describing and running targets and their dependencies.

Bullseye can be used to write targets that do anything. It is not coupled to building .NET projects.

Platform support: [.NET Standard 1.3 and upwards](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

## Quick start

- Install [.NET Core SDK 2.0.0](https://dot.net) or later.
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
          RunTargets(args);
      }
  }
  ```
- Back in your console:
  ```PowerShell
  dotnet run
  ```
  <img src="https://user-images.githubusercontent.com/677704/44547332-084cb300-a71b-11e8-974b-5f16975c3d75.png" width="384px" />
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
<img src="https://user-images.githubusercontent.com/677704/44547521-aa6c9b00-a71b-11e8-8d25-013f18b3453c.png" width="384px" />

## Enumerable inputs

```C#
Target(
    "eat-biscuits",
    ForEach("digestives", "chocolate hob nobs"),
    biscuits => Console.WriteLine($"Mmm...{biscuits}! Nom nom."));
```
<img src="https://user-images.githubusercontent.com/677704/44548441-64fd9d00-a71e-11e8-8585-03d20e3dbdfd.png" width="512px" />

## Sample wrapper scripts

- `build.ps1`
```PowerShell
dotnet run --project targets -- $args
```
- `build.sh`
```Shell
#!/usr/bin/env bash
set -euo pipefail
dotnet run --project targets -- "$@"
```
- `build.cmd`
```Batchfile
@echo Off
dotnet run --project targets -- %*
```

## Command line arguments

Generally, all the command line arguments passed to `Program.cs` should be passed along to Bullseye, as shown in the quick start above (`RunTargets(args);`). This is because Bullseye effectively provides a shell, with options for displaying a list of targets, performing dry runs, suppressing colour, and more. For full details of the command line options, run your targets project supplying the `--help`(/`-h`/`-?`) option:

```
./build.ps1 --help
./build.sh -h
./build.cmd -?
```

You can also handle custom arguments in `Program.cs`, but you should ensure that only valid arguments are passed along on to Bullseye.

---

<sub>[Target](https://thenounproject.com/term/target/345443) by [Franck Juncker](https://thenounproject.com/franckjuncker/) from [the Noun Project](https://thenounproject.com/).</sub>
