# LumenSnap — UI/UX design contract (Midnight Glow)

**Stage:** layout and priorities agreed; visual details, iconography and polish are provisional. This is a design-engineering brief, not a claim that the earlier generated mockups are production-ready.

## 1. Overall layout

- Display a thin, smooth, softly glowing blue selection frame around the selected screen region; darken the rest of the desktop. Selection content is clear, not permanently blurred.
- Two **independent, slim, floating panels** appear **outside** the selection: vertical editing panel along the right side, horizontal action panel directly under the selection.
- **User-approved alignment:** the vertical panel is moved *downward* so its **bottom edge is aligned with the bottom edge of the horizontal panel**. The horizontal panel shifts *leftward*, ends just before the vertical panel and approaches it with a very small intentional gap. Do not place horizontal action buttons under the right panel.
- Both panels appear as a harmonious almost-L arrangement, with matched radii/materials, but **must not fuse into a single thick L-shaped block**.
- The horizontal group is packed near the right edge of the selection: secondary buttons immediately precede Copy and Save. **Do not push secondary buttons to the far left of a long bar or introduce a large empty spacer.**
- Bottom horizontal panel must remain entirely **below** the selection frame, never covering screenshot pixels. Right panel remains entirely **right** of the selection frame.
- On screens with insufficient free right/bottom space, dynamically reposition the toolbars to an available outside edge (left/above as needed), keeping them visible and preserving their grouping. Do not allow offscreen controls or inclusion of panel pixels in an export.

### Geometric positioning reference

Let selection rectangle be (x, y, width, height) in screen logical coordinates, its bottom edge be B, vertical panel left coordinate be Vx, horizontal panel width W and height H.

- Right panel: Vx = x + width + external_gap.
- Horizontal panel top: Hy = B + external_gap.
- Horizontal panel right edge: Vx - inter_panel_gap.
- Horizontal panel left edge: Vx - inter_panel_gap - W.
- Right panel bottom: Hy + H. Its top is calculated from its own natural height so the **panel bottoms are exactly aligned**.

External gap may be around 6-10 device-independent pixels; the gap between separate panel surfaces around 4-8 DIP. These are starting values for visual QA, not fixed physical-pixel coordinates. Handle OS DPI, screen edges, different text sizes and monitor bounds rather than hard-coding a screenshot's coordinates.

## 2. Size and density

The product should feel slightly more comfortable than the old Lightshot UI, but not as oversized as the first generated mockup.

**Initial target range at 100% scaling (tune after actual Windows rendering):**

| Component | Approximate target |
| --- | --- |
| Vertical surface width | 48-56 DIP |
| Vertical tool hit area | 38-44 DIP square |
| Vertical icon size | 18-22 DIP |
| Horizontal surface height | 48-56 DIP |
| Secondary button hit area | 36-44 DIP |
| Copy/Save button width | about 108-136 DIP each, depending on localization |
| Copy/Save height | same as other buttons, or at most a few DIP taller |
| Tool spacing | around 4-7 DIP |
| Corner radius | around 10-15 DIP |

These are **tuning targets**, not demands for pixel-perfect imitation of a generated image. Do not introduce giant action cards, huge padding, duplicated labels or full-width bottom bars.

## 3. Vertical toolbar

Order strictly from top:
1. Pencil/Marker (highlighted priority);
2. Arrow (highlighted priority);
3. Text (highlighted priority);
4. Line (neutral);
5. Rectangle (neutral);
6. Ellipse/circle (neutral);
7. Compact current-color/options button (neutral bottom slot).

All buttons live inside **one continuous narrow vertical surface**. The three priority tools have restrained blue accented resting surfaces/edges so they stand out. An **active** tool gains a distinctly stronger selected state; all three must not look selected simultaneously.

Pencil/Arrow/Text hover can produce a small soft glow and a short tooltip; do not use distracting looping animations. Keep ordinary Line/Rectangle/Ellipse visually quieter but discoverable.

## 4. Horizontal toolbar

One compact horizontal surface, in this order: Undo, Redo, Print, More (only if it has a real purpose), Copy, Save.

