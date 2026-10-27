# Verification status

## 0.1.2 UI revision

The workflow upgrades the checksum-pinned published 0.1.1 installer to 0.1.2.
Additional gates exercise internal full-screen panels with unscaled, UI-free
export; all ten color presets changing the real editable document; removal of
HEX fields; compact property bounds; and native hover/press animations. Motion
is enabled explicitly in diagnostics to exercise storyboards even on a runner
with OS animations disabled. Normal launch honors Windows animation preference.
Gradient glow is checked for exterior clipping and spatial color variation.
Read exact-source logs/JSON for observed results; full Windows 11 manual
acceptance remains NOT RUN.


## 0.1.1 UI revision

The workflow now verifies upgrade from the checksum-pinned published 0.1.0
installer (source `218e4ee603eb63ef70035b94fbe1208fc4d8e253`). The new smoke
scenarios exercise active-tool hover and held mouse press through native Windows
input, keyboard focus, the actual text-properties popup/font dropdown, settings
scrolling/rendering, and Close immediately after Save. UI PNGs are rendered from
the real WPF controls with a synthetic screenshot background. The component
suite also checks external-only selection glow at 100/125/150/200% scale.
These checks are release gates; read the attached exact-source JSON/logs for
their observed results. Physical mixed-DPI input and full Windows 11 acceptance
remain NOT RUN. No comparison with Lightshot performance is claimed.

## Historical baseline

Implementation is a preview. Actual Windows CI availability was verified by run
[40389443223](https://github.com/MenKen1512/LumenSnap/actions/runs/40389443223).
The first full installer lifecycle passed in run
[40391401113](https://github.com/MenKen1512/LumenSnap/actions/runs/40391401113)
at source `cbcca8011029b6e344d63083ac7a0c46339b0c76`: 25 unit/component checks,
7 installed-application smoke checks per install (previous / upgraded / reinstalled),
and the install/start/upgrade/uninstall/reinstall lifecycle passed.
This is predecessor evidence; the release's exact-source evidence is its attached
build manifest, unit log, installer-result.json and smoke.json, not this baseline.
No Windows 11 manual acceptance, MENKEN-PC installation, code-signing or Lightshot
performance comparison has been performed.

| Acceptance IDs | Current state | Required evidence |
| --- | --- | --- |
| T-01 | PARTIAL | Actual WPF overlay render/open/close passed; manual region input pending |
| T-02 | PARTIAL | Exterior workspace/edge arithmetic tests; manual corner input pending |
| T-03 | PARTIAL | Aligned compact panels seen in actual rendered UI; manual acceptance pending |
| T-04 | PARTIAL | Arrow glow exported and visible in PNG; broader bright/dark review pending |
| T-05 | PARTIAL | Geometry/hit tests and pencil render; all-tool manual operation pending |
| T-06 | PARTIAL | Cyrillic multiline TextBox render; actual typing/IME/restyling acceptance pending |
| T-07 | PARTIAL | Focus policy unit check; native Copy command in latest smoke; physical Ctrl+C pending |
| T-08 | PARTIAL | Native Windows image clipboard roundtrip; external-app paste pending |
| T-09 | NOT RUN | Real cross-process clipboard lock/retry |
| T-10 | NOT RUN | Cancel native Save As dialog with edits |
| T-11 | PARTIAL | Exact pixel crop/PNG roundtrip without UI passed; manual screenshot comparison pending |
| T-12 | PARTIAL | JPEG/BMP decode and dimensions passed; manual visual comparison pending |
| T-13 | PARTIAL | Failed atomic save preserves original; actual error dialog input pending |
| T-14 | PARTIAL | Model creation/style/move/delete/undo/redo; native text input regression pending |
| T-15 | NOT RUN | Physical printer / native Print dialog cancellation |
| T-16 | PARTIAL | Native registration passed; physical key and restart interaction pending |
| T-17 | PARTIAL | Native conflict preserves existing registration; interactive settings pending |
| T-18 | NOT RUN | Side-by-side Lightshot in separately authorized environment |
| T-19 | PARTIAL | Negative coordinates/DPI math tested; actual mixed-DPI monitors pending |
| T-20 | PARTIAL | Focus policy, latest first-Escape routed check, repeated overlay close; physical sequence pending |
| T-21 | PARTIAL | No network client/dependency in source; disconnected-machine test pending |
| T-22 | PARTIAL | Hosted tray start/second invocation/quit passed; manual tray menu pending |
| T-23 | PARTIAL | Hosted Server installation/start/uninstall passed; clean Windows 11 NOT RUN |
| T-24 | PARTIAL | Real older-version fixture upgraded, settings retained; clean Windows 11 NOT RUN |
| T-25 | PARTIAL | Native focus styles and DPI arithmetic; 125–200% real rendering/input NOT RUN |

Source-level tests and automated runner checks may justify PARTIAL status for
manual scenarios. They cannot alone justify PASS for the full scenario. Stable
release is withheld until full acceptance. No branch-protection enforcement is
claimed. Native source tools here are used for source editing only.
