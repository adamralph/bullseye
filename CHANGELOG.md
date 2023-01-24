# Changelog

## 4.2.1

### Fixed bugs

- [#841: Host is not auto-detected](https://github.com/adamralph/bullseye/issues/841)

## 4.2.0

### Enhancements

- [#800: make Palette and HostExtensions public](https://github.com/adamralph/bullseye/pull/800)

## 4.1.1

### Fixed bugs

- [#820: Cannot run a target with a name that another target name starts with](https://github.com/adamralph/bullseye/issues/820)

## 4.1.0

### Enhancements

- [#809: Target name abbreviation](https://github.com/adamralph/bullseye/pull/809)

### Fixed bugs

- [#749: Help text remarks are not coloured correctly](https://github.com/adamralph/bullseye/issues/749)

## 4.0.0

### Enhancements

- [#532: **[BREAKING]** Expand help option formats](https://github.com/adamralph/bullseye/issues/532)
- [#640: Group target log lines in GitHub Actions](https://github.com/adamralph/bullseye/issues/640)
- [#641: Parse-able standard output](https://github.com/adamralph/bullseye/issues/641)
- [#642: **[BREAKING]** Better options API](https://github.com/adamralph/bullseye/issues/642)
- [#663: **[BREAKING]** remove sync over async](https://github.com/adamralph/bullseye/pull/663)
- [#673: **[BREAKING]** allow forcing of console (default) mode](https://github.com/adamralph/bullseye/pull/673)
- [#675: accept IOptions instead of Options](https://github.com/adamralph/bullseye/pull/675)
- [#682: accept custom output and diagnostic writers](https://github.com/adamralph/bullseye/pull/682)
- [#703: **[BREAKING]** accept a function instead of a string for the message prefix](https://github.com/adamralph/bullseye/pull/703)
- [#704: **[BREAKING]** Nullable annotations](https://github.com/adamralph/bullseye/issues/704)
- [#706: remove top level output messages for targets with inputs](https://github.com/adamralph/bullseye/pull/706)

### Fixed bugs

- [#674: **[BREAKING]** fix AppVeyor casing](https://github.com/adamralph/bullseye/pull/674)

### Other

- [#687: **[BREAKING]** switch from netstandard2.0 to netstandard2.1](https://github.com/adamralph/bullseye/pull/687)
- [#710: **[BREAKING]** remove Azure Pipelines mode](https://github.com/adamralph/bullseye/pull/710)

## 3.8.0

### Enhancements

- [#597: add README.md to package](https://github.com/adamralph/bullseye/pull/597)
- [#622: Avoid walking the dependencies of a target more than once](https://github.com/adamralph/bullseye/issues/622)
- [#636: avoid redundantly awaiting targets](https://github.com/adamralph/bullseye/pull/636)

### Fixed bugs

- [#594: source stepping doesn't work](https://github.com/adamralph/bullseye/pull/594)
- [#620: Verbose log shows incorrect targets when a dependency doesn't exist](https://github.com/adamralph/bullseye/issues/620)
- [#626: Targets may be run more than once when running in parallel](https://github.com/adamralph/bullseye/issues/626)
- [#632: Dependency paths get jumbled in verbose logs when running in parallel](https://github.com/adamralph/bullseye/issues/632)

## 3.7.1

### Fixed bugs

- [#611: Static RunTargetsWithoutExiting overload with Options returns before running targets](https://github.com/adamralph/bullseye/issues/611)

## 3.7.0

### Enhancements

- [#525: Add description for each target](https://github.com/adamralph/bullseye/issues/525)

### Fixed bugs

- [#536: Options.NoColor XML doc says "input" instead of "output"](https://github.com/adamralph/bullseye/pull/536)

## 3.6.0

### Enhancements

- [#521: Support NO\_COLOR — https://no-color.org/](https://github.com/adamralph/bullseye/issues/521)
- [#529: Improve help](https://github.com/adamralph/bullseye/issues/529)

### Fixed bugs

- [#482: running targets without exiting leaves the console screen buffer output mode changed](https://github.com/adamralph/bullseye/pull/482)
- [#528: Help shows invalid options in examples](https://github.com/adamralph/bullseye/issues/528)

## 3.5.0

### Enhancements

- [#456: tighten timings to measure execution of target actions only](https://github.com/adamralph/bullseye/pull/456)
- [#457: align per-target succeeded text with summary by removing full stop](https://github.com/adamralph/bullseye/pull/457)
- [#459: align timings shown during execution with those in summary](https://github.com/adamralph/bullseye/pull/459)

### Fixed bugs

- [#460: timing rounding is sometimes incorrect](https://github.com/adamralph/bullseye/pull/460)

## 3.4.0

### Enhancements

- [#421: sort results in summary by start/completion order](https://github.com/adamralph/bullseye/pull/421)

### Fixed bugs

- [#427: Option.Host is mutated when set to Unknown, and host is detected](https://github.com/adamralph/bullseye/pull/427)

## 3.3.0

### Enhancements

- [#396: Make args parsing public](https://github.com/adamralph/bullseye/issues/396)

### Fixed bugs

- [#405: extra line break after listing targets](https://github.com/adamralph/bullseye/pull/405)

## 3.2.0

### Enhancements

- [#361: API for integration with third party CLI libs](https://github.com/adamralph/bullseye/pull/361)
- [#388: upgrade to SourceLink 1.0.0](https://github.com/adamralph/bullseye/pull/388)
- [#393: add option for no extended chars](https://github.com/adamralph/bullseye/pull/393)

## 3.1.0

### Enhancements

- [#348: Less verbose output when a specified target, or the dependency graph, is invalid](https://github.com/adamralph/bullseye/issues/348)
- [#358: Non-static API](https://github.com/adamralph/bullseye/pull/358)
- [#367: Make exceptions public](https://github.com/adamralph/bullseye/issues/367)

## 3.0.0

### Enhancements

- [#176: **[BREAKING]** Log diagnostics to standard error (stderr)](https://github.com/adamralph/bullseye/issues/176)
- [#225: **[BREAKING]** Consistent minute and second indicators](https://github.com/adamralph/bullseye/issues/225)
- [#230: **[BREAKING]** Case-insensitive target names](https://github.com/adamralph/bullseye/issues/230)
- [#252: Show a list of targets in help output](https://github.com/adamralph/bullseye/issues/252)
- [#260: **[BREAKING]** Type short options for lists without using the Shift key](https://github.com/adamralph/bullseye/issues/260)
- [#273: Update SourceLink to 1.0.0-beta2-19367-01](https://github.com/adamralph/bullseye/issues/273)
- [#280: **[BREAKING]** Change the log prefix to be the entry assembly name](https://github.com/adamralph/bullseye/issues/280)
- [#289: Warn instead of crashing when console cannot be cleared](https://github.com/adamralph/bullseye/issues/289)
- [#292: Optimised colour palette for GitHub Actions](https://github.com/adamralph/bullseye/issues/292)
- [#301: Add XML documentation file to package 🤦‍♂](https://github.com/adamralph/bullseye/issues/301)
- [#311: **[BREAKING]** Use a consistent log prefix](https://github.com/adamralph/bullseye/issues/311)
- [#313: Use consistent colour for target names in lists](https://github.com/adamralph/bullseye/issues/313)
- [#316: Better colour for target names in log and lists](https://github.com/adamralph/bullseye/issues/316)
- [#317: Optimised colour palette for GitLab CI](https://github.com/adamralph/bullseye/issues/317)
- [#318: Optimised colour palette for VS Code terminal](https://github.com/adamralph/bullseye/issues/318)

### Fixed bugs

- [#315: Log colouring bleeds into whitespace](https://github.com/adamralph/bullseye/issues/315)
- [#319: Symbols are garbled on TeamCity](https://github.com/adamralph/bullseye/issues/319)

### Other

- [#239: **[BREAKING]** Remove RunTargets* methods](https://github.com/adamralph/bullseye/issues/239)
- [#300: **[BREAKING]** replace Run* overloads with optional params](https://github.com/adamralph/bullseye/pull/300)

## 2.4.0

### Enhancements

- [#168: Summary reporting](https://github.com/adamralph/bullseye/issues/168)
- [#199: Optimize colour palette for Azure Pipelines](https://github.com/adamralph/bullseye/pull/199)
- [#238: Add RunTargetsWithoutExiting* methods](https://github.com/adamralph/bullseye/issues/238)

## 2.3.0

### Enhancements

- [#128: Option to show dependency tree](https://github.com/adamralph/bullseye/issues/128)
- [#147: Indicate missing dependencies when listing them](https://github.com/adamralph/bullseye/issues/147)
- [#162: Log exceptions inside targets](https://github.com/adamralph/bullseye/issues/162)
- [#166: simplify exception messages](https://github.com/adamralph/bullseye/pull/166)
- [#172: RunTargetsAndExit() and RunTargetsAndExitAsync()](https://github.com/adamralph/bullseye/issues/172)
- [#187: don't log exception message when already logged by an input invocation](https://github.com/adamralph/bullseye/pull/187)

### Fixed bugs

- [#165: fix pluralisation in unknown option(s) message](https://github.com/adamralph/bullseye/pull/165)

## 2.2.0

### Enhancements

- [#127: Options to force CI server mode](https://github.com/adamralph/bullseye/issues/127)
- [#134: Better help line in help](https://github.com/adamralph/bullseye/issues/134)
- [#136: Enriched verbose output](https://github.com/adamralph/bullseye/issues/136)

## 2.1.0

### Enhancements

- [#81: Switch palettes for CI environments](https://github.com/adamralph/bullseye/issues/81)
- [#113: align colours](https://github.com/adamralph/bullseye/pull/113)

## 2.0.0

### Enhancements

- [#69: Parallel targets](https://github.com/adamralph/bullseye/issues/69)
- [#98: align language used in verbose output](https://github.com/adamralph/bullseye/pull/98)
- [#101: **[BREAKING]** Fail when circular deps are detected](https://github.com/adamralph/bullseye/pull/101)
- [#106: log inputs when a target has no action](https://github.com/adamralph/bullseye/pull/106)
- [#107: better logging when a target has no inputs or no action](https://github.com/adamralph/bullseye/pull/107)
- [#112: Use dependencies to determine target execution order, even when skipping dependencies](https://github.com/adamralph/bullseye/issues/112)

### Other

- [#78: **[BREAKING]** Remove Add(), Run(), and RunAsync()](https://github.com/adamralph/bullseye/issues/78)
- [#105: **[BREAKING]** remove redundant Target(string name) overload](https://github.com/adamralph/bullseye/pull/105)

## 1.3.0

### Enhancements

- [#75: Option to display input values when listing targets](https://github.com/adamralph/bullseye/issues/75)
- [#87: Make target names bright white in lists and help](https://github.com/adamralph/bullseye/pull/87)
- [#88: sort options in help](https://github.com/adamralph/bullseye/pull/88)
- [#89: add help to help](https://github.com/adamralph/bullseye/pull/89)
- [#92: avoid using background colour for output](https://github.com/adamralph/bullseye/pull/92)

## 1.2.0

### Enhancements

- [#52: Source stepping](https://github.com/adamralph/bullseye/issues/52)
- [#73: Attempt to automatically enable console colors in Windows](https://github.com/adamralph/bullseye/issues/73)

## 1.1.0

### Enhancements

- [#34: New `Target()` and `RunTargets()` API](https://github.com/adamralph/bullseye/issues/34)
- [#36: Targets with enumerable inputs](https://github.com/adamralph/bullseye/pull/36)
- [#49: Better log messages](https://github.com/adamralph/bullseye/issues/49)
- [#50: Option to clear the console](https://github.com/adamralph/bullseye/issues/50)

### Fixed bugs

- [#59: Some internal awaits are missing .ConfigureAwait(false)](https://github.com/adamralph/bullseye/issues/59)

## 1.0.1

### Fixed bugs

- [#45: Unknown options are not reported properly](https://github.com/adamralph/bullseye/issues/45)

## 1.0.0

### Enhancements

- [#2: Initial version](https://github.com/adamralph/bullseye/issues/2)
