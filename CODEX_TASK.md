# Codex implementation brief — LumenSnap

## Mission

Build a **standalone, offline-first, beautiful Windows screenshot and annotation application** that replaces the user's core Lightshot workflow. Deliver not only source code but a **reproducibly built and tested installable Windows x64 Setup.exe**. Development begins only when explicitly tasked; this document is the approved handoff specification.

**Canonical repository:** https://github.com/MenKen1512/LumenSnap

**Read in this order:**
1. AGENTS.md
2. docs/PRODUCT_SPEC.md
3. docs/DESIGN_SPEC.md
4. docs/ACCEPTANCE_RELEASE.md
5. Current main, existing branches/PRs and the actual repository code.

The product specification outranks speculative mockups or old chat summaries.

## Five non-negotiables

1. Preserve core Lightshot workflow: quick configurable capture hotkey, region selection, annotation, Ctrl+C copy/close, Ctrl+S Save As, printing, undo/redo and tray.
2. Make text genuinely comfortable: editable font/size/color/weight/alignment and re-editable text directly on selected pixels. Strong Pencil, Arrow and Text tools; attractive restrained glow on exported arrows.
3. Make the actual overlay **compact**: a narrow vertical right toolbar and a short horizontal lower toolbar. Their **bottom edges align**; horizontal panel finishes immediately to the left of the right panel, with minimal gap. Ordinary regions keep panels outside. User revision 2026-10-27: full-screen/full-monitor selection places panels inside without shrinking the view; export excludes all UI. All lower buttons are grouped; Copy/Save are modestly larger/stronger, not giant cards. Ten named colors replace HEX entry throughout compact menus.
4. No cloud/upload/share/search/OCR/video/AI/telemetry or unused placeholder buttons in the first release. No network needed for normal use.
5. Produce an actual installable offline Windows Setup.exe, verify install/start/uninstall/upgrade, and publish clear exact-SHA evidence. Source-only success is not the goal.

## Work ownership and limits

Act as an **independent developer** using your own Git/worktree/sandbox, feature branches and PRs. Start from **fresh GitHub main and current active PRs**, not a cached handoff. Self-review, run suitable isolated tests and attach exact-head evidence. Do not depend on ChatGPT, Claude, JARVIS Runtime, mandatory local reviewers or the user's resident LLM to perform ordinary development.

Use C#/.NET 10 + WPF + Inno Setup by default; if an alternative is materially better, explain and document the trade-off before replacing the platform decision.

**No local installation/deployment or modification of Lightshot on MENKEN-PC** in this source task. Actual host acceptance is a separate explicitly permitted step.

## Research instruction — Lightshot / REA

Lightshot is a **behavioral and interaction reference**, not a codebase to copy.

- First create a reproducible **functional inventory** of the P0 behaviors with user-observable evidence, Windows API references, and clear known/unknown status.
- Investigate access to **REA and Ghidra** using your own legitimately available environment or separately authorized read-only tooling. Validate real operation on a legal, SHA-verified copy and record tool/provider versions and result boundaries.
- Confirm local software rights/licensing before reverse engineering. No uploading installed Lightshot exe, imported UI icons/assets, decompiled pseudocode, proprietary strings or private research artifacts to this **public** repository.
- Existing JARVIS Workstation Bridge REA input roots do **not** automatically authorize Lightshot. Never forge another project, bypass a denied read path or broaden a security control to make analysis work.
- If REA is inaccessible, forbidden, legally uncertain or simply not useful, **do not block implementation**. Record the limitation, derive functional requirements via legal black-box observation and public platform APIs, and build your own logic.
- Publish only independently written functional descriptions/tests; do not copy proprietary implementation.

## Execution stages

**0 — Inspect and baseline.** Verify main/HEAD, existing PRs and available Windows testing toolchain. Inventory P0 Lightshot behavior; evaluate REA legally/safely and record gaps. Make a compact architecture decision with scope + dependencies.

**1 — Capture core.** Single-instance tray host, hotkey registration and settings, freeze frame, drag/resize/move selection, high-DPI/multi-monitor support; implement lower/right panels and copy/save. Test bitmap integrity early.

**2 — Editable annotations.** Pencil/Marker, visible medium-bold glow Arrow, Text, line, rectangle, ellipse, color/width, object editing and Undo/Redo. Implement focus-aware shortcuts and Print. Match docs/DESIGN_SPEC.md.

**3 — Packaging.** Reproducible Release and self-contained per-user Setup.exe, version metadata, README/build manual, uninstall/upgrade test, optional portable ZIP. No forced autostart or conflicts with Lightshot.

**4 — Quality and release.** Run tests in an **actual authorized Windows environment**, provide real UI screenshots and a T-01..T-25 acceptance matrix; fix issues, self-review and publish GitHub PRs/release assets at exact SHAs. Mark any unavailable test NOT RUN and withhold stable-claim until verified.

Do not turn this into a large framework or a multi-agent orchestration. Implement the smallest correct independent application in iterative verified PRs. Do not add out-of-scope features.

## Deliverables

- Clean, documented buildable source and dependency/license manifests.
- User-usable Russian UI, configurable shortcuts, functional editor and tray.
- Automated and manual tests with evidence linked to exact source versions.
- Screenshots from the **real application**, not only AI concept images.
- Versioned **LumenSnap-Setup-<version>-win-x64.exe**, SHA-256 and clean-Windows installer verification; ideally a GitHub Release.
- Reproducible build/install/uninstall and troubleshooting instructions.
- Summary of completed items, PR/commit links, tests, deployment status and limitations.

## Completion boundary

Do not claim the original user goal complete at source compilation, mockup approval, REA output, PR merge, or GitHub Release alone. The first product-level milestone is an **actually tested Windows installer plus P0-compliant app**. Installation on MENKEN-PC remains separately authorized.
