<img src="assets/bullseye.png" width="100px" />

# Bullseye

_[![NuGet version](https://img.shields.io/nuget/v/Bullseye.svg?style=flat)](https://www.nuget.org/packages/Bullseye)_
_[![Build status](https://ci.appveyor.com/api/projects/status/9qrp4gp31oy4ixh2/branch/master?svg=true)](https://ci.appveyor.com/project/adamralph/bullseye/branch/master)_

Bullseye is a [.NET package](https://www.nuget.org/packages/Bullseye) for describing and running targets and their dependencies.

Bullseye can be used to write targets that do anything. It is not coupled to building .NET projects.

Platform support: [.NET Standard 1.3 and upwards](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

## Quick start

- Install [.NET Core SDK 2.0.0](https://dot.net/core) or later.
- In a console:
  ```PowerShell
  mkdir targets
  cd targets
  dotnet new console
  dotnet add package Bullseye
  ```
- Replace `Program.cs`:
  ```C#
  using static Bullseye.Targets;

  class Program
  {
      static void Main(string[] args)
      {
          Add("default", () => System.Console.WriteLine("Hello, world!"));
          Run(args);
      }
  }
  ```
- In a console:
  ```PowerShell
  dotnet run
  ```
  <img src="https://raw.githubusercontent.com/adamralph/assets/10ad80273809e587eaa732e552fbf59ddc087cea/bullseye-hello-world-output.png" width="384px" />
- For help:
  ```PowerShell
  dotnet run -- --help
  ```

Also see the [async quick start](https://github.com/adamralph/bullseye/wiki/Async-quick-start).

## Defining dependencies

```C#
Add("default", DependsOn("drink-tea", "walk-dog"));
Add("make-tea", () => Console.WriteLine("Tea made."));
Add("drink-tea", DependsOn("make-tea"), () => Console.WriteLine("Ahh... lovely!"));
Add("walk-dog", () => Console.WriteLine("Walkies!"));
```
<img src="https://raw.githubusercontent.com/adamralph/assets/10ad80273809e587eaa732e552fbf59ddc087cea/bullseye-dependencies-output.png" width="384px" />

---

<sub>[Target](https://thenounproject.com/term/target/345443) by [Franck Juncker](https://thenounproject.com/franckjuncker/) from [the Noun Project](https://thenounproject.com/).</sub>
