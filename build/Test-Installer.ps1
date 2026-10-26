param([string]$Version = '0.1.0', [string]$PreviousVersion = '0.0.9')
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if (-not $env:GITHUB_ACTIONS -and $env:LUMENSNAP_AUTHORIZED_TEST_VM -ne '1') { throw 'Run only in GitHub Actions or an explicitly authorized disposable Windows VM' }
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$out = Join-Path $repo 'artifacts/installer-evidence'
New-Item -ItemType Directory -Force $out | Out-Null
$installed = Join-Path $env:LOCALAPPDATA 'Programs/LumenSnap'
$settingsDir = Join-Path $env:LOCALAPPDATA 'LumenSnap'
$exe = Join-Path $installed 'LumenSnap.exe'
function Run-Checked([string]$file, [string[]]$arguments, [string]$name) {
    Write-Host "Checking: $name"
    # Inno's uninstaller delegates work to a temporary child. -Wait waits for the
    # process tree; Process.WaitForExit alone can race a subsequent reinstall.
    $p = Start-Process -FilePath $file -ArgumentList $arguments -Wait -PassThru -WindowStyle Hidden
    if ($p.ExitCode -ne 0) { throw "$name failed: $($p.ExitCode)" }
}
function Smoke-Installed([string]$label) {
    $path = Join-Path $out $label
    Run-Checked $exe @('--smoke',"`"$path`"") $label
    if (-not (Test-Path (Join-Path $path 'smoke.json'))) { throw 'Smoke did not produce evidence' }
}
$entry = 'HKCU:/Software/Microsoft/Windows/CurrentVersion/Uninstall/{E7201EB6-CE0A-4C13-9736-4E7C70B29A0C}_is1'
Run-Checked "$repo/artifacts/dist/LumenSnap-Setup-$PreviousVersion-win-x64.exe" @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LOG=`"$out/install-previous.log`"") 'previous install'
if ((Get-Item $exe).VersionInfo.ProductVersion -notlike "$PreviousVersion*") { throw 'Previous binary version mismatch' }
Smoke-Installed 'previous-start'
New-Item -ItemType Directory -Force $settingsDir | Out-Null
'{"Schema":1,"Color":"#FFD166","LastFolder":"C:\\Снимки"}' | Set-Content "$settingsDir/settings.json" -Encoding utf8
$before = (Get-FileHash "$settingsDir/settings.json").Hash
Run-Checked "$repo/artifacts/dist/LumenSnap-Setup-$Version-win-x64.exe" @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LOG=`"$out/upgrade.log`"") 'upgrade install'
if ((Get-Item $exe).VersionInfo.ProductVersion -notlike "$Version*") { throw 'Installed binary version mismatch' }
if ((Get-ItemProperty $entry).DisplayVersion -ne $Version) { throw 'Installed Apps version mismatch' }
if ((Get-FileHash "$settingsDir/settings.json").Hash -ne $before) { throw 'Upgrade changed settings' }
if (-not (Test-Path (Join-Path $env:APPDATA 'Microsoft/Windows/Start Menu/Programs/LumenSnap/LumenSnap.lnk'))) { throw 'Start Menu shortcut missing' }
$run = Get-ItemProperty 'HKCU:/Software/Microsoft/Windows/CurrentVersion/Run' -ErrorAction SilentlyContinue
if ($null -ne $run -and $run.PSObject.Properties.Name -contains 'LumenSnap') { throw 'Installer unexpectedly enabled autostart' }
Smoke-Installed 'upgraded-start'
$resident = Start-Process $exe -PassThru -WindowStyle Hidden
Start-Sleep -Milliseconds 1200
if ($resident.HasExited) { throw 'Tray app exited on launch' }
$resident.Refresh()
$cpuStart = $resident.TotalProcessorTime.TotalMilliseconds
Start-Sleep -Seconds 2
$resident.Refresh()
$idle = @{ sampleSeconds=2; cpuMilliseconds=($resident.TotalProcessorTime.TotalMilliseconds - $cpuStart); workingSetBytes=$resident.WorkingSet64; handles=$resident.HandleCount }
$second = Start-Process $exe -PassThru -WindowStyle Hidden
if (-not $second.WaitForExit(10000) -or $second.ExitCode -ne 0) { throw 'Second instance did not exit cleanly' }
Run-Checked $exe @('--quit') 'tray quit'
if (-not $resident.WaitForExit(10000)) { throw 'Tray instance did not shut down' }
Run-Checked "$installed/unins000.exe" @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LOG=`"$out/uninstall.log`"") 'uninstall'
if (Test-Path $exe) { throw 'Uninstall left application executable' }
if (Test-Path $entry) { throw 'Uninstall entry remained' }
if (-not (Test-Path "$settingsDir/settings.json")) { throw 'Default uninstall must retain settings' }
Run-Checked "$repo/artifacts/dist/LumenSnap-Setup-$Version-win-x64.exe" @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LOG=`"$out/reinstall.log`"") 'reinstall'
Smoke-Installed 'reinstalled-start'
Run-Checked "$installed/unins000.exe" @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/PURGESETTINGS=1',"/LOG=`"$out/purge-uninstall.log`"") 'purge uninstall'
if (Test-Path "$settingsDir/settings.json") { throw 'Explicit settings purge failed' }
@{ source=(& git -C $repo rev-parse HEAD).Trim(); version=$Version; previous=$PreviousVersion; status='PASS'; os=[System.Runtime.InteropServices.RuntimeInformation]::OSDescription; osCaption=(Get-CimInstance Win32_OperatingSystem).Caption; idle=$idle; note='Disposable hosted runner, NOT clean Windows 11 or manual acceptance' } | ConvertTo-Json | Set-Content "$out/installer-result.json" -Encoding utf8
