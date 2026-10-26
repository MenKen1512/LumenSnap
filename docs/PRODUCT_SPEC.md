# LumenSnap — product and functional specification

**Status:** approved initial product scope; visual polish remains iterative. **Target:** independent offline-first Windows 11 x64 application. **Delivery:** reproducible source + verified installable EXE. **Reference:** Lightshot user-visible behavior, not proprietary implementation.

This document defines **what** the first usable LumenSnap version must do. docs/DESIGN_SPEC.md defines **how it looks**; docs/ACCEPTANCE_RELEASE.md defines **how to prove it works**. The final selection and desktop screenshot images discussed in the design conversation are inspiration only; the geometry and interaction rules in these documents are authoritative when those images are unavailable to Codex.

## 1. Product outcome and non-goals

Goal: replace the user's everyday Lightshot workflow with a faster-to-edit, more attractive native Windows tool while retaining familiar direct region capture, clipboard copy, saving, configurable keyboard activation and printing.

**P0 = mandatory for the first usable installer.** **P1 = included if it is needed for P0 reliability or explicitly marked.** **Deferred = not to implement without a new product decision.**

P0 is intentionally narrower than the entire Lightshot feature catalogue: **do not reproduce unnecessary cloud or network functionality**.

### Excluded from first release (do not build)

- Upload to Lightshot or another server, shareable links, web search for similar images and network integrations.
- OCR, AI assistant, video/GIF recording, scrolling capture, image history/cloud backup, account/login system.
- Telemetry, ads, hidden analytics, background image upload, automatic updater or silent update installation.
- Any binary patching, runtime injection, replacement of Lightshot, or reuse of proprietary Lightshot source/assets.
- A separate daemon, external service, browser dependency or JARVIS Runtime integration.

The first release must remain **useful completely offline**. Do not introduce menu buttons that lead nowhere. Excluded features may be proposed as future enhancements, not quietly added to the baseline.

## 2. Core capture workflow

### CAP-01 — Entry and lifecycle (P0)
- User starts LumenSnap from its configurable global hotkey or the tray menu.
- The app shows a frozen snapshot of the current virtual desktop with outside-selection dimming. No toolbar/window/capture overlay may appear in the exported image.
- Click-drag draws a region. The selection displays an antialiased soft-glow frame, handles and a live pixel dimension label.
- Region can be resized from corners/edges, repositioned, and edited while the capture session is active; selection boundaries remain within the available desktop area.
- Esc cancels cleanly; repeated open/close cycles must not leave a stuck transparent overlay or active input hook.
- The main application remains in the tray after a capture action completes, unless user explicitly quits it.

### CAP-02 — Multiple monitors and DPI (P0)
- Capture the whole virtual desktop coordinate space, including monitors with negative offsets and different DPI scaling.
- Physical capture pixels, selection bounds and resulting bitmap dimensions must be consistent.
- Panels must stay visible on the appropriate display, reposition near edges and avoid interfering with the capture selection.
- Windows 11 x64 is the initial acceptance environment; Windows 10 is not a guaranteed target without separate verification.

### CAP-03 — Image integrity (P0)
- Output contains exactly the selected pixels and committed annotations, without selection borders, resize handles, crosshairs, settings popovers or other LumenSnap UI.
- No unrequested cropping/scaling or lossy conversion in the default PNG workflow.
- If selection changes after adding annotations, retain editable objects and correctly clip them to final region; never silently discard user edits.

## 3. Drawing and annotations

### EDIT-01 — Fixed tool set (P0)
Vertical tools, in this exact order from top:
1. Freehand Pencil/Marker;
2. Arrow;
3. Text;
4. Straight line;
5. Rectangle;
6. Ellipse/circle;
7. Color/options control at the bottom of the vertical panel.

No dedicated Search, Upload, Share, OCR or AI tools in the main panel.

