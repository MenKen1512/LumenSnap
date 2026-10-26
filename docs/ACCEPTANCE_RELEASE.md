# LumenSnap — verification, installer and release acceptance

**This is a delivery contract, not evidence that any code or installer currently exists.** Product requirements: docs/PRODUCT_SPEC.md. UI requirements: docs/DESIGN_SPEC.md.

## 1. Independent development and evidence

- Codex develops from an **isolated worktree/branch** against the freshest GitHub main/PR state, runs reproducible native tests, self-reviews, creates PRs and checks exact-head SHA before any permitted merge.
- GitHub APIs are not a Windows execution runtime. If Windows testing is unavailable, label checks **NOT RUN**; do not mark an installer verified on build metadata alone.
- Use Codex's native isolated Windows environment or an explicitly authorized approved Windows test executor for integration. Do not silently run untrusted source directly on MENKEN-PC or bypass any existing host/Runtime security restriction.
- REA/Ghidra research, if legitimately accessible, is **read-only** and independent of product builds. A successful REA status is not a successful decompilation; its absence is not permission to relax protections.
- No user host installation, Lightshot takeover or deployment before an explicitly permitted local acceptance step.

## 2. Automated test layers

### Unit tests (required)
- Rectangle selection arithmetic: minimum region, edge/corner resizing, moving, clipping and monitor negative coordinates.
- Coordinate transforms: DPI scaling, capture pixel dimensions and logical/UI coordinates.
- Annotation object model: draw/update/delete, z-order, property changes, undo/redo, text state.
- Export compositing: selected region only, correct output dimensions and optional glow rasterization; screenshot overlay never included.
- Hotkey state machine: global activation, collision/failure handling, text-edit focus vs Ctrl+C, Esc hierarchy.
- Settings serialization, migration/defaults, corrupt settings recovery.
- Filename/format behavior and cancellation/saving state.

### Windows integration tests (required)
- Real monitor capture and clipboard interoperability with common image-aware apps.
- Unicode/Cyrillic text input and file names; custom hotkey binding; tray launch/quit; second instance.
- Windows Save As and Print dialogs; cancellation does not lose edits.
- UI screenshot checks: no panel overlap with selection and bottom alignment of two panels.
- Installer on clean Windows 11 x64 test environment, per-user installation, launch, version reporting, uninstall and reinstall.

### Visual/manual acceptance (required)
- Prove that no pixels from the toolbar or glowing selection boundary appear in saved or copied image.
- Review annotation aesthetics against actual bright/dark content, not only a dark wallpaper.
- Measure start/hotkey-to-ready responsiveness and long-idle resource use and report real values; do not claim arbitrary FPS/latency targets as achieved without measurement.
- Regression after multiple repeated captures, resizing and monitor reconnect.

## 3. Acceptance matrix (P0)

| ID | Reproducible scenario | Expected observation |
| --- | --- | --- |
| T-01 | Start from tray; select rectangle | Selection/handles show; toolbars outside image; size label changes |
| T-02 | Select near screen right/bottom edges | Both panels remain visible, move outside selection, never appear in final crop |
| T-03 | Normal large selection | Vertical/horizontal bars align at bottom edges, small deliberate gap, compact clustered buttons |
| T-04 | Draw arrow on light and dark areas | Medium-bold anti-aliased shaft/head, restrained glow in **export**, adjustable color/thickness |
| T-05 | Pencil, line, rectangle, circle | Correct paths/shapes and configurable color/width |
| T-06 | Type Cyrillic multi-line text | Text remains legible, editable, resizable and re-stylable |
| T-07 | Edit text, select characters, Ctrl+C | Copies text selection according to focus policy; capture remains open |
| T-08 | Outside text edit press Ctrl+C | Clipboard gets only region + annotations; capture closes **on success** |
| T-09 | Clipboard busy/error | Useful error; capture/annotations retained |
| T-10 | Press Ctrl+S and cancel Save As | Capture stays open with all edits |
| T-11 | Save PNG | File has correct selected pixel dimensions and no toolbar/glowing frame |
| T-12 | Save JPG/BMP | Formats load correctly, transparent/background behavior documented |
| T-13 | Save to invalid/unwritable location | Error is shown, editor stays open |
| T-14 | Undo/Redo annotation/text edit | State/history restored; buttons enable/disable correctly |
| T-15 | Press Print then cancel | Native print dialog exits; no lost editing work |
| T-16 | Register an available custom hotkey | Next key press starts capture; binding persists after app restart |
| T-17 | Bind reserved/already-used hotkey | App detects failure, keeps existing binding, shows conflict |
| T-18 | Lightshot installed alongside LumenSnap | Both remain installed; no silent takeover, autostart or settings modifications |
| T-19 | Two monitors, different scaling, negative origin | Selection and clipboard/save dimensions match physical pixels |
| T-20 | Esc in text edit, then Esc in capture | Context-aware exit, no stuck overlay/process |
| T-21 | No network available | All P0 features still work; no automatic uploads or analytics |
| T-22 | Quit tray, reopen and second instance | Clean shutdown and single-instance behavior |
| T-23 | Install and uninstall in clean Windows 11 x64 | Setup succeeds as normal user; Start entry, app entry, uninstaller and shortcuts correct |
| T-24 | Install upgraded version over old version | Settings preserved, app starts at new version, uninstall still works |
| T-25 | Copy/Save hover/keyboard focus and 125%-200% DPI | Icons/text sharp, focus discernible, panels visible and not clipped |

