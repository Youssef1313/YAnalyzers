# YAnalyzers

An analyzers package implemented with the .NET Compiler platform (aka Roslyn), available via [NuGet](https://www.nuget.org/packages/YAnalyzers).

Currently contains 'Y0001' and 'Y0002', which are replacements to [IDE0007 and IDE0008](https://docs.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0007-ide0008).

For both rules, many developers (including me) don't like the heuristics they use for determining whether the type is apparent. The replacements try to provide a different implementation which adheres to [dotnet/runtime coding style](https://github.com/dotnet/runtime/blob/master/docs/coding-guidelines/coding-style.md).

Feel free to open issues suggesting other rules, or reporting bugs with existing rules.

## Rules

### Y0001: Use implicit type

This rule suggests using implicit type (`var`) when the type is apparent.

### Y0002: Use implicit type

This rule suggests using explicit type when the type is not apparent.
