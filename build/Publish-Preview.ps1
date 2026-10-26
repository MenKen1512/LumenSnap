$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if ($env:GITHUB_ACTIONS -ne 'true' -or $env:GITHUB_EVENT_NAME -ne 'push' -or $env:GITHUB_REF -ne 'refs/heads/codex/windows-offline-preview') { throw 'Preview publishing is scoped to the authorized branch push workflow' }
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $repo
try {
    $sha = (& git rev-parse HEAD).Trim()
    $result = Get-Content artifacts/installer-evidence/installer-result.json -Raw | ConvertFrom-Json
    if ($result.status -ne 'PASS' -or $result.source -ne $sha) { throw 'Current-source installer evidence required' }
    $smoke = Get-Content artifacts/installer-evidence/upgraded-start/smoke.json -Raw | ConvertFrom-Json
    if (@($smoke.results | Where-Object status -ne 'PASS').Count -ne 0 -or $smoke.source -notlike "*$sha") { throw 'Current-source application evidence required' }
    Compress-Archive -Path artifacts/installer-evidence,artifacts/unit-tests.txt,artifacts/environment.txt -DestinationPath artifacts/dist/LumenSnap-Evidence-0.1.0-win-x64.zip -Force
    Copy-Item docs/USER_GUIDE_RU.md,docs/VERIFICATION.md -Destination artifacts/dist
    $files = Get-ChildItem artifacts/dist/*0.1.0* -File | Where-Object Name -notlike 'SHA256SUMS*'
    $files | Get-FileHash -Algorithm SHA256 | ForEach-Object { "$($_.Hash.ToLowerInvariant())  $(Split-Path $_.Path -Leaf)" } | Set-Content artifacts/dist/SHA256SUMS-0.1.0.txt -Encoding utf8
    $notes = @"
LumenSnap 0.1.0 — предварительная сборка, без цифровой подписи.

Исходники: $sha. PR: https://github.com/MenKen1512/LumenSnap/pull/2
Среда: $($result.osCaption), $($result.os), GitHub Actions windows-2025.
Установщик проверен: установка, запуск установленного приложения, обновление реального бинарника 0.0.9 → 0.1.0 с сохранением настроек, удаление, переустановка, явное удаление настроек. Обычный запуск трея, второй экземпляр и выход также проверены.

Тесты и PNG настоящего WPF-приложения находятся в архиве Evidence; фон в UI PNG — синтетический тестовый образец. Нативный захват рабочего стола и Windows clipboard проверялись отдельно. Build manifest фиксирует SDK, runtime и компилятор. SHA-256 всех версионных файлов — в SHA256SUMS.

NOT RUN: чистая Windows 11 x64, полноценная ручная приёмка T-01..T-25, смешанный DPI/несколько физических мониторов, IME, сравнение скорости с Lightshot и одновременная работа с ним. Автоматические проверки НЕ означают прохождения этих сценариев. Это prerelease, не стабильная версия и не завершённая продуктовая приёмка. На MENKEN-PC ничего не устанавливалось и не запускалось.

Известные ограничения: масштабирование только вида редактора для внешних панелей; очень маленькие экраны могут не вмещать панели. Точки карандаша не редактируются по отдельности. Текст вне размеров блока обрезается; обводка отображается после завершения ввода. HDR/защищённое видео не гарантированы. Автообновления нет.

Установка для текущего пользователя, runtime включён, интернет для работы не нужен. Закройте LumenSnap перед обновлением. Удаление через «Установленные приложения», сохранение настроек по выбору. Это обновление не меняет Lightshot, PrintScreen или параметры Windows.
"@
    $notes | Set-Content artifacts/release-notes.txt -Encoding utf8
    $tag = 'v0.1.0-preview.' + $sha.Substring(0,8)
    $assets = @(Get-ChildItem artifacts/dist -File | Where-Object { $_.Name -like '*0.1.0*' -or $_.Extension -eq '.md' } | Select-Object -ExpandProperty FullName)
    & gh release create $tag @assets --repo MenKen1512/LumenSnap --target $sha --prerelease --title "LumenSnap 0.1.0 preview ($($sha.Substring(0,8)))" --notes-file artifacts/release-notes.txt
    if ($LASTEXITCODE -ne 0) { throw 'Preview release publication failed' }
} finally { Pop-Location }
