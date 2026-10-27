$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$dist = Join-Path $PSScriptRoot '../artifacts/dist'
New-Item -ItemType Directory -Force $dist | Out-Null
$file = Join-Path $dist 'LumenSnap-Setup-0.1.1-win-x64.exe'
$url = 'https://github.com/MenKen1512/LumenSnap/releases/download/v0.1.1-preview.3ffac60e/LumenSnap-Setup-0.1.1-win-x64.exe'
$expected = 'aee94029acea3c60e9cfc8d00baffe35d77383307c2356815a8852d599d85774'
Invoke-WebRequest -Uri $url -OutFile $file
if ((Get-FileHash $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) { throw 'Published 0.1.1 installer checksum mismatch' }
@{ url=$url; sha256=$expected; source='3ffac60e48060af4f3a256241d47c60f27e118d5'; version='0.1.1' } | ConvertTo-Json | Set-Content (Join-Path $dist '../previous-installer.json') -Encoding utf8