### EDIT-02 — Configurable properties (P0)
- Pencil/Marker: smooth antialiased freehand path, adjustable stroke width and color; optional opacity within the same tool rather than another permanent toolbar button.
- Arrow: ready-to-use clearly visible arrow with proportionate head, configurable thickness/color and subtle optional glow that is **part of the exported annotation**. Default must be visible on both light and dark backgrounds without excessive neon/bloom.
- Line/rectangle/ellipse: drag to place, adjust dimensions and line width/color, consistent antialiasing.
- Colors: compact preset palette, recent/custom colors and explicit HEX input, with a clear visual current selection. Eyedropper may be deferred if implementation threatens the initial schedule.
- Retain per-tool last-used settings within a capture and across sessions where feasible. Provide sensible initial defaults.

### EDIT-03 — Object-based editing (P0)
- Keep text and geometric/drawn objects as editable data until copy/save/print rasterizes final output.
- Allow select/move/delete/resize, reselect active tool and change style of an existing element.
- Undo/Redo must cover creation, deletion, moves, text and property changes, without losing earlier annotations.
- Do not commit annotations to the underlying screenshot prematurely.
- If freehand path control-point editing is disproportionately complex, retain object-level move/delete and disclose that limitation; do not pretend it exists.

### EDIT-04 — Text editor (P0, key differentiator)
- Click to place text; type directly on the image; text shows at a readable default size.
- Contextual editor for font family, numeric font size and convenient size adjustment, weight, alignment, line breaks, color, optional background and optional outline.
- Move/resize the text bounding box; reopen and change text and style after placement.
- Full Cyrillic/Latin/number input; IME and common keyboard navigation must not break.
- Text must not shrink unpredictably after placement or save.
- Copying **selected characters while a text field is editing** must not prematurely end the entire capture session. A text-editing focus state and a capture-level hotkey state must be explicitly implemented and tested.
- Esc while typing first leaves/cancels the active text edit according to a documented consistent rule, without accidentally discarding the entire screenshot; a further Esc may cancel capture.

### EDIT-05 — Tool priority and selection state (P0)
- Pencil/Marker, Arrow and Text are always more visually identifiable than Line/Rectangle/Ellipse. They are not all simultaneously "selected": persistent emphasis and actual active-tool state must be visibly different.
- Tooltips and keyboard focus indication are required; no permanent verbose labels on the narrow vertical strip.

## 4. Lower action toolbar

### ACT-01 — Fixed contents (P0)
Left-to-right in **one compact contiguous horizontal surface**: Undo, Redo, Print, More (ellipsis), then Copy and Save. All buttons are physically grouped near the lower-right of the selection; no empty flex gap splitting left and right sides. The ellipsis can be omitted until a real secondary action exists; **no empty or duplicate menu**.

### ACT-02 — Copy (P0)
- Copy is a high-priority, modestly wider and visually stronger button than normal icon buttons. Tooltip/label explains "Копировать" and Ctrl+C.
- Ctrl+C when capture/editor is active and no text input selection is being edited: rasterize the exact final region plus annotations into Windows image clipboard, then close capture overlay **only after success**.
- Image must paste into common Windows applications without a proprietary format requirement.
- On clipboard failure/lock, show a useful nonblocking error and keep capture session open for retry.
- On success, optional subtle toast with no focus theft; never include toast in the screenshot.

### ACT-03 — Save (P0)
- Save is the second high-priority button, coherent with Copy, not a huge separate card.
- Ctrl+S opens a native Save As dialog. Default PNG; JPG and BMP where reliable.
- Remember last destination/format, allow filename edit, avoid silent overwrite, preserve UTF-8/Cyrillic filenames.
- Close capture only after successful save. On user cancellation, invalid path or write error, retain selection and edits.
- Default operation remains local/offline.

### ACT-04 — Print, Undo, Redo (P0)
- Print invokes a standard Windows print dialog for the selected image with annotations; cancellation returns safely to editing.
- Undo/Redo buttons reflect whether history exists (disabled state when not available).
- More (ellipsis) must contain only genuinely implemented secondary actions; no cloud action placeholders.

