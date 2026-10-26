# LumenSnap

LumenSnap is an independent, offline-first Windows screen-capture and annotation application written in C#/.NET 10 and WPF.

**Status: implemented preview, not stable.** Self-contained per-user Inno Setup EXE; actual Windows Server runner install/start/upgrade/uninstall/reinstall checks and native WPF/clipboard tests. Full Windows 11 x64 manual acceptance remains **NOT RUN**. No installation or execution on MENKEN-PC.

[Downloads / unsigned prereleases](https://github.com/MenKen1512/LumenSnap/releases) · [Implementation PR #2](https://github.com/MenKen1512/LumenSnap/pull/2) · [Russian user guide](docs/USER_GUIDE_RU.md) · [Build instructions](docs/BUILD.md) · [Verification / limitations](docs/VERIFICATION.md)

Default shortcut: **Ctrl+Alt+S**, configurable in tray settings. The screenshot editor includes selection move/resize, Pencil/Marker, Arrow with exported glow, editable Cyrillic text, line/rectangle/ellipse, object move/resize/delete, Undo/Redo, Copy, Save As (PNG/JPEG/BMP) and Print. Text input keeps its own clipboard and undo commands. When exterior panel space is tight, only the editing view scales; export retains original pixels.

Build on an authorized Windows developer machine with .NET SDK 10 and Inno Setup 6: `./build/Build.ps1 -Version 0.1.0`. Installer lifecycle tests require a disposable authorized Windows VM; see the build guide. Release assets contain exact source SHA, toolchain metadata, checksums and raw test evidence. Runtime is bundled; normal use is offline. Builds are unsigned.

## Product direction

- Modern, compact **Midnight Glow** UI with a softly illuminated selection rectangle.
- Floating **vertical editing tools on the right** and **horizontal actions below**, both outside the selection, aligned near its lower-right corner.
- Prominent but compact **Copy** and **Save** actions, plus Pencil, Arrow and Text tools.
- Editable text with font, size and color controls; smooth arrows, freehand pen and geometric shapes.
- Offline-first operation, tray integration, configurable capture hotkey, native clipboard, saving and printing.
- A distributable Windows **Setup.exe**, not merely source code.

## Start here

1. [CODEX_TASK.md](CODEX_TASK.md) — execution brief for Codex.
2. [docs/PRODUCT_SPEC.md](docs/PRODUCT_SPEC.md) — scope and functional contract.
3. [docs/DESIGN_SPEC.md](docs/DESIGN_SPEC.md) — UI layout and interaction contract.
4. [docs/ACCEPTANCE_RELEASE.md](docs/ACCEPTANCE_RELEASE.md) — tests, installer and release acceptance.
5. [AGENTS.md](AGENTS.md) — repository instructions for autonomous developers.

## Original application reference

Lightshot is a **behavioral reference**, not a source dependency. Do not commit or redistribute Lightshot binaries, decompiled source, assets or private research data. No affiliation with Lightshot is implied.

First release intentionally excludes cloud upload, remote links, image search, OCR, video recording, AI and telemetry.