- All controls clustered together. Compact neutral icon-only buttons on the left; slightly more prominent Copy/Save on the right, within the **same background panel**.
- Copy and Save are roughly the same visual priority to each other: a contrasting blue-tinted field, clear icon and legible action label; shortcuts Ctrl+C and Ctrl+S can be shown in microcopy only if the density remains balanced, otherwise use tooltips.
- They must not look like enormous separate cards or cover significant desktop area. Primary differentiation = contrast, border and restrained glow, with modest size adjustment only.
- Hover, press, disabled, keyboard-focus and successful-result states need a coherent design. Do not make all buttons bright neon by default.
- Redo is visibly disabled when no redo action exists.
- More menu must not advertise unimplemented cloud/search/share services. If empty, do not show More.

## 5. Colors, materials and typography

Style theme: **Midnight Glow**, clean graphite/navy panels with subtle translucent materials, no Windows XP-like chrome. Soft luminous blue accents on key controls and selection outlines, not overexposed electric-blue blocks everywhere.

Suggested starting color roles (subject to visual revision):
- Main panel: #141F2E to #202C3D dark navy/graphite.
- Accent blue: #5CAFFF to #83C9FF.
- Main text: #EAF3FF.
- Secondary icon/text: #A7B9D1.
- Border: low-contrast cool slate, slightly stronger for hover/focus.

Use accessible contrast for text/icons and test against both bright and dark captured backgrounds. Maintain crisp strokes/icons at 100%, 125%, 150%, and 200% Windows scaling. Animation should be brief and optional/reduced-motion friendly.

## 6. Interaction specifics

**Capture:** hotkey -> frozen desktop overlay -> drag area -> panels anchor near lower-right outside it -> adjust/draw -> Copy/Save/Print/Esc. Creating or resizing a selection should not flicker the desktop.

**Selection:** fine border and handles, top-left compact size badge, direct manipulation with visible cursor changes. Selection border color and glow are UI only, not exported.

**Draw properties:** clicking Pencil/Arrow/Line/Shape activates it; compact contextual popover offers appropriate color/width. It may float near the tool even if it temporarily covers captured pixels while editing, but must be dismissible, movable/repositioned at edges and completely excluded from export. Popovers should not remain visible once the operation completes.

**Text properties:** click Text -> click selected image -> immediately type. In-place editor supports font family, numeric font size, color, weight, alignment, optional background and outline; edit/move existing text directly. This experience must be materially better than Lightshot's small, awkward default text.

**Arrow default:** medium-bold smooth shaft and attractive proportional head. Slight controlled glow around colored stroke in **output image** by default, adjustable/off. Preview must faithfully match export.

**Palette:** preset swatches, recent color, numeric/HEX entry (and eyedropper if feasible). Stroke sizes available by direct slider and numeric value, not only unlabelled dots.

**Keyboard:** users can tab/focus controls; shortcuts should not unexpectedly end active text editing. Tooltips show both action and binding.

**Success feedback:** on Copy optionally a quiet transient non-focus-stealing "Скопировано" notification; on Save show short confirmation only after actual disk success, both excluded from capture.

## 7. Anti-patterns to avoid

- Lower bar overlaps the selected image, or right bar ends well above the lower bar.
- Two panels touch/fuse awkwardly or do not align on their bottom edges.
- Undo/Redo/Print/More are shifted to the distant left, detached from Copy/Save.
- Huge Copy/Save blocks, stretched spacer, excessive glow, overlong labels.
- Multiple blue tool buttons falsely suggesting multiple active tools.
- A toolbar where selected, disabled and hovered states are indistinguishable.
- Giant fixed settings dialogs for adjusting text, or text that cannot be reopened.
- UI decorations accidentally baked into exported images.

## 8. Visual acceptance procedure

Codex should provide **real rendered Windows screenshots**, not only AI-generated concept art: capture-without-annotation, active Pencil/Arrow/Text, text editing, color/thickness popover, small selection near each corner, mixed-DPI monitors and compact viewport. Compare these against this design contract and iterate before calling the UI finished.

The user's generated mockups from the chat are conceptual references only; rendering and geometry of the actual implementation take precedence.
