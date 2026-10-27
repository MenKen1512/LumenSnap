$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$dist = Join-Path $PSScriptRoot '../artifacts/dist'
New-Item -ItemType Directory -Force $dist | Out-Null
$file = Join-Path $dist 'LumenSnap-Setup-0.1.0-win-x64.exe'
$url = 'https://github.com/MenKen1512/LumenSnap/releases/download/v0.1.0-preview.218e4ee6/LumenSnap-Setup-0.1.0-win-x64.exe'
$expected = '22ea684db0996f8dea5dbad5c2b56d30bc026e51d57e8b60fc7ee73b5cb9ee1a'
Invoke-WebRequest -Uri $url -OutFile $file
if ((Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) { throw 'Published 0.1.0 installer checksum mismatch' }
@{ url=$url; sha256=$expected; source='218e4ee603eb63ef70035b94fbe1208fc4d8e253'; version='0.1.0' } | ConvertTo-Json | Set-Content (Join-Path $dist '../previous-installer.json') -Encoding utf8
