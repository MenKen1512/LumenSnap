param([string]$Version = '0.1.0', [switch]$SkipTests)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version must be numeric major.minor.patch' }
Push-Location (Join-Path $PSScriptRoot '..')
try {
    & ./build/Make-Icon.ps1
    $source = (& git rev-parse HEAD).Trim()
    if ($LASTEXITCODE -ne 0) { throw 'Git source SHA unavailable' }
    $publish = Join-Path (Get-Location) "artifacts/publish-$Version"
    $dist = Join-Path (Get-Location) 'artifacts/dist'
    New-Item -ItemType Directory -Force $publish,$dist | Out-Null
    if (-not $SkipTests) {
        & dotnet run --project tests/LumenSnap.Tests -c Release "-p:SourceRevisionId=$source" 2>&1 | Tee-Object artifacts/unit-tests.txt
        if ($LASTEXITCODE -ne 0) { throw 'Unit tests failed' }
    }
    & dotnet publish src/LumenSnap -c Release -r win-x64 --self-contained true -o $publish "-p:Version=$Version" "-p:SourceRevisionId=$source" -p:PublishSingleFile=false -p:PublishTrimmed=false
    if ($LASTEXITCODE -ne 0) { throw 'Publish failed' }
    Copy-Item LICENSE,THIRD_PARTY.md -Destination $publish
    Copy-Item docs/USER_GUIDE_RU.md -Destination $publish
    $licenses = Join-Path $publish 'licenses'
    New-Item -ItemType Directory -Force $licenses | Out-Null
    foreach ($package in @('microsoft.netcore.app.runtime.win-x64','microsoft.windowsdesktop.app.runtime.win-x64')) {
        $packagePath = Join-Path $env:USERPROFILE ".nuget/packages/$package"
        if (Test-Path $packagePath) {
            Get-ChildItem $packagePath -Recurse -File | Where-Object { $_.Name -match '^(LICENSE|THIRD-PARTY-NOTICES)' } | ForEach-Object {
                Copy-Item $_.FullName (Join-Path $licenses "$package-$($_.Directory.Name)-$($_.Name)")
            }
        }
    }
    $compiler = Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6/ISCC.exe'
    if (-not (Test-Path $compiler)) { throw 'Inno Setup 6 compiler is required on an authorized build machine' }
    & $compiler "/DAppVersion=$Version" "/DPublishDir=$publish" "/DOutputDir=$dist" installer/LumenSnap.iss
    if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed' }
    Compress-Archive -Path "$publish/*" -DestinationPath "$dist/LumenSnap-Portable-$Version-win-x64.zip" -Force
    $manifest = [ordered]@{ source=$source; version=$Version; os=[System.Runtime.InteropServices.RuntimeInformation]::OSDescription; sdk=(& dotnet --version); inno=(Get-Item $compiler).VersionInfo.FileVersion; unsigned=$true; runtime=(Get-Content "$publish/LumenSnap.runtimeconfig.json" -Raw | ConvertFrom-Json) }
    $manifest | ConvertTo-Json -Depth 10 | Set-Content "$dist/build-$Version.json" -Encoding utf8
    Get-ChildItem "$dist/*-$Version-win-x64.*" | Get-FileHash -Algorithm SHA256 | ForEach-Object { "$($_.Hash.ToLowerInvariant())  $(Split-Path $_.Path -Leaf)" } | Set-Content "$dist/SHA256SUMS-$Version.txt" -Encoding utf8
} finally { Pop-Location }
