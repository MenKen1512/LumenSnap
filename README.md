# LumenSnap

LumenSnap is a planned, independent Windows screen-capture and annotation application. Its first release focuses on fast region capture, polished offline editing, configurable shortcuts and a real Windows installer.

**Status:** product/design specification prepared; implementation, native tests and installer are **not yet completed**.

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