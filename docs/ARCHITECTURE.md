# Architecture decision

Retain the approved C# / .NET 10 / WPF / Inno Setup stack. There is no web renderer,
service, network client, AI runtime or third-party NuGet dependency.

- `Native.cs`: GDI snapshot before opening overlay; all GDI resources released in
  finally; native hotkeys replace bindings only after a new registration succeeds.
- `Model.cs`: physical-pixel selection arithmetic, immutable annotation values,
  bounded 100-operation history, panel layout and focus-aware shortcut policy.
- `Rendering.cs`: shared annotation preview/export routines. Export receives only
  frozen pixels, crop and objects; no overlay control tree. Default 96-DPI bitmap
  coordinates avoid accidental scaling. Save uses sibling temporary file then move.
- `Overlay.cs`: original Midnight Glow WPF UI, region manipulation, object editing,
  native TextBox editing, separate compact panels, native save and print dialogs.
- `Program.cs`: single-instance tray lifecycle, deferred capture after tray dismissal,
  hotkey collision reporting. No persistent rendering timer; capture timer is one-shot.
- `Settings.cs`: versioned per-user JSON; validation/default recovery; explicit opt-in
  HKCU startup setting. No Windows/Lightshot preference modifications.
- `Smoke.cs`: opt-in diagnostics on disposable Windows test machine. Actual WPF
  rendering over a synthetic fixture plus independent real desktop capture checks.
- `build/` and `installer/`: self-contained per-user package and executable lifecycle
  tests. No development installation or execution on MENKEN-PC.

One overlay spans the frozen virtual desktop. Its canvas uses physical pixel
coordinates with the inverse HWND DPI transform; monitor-relative panels use
monitor DPI. Negative virtual origins are translated once at the capture boundary.
Display topology changes cancel stale capture. Real mixed-DPI acceptance remains
required; arithmetic tests alone do not prove it.

When selection covers a full monitor or virtual desktop, panels dock inside the
selected monitor without a scene transform. Other regions may reserve exterior
workspace by scaling only the editing view if exterior panels cannot fit. Scene
coordinates stay in physical pixels; WPF inverse transforms map input back to the
unchanged document. Export never uses the scene or any control tree, so even
internal full-screen panels cannot appear in saved pixels. On viewports smaller than the
panels themselves, keyboard operations remain available; those viewports require
separate accessibility/design work. Real mixed-DPI and pointer acceptance remains
required before stable release.
