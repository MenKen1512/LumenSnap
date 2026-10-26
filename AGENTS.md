# LumenSnap development instructions

Repository: MenKen1512/LumenSnap. This is an independent, offline-first Windows screenshot/annotation product, not a JARVIS module.

## Authoritative requirements

- Read CODEX_TASK.md, docs/PRODUCT_SPEC.md, docs/DESIGN_SPEC.md and docs/ACCEPTANCE_RELEASE.md before implementing or redefining scope.
- The user's approved layout has **two distinct compact panels**: vertical drawing tools to the right of the selection, horizontal actions immediately below it, adjacent near the lower-right corner. Both panels stay outside selected pixels.
- The primary actions are Copy and Save; the primary drawing tools are freehand Pencil/Marker, Arrow and Text.
- The first release must include a tested Windows installer executable, plus source and documented build instructions. Do not equate successful compilation with a working installer.

## Development workflow

- Use GitHub-first, a new isolated branch/worktree per change, self-review the diff, run relevant reproducible tests, publish a PR and verify its exact head. Do not edit the user's human checkout as a shortcut.
- Codex and other developers use their own authorized native environments; no mandatory JARVIS Runtime task, local model, reviewer or custom orchestration.
- Do not install or launch any software on MENKEN-PC, change system hotkeys/settings or touch running Lightshot without a distinct authorized local operation.
- Treat the Research/REA stage as read-only and legally bounded. Confirm access and licensing; never bypass tool denials or copy proprietary Lightshot code/assets/binaries into the repo.
- Never invent native Windows test results, code signing, CI success, installation or live acceptance. State exact SHA, platform and evidence, or mark NOT RUN.
- Keep implementation offline-first; do not add telemetry, upload services or unauthorized network dependencies.
- Preserve all final user-visible strings and settings in Russian at minimum; ensure Cyrillic input works.
