
# Windows Analyzers

This is an example/sample/test experimental repository containing some initial ideas and examples of possible Roslyn based C# analyzers to aid Windows developers using WinUI 3 and the Windows App SDK.

Be sure to upvote and follow the tracking issue here: https://github.com/microsoft/WindowsAppSDK/discussions/4562

If you have an idea for an analyzer, please open up an issue. Be sure to include a minimal snippet of the code to detect and potential fixed code afterwards.

## How this project was created

This project was started to investigate the feasibility and complexity around creating sets of analyzers around common pitfalls in Windows development. DependencyProperty nuances and documented but unenforced conventions were the motivation behind this investigation.

It was created using the `Analyzer with code fix (.NET Standard)` Visual Studio template as [outlined in this tutorial](https://learn.microsoft.com/dotnet/csharp/roslyn-sdk/tutorials/how-to-write-csharp-analyzer-code-fix). The project was updated to the latest dependencies for Roslyn, switched to use [file-scoped namespaces](https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/namespace), and the Unit Test project was swapped to use the new [WinUI based template](https://devblogs.microsoft.com/visualstudio/dive-into-native-windows-development-with-new-winui-workload-and-template-improvements/) so Analyzers/Code Fixes could be tested for their Windows development target.

https://github.com/user-attachments/assets/d082548e-b75e-471e-a1e7-e680d81adfc8

## Current Example Analyzers

| DiagnosticID | Description                                                                           |
|--------------|---------------------------------------------------------------------------------------|
| WASDKWUI0001 | DependencyProperty - Use nameof for first argument                                    |
| WASDKWUI0002 | DependencyProperty - Third argument should match containing class                     |
| WASDKWUI0003 | DependencyProperty - Identifer should end with 'Property'                             |
| WASDKWUI0004 | DependencyProperty - Property should match name without 'Property' suffix             |

## Ideas of other Analyzers

| DiagnosticID | Description                                                                           | Approach / Notes                                                   |                              
|--------------|---------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| WASDKWUI0005 | DependencyProperty - GetValue/SetValue DP reference match expected property           | Can analyze the argument, then codefix would be to align to containing property name?        |
| WASDKAPI0001 | NativeMethods - Use C#/Win32 to generate PInvoke calls                                | Detect DllImport attribute to user32.dll, need example cases? Probably just info vs. codefix |