## 5. Keyboard and system integration

### HOT-01 — Custom global capture binding (P0)
- User can record/configure the capture shortcut via settings. Do not assume physical PrintScreen is the user's chosen key; they use a nonstandard assignment.
- Support normal Windows-detectable key combinations and independently test special key handling. If a hardware/vendor key does not generate an OS-recognized key event, explain limitation; never claim universal support.
- Explicitly detect and report registration failures and conflicts (Windows Snipping Tool, Lightshot, another app). Never modify Windows/Lightshot settings or force-reassign another program's key.
- Provide usable fallback from tray menu and a default shortcut chosen to minimize collision; allow user reset.

### HOT-02 — In-session shortcuts (P0)
- Ctrl+C copy image unless text editing has focus; Ctrl+S save; Esc context-aware cancel; Ctrl+Z/Ctrl+Y (or Ctrl+Shift+Z) undo/redo, with context-aware text editing.
- Hotkeys work across Russian and English keyboard layouts; document behavior for unsupported/reserved shortcuts.
- User must not lose unfinished annotations due to key focus ambiguity.

### SYS-01 — Tray and settings (P0)
- Single running instance with clear behavior on second launch; tray menu contains "Новый скриншот", "Настройки", "Выход".
- Launch on Windows sign-in is **optional, disabled until explicitly enabled**. Closing a capture session does not quit tray app.
- Settings stored in per-user local application-data location with versioned defaults and safe recovery from corrupt settings.
- Settings: global hotkey, launch-at-login, defaults for color/thickness/font, visual glow intensity (optional), notification toggle, last save folder and image format.
- Local Russian user interface as default; no mandatory account/online license.

### SYS-02 — Responsiveness and resource hygiene (P0)
- Do not launch a heavyweight browser/renderer for capture. Avoid persistent CPU/GPU use while idle and ensure repeated capture does not leak overlays, GDI handles or hooks.
- Performance goals are to be measured against Lightshot on an authorized Windows test system, not asserted without data. Prefer immediate hotkey feedback and smooth, flicker-free interaction.
- Do not require administrator rights for normal operation.

## 6. Implementation constraints

- Preferred stack: C#/.NET 10, WPF (Windows native desktop), Windows desktop capture/clipboard/printing APIs. Codex may justify an alternative only if it preserves a genuine standalone Windows product, offline behavior, UI fidelity and installable EXE.
- Separate capture engine, annotation document model, rendering/export, overlay/UI, hotkey management, settings and packaging so visuals can improve without reimplementing core capture.
- Avoid unnecessary network dependencies, commercial closed-source assets and binding to JARVIS MCP/Runtime.
- Use source-controlled text-based resources and original graphics/icons with appropriate licenses.
- This specification **does not authorize installation** on MENKEN-PC. Windows host acceptance is a separate controlled step.

## 7. Lightshot research boundary

- The original is installed on the user's MENKEN-PC. Its binary is proprietary and must not be copied into this public repository, nor should decompiled functions, assets or secrets be posted.
- Codex should observe user-facing behavior first (test matrix, screenshots where allowed) and use official Windows API documentation for independent implementation.
- REA/Ghidra analysis is **conditional on a legitimately accessible, SHA-verified research copy, the applicable legal/license boundary and an authorized environment**. Record exact version/hash, scope, tool version and uncertainty locally. REA is a research tool, not source-code recovery or deployment authority.
- The currently documented JARVIS Workstation Bridge REA input restrictions do **not** include arbitrary commercial application paths. Do not circumvent read roots, fake an owning project, disable protections, or assume Codex cloud can see MENKEN-PC.
- If REA is not available or analysis is not permitted, label the research gap and proceed with independently implemented, observable P0 behavior. Do not block normal development on reverse-engineering completion.
- Research findings should document **behavior/contracts**, not reproduce Lightshot source code, GUI assets or private protocol details.
