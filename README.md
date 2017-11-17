<img src="assets/bullseye.png" width="100px" />

# Bullseye

[![NuGet version](https://img.shields.io/nuget/v/Bullseye.svg?style=flat)](https://www.nuget.org/packages/Bullseye) [![Build status](https://ci.appveyor.com/api/projects/status/9qrp4gp31oy4ixh2/branch/master?svg=true)](https://ci.appveyor.com/project/adamralph/bullseye/branch/master)

Bullseye is a .NET package for describing and running targets and their dependencies.

Bullseye can be used to write targets that do anything. It is not coupled to building .NET projects.

Platform support: [.NET Standard 1.3 and upwards](https://docs.microsoft.com/en-us/dotnet/standard/net-standard).

## Quick start

```PowerShell
Install-Package Bullseye
```

```C#
using static Bullseye.Targets;

namespace MyTargets
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Add("default", () => Console.Out.WriteLineAsync("Hello, world!"));
            return await RunAsync(args);
        }
    }
}
```

For help, pass `"--help"` as an argument.

---

<sub>[Target](https://thenounproject.com/term/target/345443) by [Franck Juncker](https://thenounproject.com/franckjuncker/) from [the Noun Project](https://thenounproject.com/).</sub>
