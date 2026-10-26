# Verification status

Implementation is a preview. CI availability was actually verified by run
[40389443223](https://github.com/MenKen1512/LumenSnap/actions/runs/40389443223)
at source `82486e31c8cd74b7facdd4bc159990769ec8d236`. This probe is not an app test.
The implementation's exact-source results will be attached to PR #2 and release.
No Windows 11 manual acceptance, MENKEN-PC installation, code-signing or Lightshot
performance comparison has been performed.

| Acceptance IDs | Current state | Required evidence |
| --- | --- | --- |
| T-01, T-03 | NOT RUN | Actual overlay UI and interactive selection |
| T-02 | KNOWN LIMITATION | Exterior panel space; see ARCHITECTURE.md |
| T-04, T-05 | NOT RUN | Preview/export shapes and visual review |
| T-06, T-07, T-20 | NOT RUN | Cyrillic/IME input, native text commands, Escape hierarchy |
| T-08, T-09 | NOT RUN | Clipboard success and locked clipboard recovery |
| T-10, T-13, T-15 | NOT RUN | Cancel/error paths in actual Windows dialogs |
| T-11, T-12 | NOT RUN | Exact-pixel image encode/decode and no UI artifacts |
| T-14 | NOT RUN | Object/text/style history and interactive text undo |
| T-16, T-17 | NOT RUN | Hotkey registration, replacement conflicts and physical input |
| T-18 | NOT RUN | Side-by-side Lightshot on separately authorized environment |
| T-19, T-25 | NOT RUN | Actual multiple monitors and 125–200% DPI visual/input checks |
| T-21 | NOT RUN | Disconnected-machine operation; source has no network client |
| T-22 | NOT RUN | Tray shutdown and second instance |
| T-23, T-24 | NOT RUN | Installed EXE lifecycle and upgrade, then clean Windows 11 check |

Source-level tests and automated runner checks may justify PARTIAL status for
manual scenarios. They cannot alone justify PASS for the full scenario. Stable
release is withheld until full acceptance. No branch-protection enforcement is
claimed. Native source tools here are used for source editing only.
