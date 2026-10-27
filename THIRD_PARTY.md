# Dependencies and assets

No third-party NuGet package references, network SDK, browser, telemetry or
commercial assets. Original icon: build/Make-Icon.ps1. UI uses original geometry
and system font symbols. No font files or Lightshot artifacts are redistributed.

Self-contained packages include Microsoft .NET / Windows Desktop runtime code
under its applicable MIT and third-party licenses. Build.ps1 copies license/notice
files from runtime packages to licenses/. Primary upstream notices:

- https://github.com/dotnet/runtime/blob/main/LICENSE.TXT
- https://github.com/dotnet/runtime/blob/main/THIRD-PARTY-NOTICES.TXT
- https://github.com/dotnet/wpf/blob/main/LICENSE.TXT
- https://github.com/dotnet/wpf/blob/main/THIRD-PARTY-NOTICES.TXT
- https://github.com/dotnet/winforms/blob/main/LICENSE.TXT

Build tools: .NET SDK, PowerShell, Git, Inno Setup 6 (not app dependencies).
Inno Setup license: https://jrsoftware.org/files/is/license.txt . Hosted runner
supplies the compiler; no installation on MENKEN-PC is part of development.
Project code license remains an explicit repository-owner decision.