A test is **PASS only with reproducible evidence** (test result/log/screenshot tied to the version). **NOT RUN**, **PARTIAL**, or **BLOCKED** must not be presented as PASS.

## 4. Installer and packaging

The primary deliverable is a normal **Windows x64 Setup.exe**. Preferred packaging: **Inno Setup** with a self-contained .NET desktop application (if the final stack is C#/.NET/WPF).

Requirements:
- Offline install for the current user by default, no UAC required for normal use; request elevation only if genuinely required and explicitly authorized.
- Include all runtime dependencies necessary to start on a clean supported Windows 11 x64 system. Installer must not download runtime at first launch.
- Conventional Start Menu shortcut, Windows Installed Apps/Uninstall entry, tray app executable and version metadata; optional desktop shortcut.
- No surprise launch-at-login, no system shortcut/PrintScreen override during installation.
- Safe upgrade, single installed version, no duplicate tray/startup instances. Do not delete user settings on upgrade. Uninstall should offer/configure whether to retain per-user settings and remove own startup entry.
- Exe, icons and UI resources must be originals or appropriately licensed. Include license/attribution files as required.
- Versioned artifact name: **LumenSnap-Setup-<version>-win-x64.exe**. Optional second artifact: **LumenSnap-Portable-<version>-win-x64.zip** if time allows, clearly not a substitute for the installer.
- Provide a reproducible documented build command/script and packaging assets in the repository. Build output must come from a traceable Git commit.
- Include SHA-256 digest for each published artifact; document build machine/OS/tool versions.
- Unsigned builds must be openly labeled **unsigned**, with possible Windows SmartScreen warnings. Do not claim code-signing or warning-free install without a legitimate signing identity and actual validation.
- Auto-update is deferred. Future updates can initially be performed manually from GitHub Releases.

## 5. Versioning and release gates

### R0 — Research and architecture
- Behavior-based Lightshot baseline documented without proprietary source.
- REA feasibility checked and legal/permission boundaries recorded; no insecure workaround.
- Project skeleton/architecture decision and measurable test list exist.

### R1 — Functional preview
- Capture overlay, selected region, Copy, Save, keyboard handling and UI panels implemented.
- Real Windows smoke evidence available for at least capture/clipboard/save.
- No installer claim yet unless one has actually been built and tested.

### R2 — First installable beta
- All P0 annotation/text tools, settings/tray and printing implemented.
- Actual Windows Setup.exe built; install/uninstall/upgrade smoke-tested.
- Reproducible native tests and all P0 acceptance matrix statuses documented, open failures tracked.
- No automatic deployment on MENKEN-PC.

### R3 — First stable
- All T-01..T-25 P0 scenarios pass in relevant tested Windows environment with evidence.
- Exact source SHA, user-visible version and Release artifact digest recorded.
- Reproducible build, end-user install/use/uninstall documentation and license review complete.
- User approves any later local MENKEN-PC installation separately. Actual user-host acceptance is recorded independently and is not implied by GitHub Release.

## 6. Evidence in every implementation PR/release

Provide: base/HEAD SHA, branch, scope, diff summary, test commands and raw counts/results, Windows environment and DPI/monitor setup where applicable, exact installer filename/SHA-256, known limitations, rollback/uninstall path and what remains unverified. Hosted GitHub Actions are optional for ordinary changes and must never be described as enforced without actual branch-protection evidence.

**Do not merge or announce stable release just because a workflow is green or an executable file exists.** The acceptance matrix and clean-Windows installer test are the completion bar.
