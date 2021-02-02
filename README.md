# YAnalyzers

An analyzers package implemented with the .NET Compiler platform (aka Roslyn), available via [NuGet](https://www.nuget.org/packages/YAnalyzers).

Currently contains only one analyzer 'Y0001', which is a replacement to [IDE0007](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0007-ide0008).

Soon, I may also add a replacement to IDE0008.

Feel free to open issues suggesting other rules.

## Rules

### Y0001: Use implicit type

While IDE0007 provides the same analyzer, many developers (including me) don't like the heuristics it use for determining whether the type is apparent. The rule Y0001 tries to solve this.
