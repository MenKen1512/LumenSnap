# Build, test and package

Use a separately authorized Windows x64 developer/test machine. Do not run these
steps on MENKEN-PC without separate authorization. Dependencies: Git, PowerShell 7,
.NET SDK 10.x (global.json selects 10.0.100 or later feature band), Inno Setup 6
at its conventional Program Files (x86) path. The GitHub windows-2025 image supplies
Inno; the workflow sets up .NET in its disposable runner. No tools are installed
on the user's workstation by this workflow.

```powershell
./build/Build.ps1 -Version 0.1.0
```

This generates the original application icon, runs the dependency-free STA test
executable, publishes all .NET desktop runtime dependencies and compiles Setup.
Output: artifacts/dist, including Setup EXE, portable ZIP, SHA256SUMS and build
manifest (exact source SHA, OS, SDK, Inno and runtime metadata). Unsigned.
Builds are traceable and script-reproducible; byte-identical installer/ZIP output
is not claimed (compiler/package timestamps). Build dependencies require network
on first restore; installation and application operation require none.

For actual install/upgrade/uninstall validation in a disposable authorized VM:

```powershell
./build/Build.ps1 -Version 0.0.9 -SkipTests
./build/Build.ps1 -Version 0.1.0
$env:LUMENSNAP_AUTHORIZED_TEST_VM = '1'
./build/Test-Installer.ps1
```

The 0.0.9 fixture is built from the same source with an older real executable and
installer version; it tests upgrade mechanics, not compatibility with a historical
released application. The script installs/runs only in the explicitly selected
disposable environment. Evidence goes to artifacts/installer-evidence. Self-tests
produce JSON and actual WPF PNGs; they are not substituted for human input tests.
The fixture diagnostics do not read personal screenshots or installed Lightshot.

No stable release until all T-01..T-25 acceptance requirements are evidenced on
Windows 11 x64. Hosted Windows Server smoke is reported with its real OS name.
To investigate failures, inspect unit-tests.txt, smoke.json and Inno install logs.
