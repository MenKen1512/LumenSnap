# Behavioral baseline and research boundaries

Research date: 2026-10-27 (Asia/Yekaterinburg). Starting main:
`5c42c205f917b64ffdc83e37df68bbd63725a207`. PR #1 was merged documentation;
no existing implementation branch or application was present.

This inventory uses public first-party documentation, not observation of the
user's installed binary. Documented behavior is not a measured Lightshot result.

| Behavior | Public evidence / known state | Independent LumenSnap contract |
| --- | --- | --- |
| Activate and select region | [Official shortcuts](https://app.prntscr.com/en/hotkeys.html), documented PrintScreen workflow | Configurable RegisterHotKey; collision-safe, tray fallback |
| Copy image | Same source, Ctrl+C | Standard Windows image clipboard; close only on success |
| Local formats | [Official features](https://app.prntscr.com/en/learnmore.html), PNG/JPEG/BMP | Native encoders; atomic file replacement |
| Save shortcut | Official shortcuts describe direct recent-folder saving | Approved product spec takes precedence: always Save As |
| Cancel | Official shortcuts describe Escape | Text-edit Escape commits first, capture Escape cancels second |
| Online editing / upload | Official pages document online actions | Explicitly excluded by approved scope |
| Local annotations, text editing and panels | Exact version and current behavior NOT OBSERVED | Use approved product/design specs; original implementation |
| Undo, print, special keys | Installed-version semantics NOT OBSERVED | Implement and test the approved LumenSnap contract |
| Mixed DPI and performance | NOT MEASURED on Lightshot | Physical-pixel capture, no idle rendering loop; comparison pending |

Platform references: [RegisterHotKey](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey),
[GDI capture](https://learn.microsoft.com/en-us/windows/win32/gdi/capturing-an-image),
[DPI guidance](https://learn.microsoft.com/en-us/windows/win32/hidpi/high-dpi-desktop-application-development-on-windows),
[self-contained deployment](https://learn.microsoft.com/en-us/dotnet/core/deploying/),
[Inno per-user installation](https://jrsoftware.org/ishelp/topic_setup_privilegesrequired.htm).

## REA / Ghidra feasibility

NOT RUN. This session exposes no dedicated REA/Ghidra tool. No licensed,
SHA-verified research copy of Lightshot is available in the isolated clone.
The repository grants no authorization to read the installed MENKEN-PC copy or
broaden JARVIS input roots; applicable reverse-engineering rights are unconfirmed.
No binaries were fetched, copied, uploaded, decompiled or modified, and no access
restriction was bypassed. Tool version/provider and binary SHA: unavailable.
This is a research limitation, not an implementation blocker. Any later analysis
needs a lawful research copy, hash, tool version and explicit permitted scope.
