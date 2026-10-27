using System.Diagnostics;
using System.Text.Json;
using System.Windows.Interop;
using System.Windows.Threading;

namespace LumenSnap;

// Opt-in diagnostics used only in an explicitly invoked --smoke run. Never starts on ordinary launch.
internal static class Smoke
{
    internal static int Run(string output)
    {
        Directory.CreateDirectory(output);
        var results = new List<object>();
        void Check(string name, Action body)
        {
            var watch = Stopwatch.StartNew();
            try { body(); results.Add(new { name, status = "PASS", milliseconds = watch.Elapsed.TotalMilliseconds }); }
            catch (Exception e) { results.Add(new { name, status = "FAIL", error = e.ToString() }); throw; }
        }
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        var timer = Stopwatch.StartNew(); Overlay? overlay = null; int code = 0;
        try
        {
            // Force motion in diagnostics so the animated template is tested even on a runner
            // with OS animations disabled. Normal launch follows the user's Windows preference.
            Check("theme-load", () => Theme.Install(app, true));
            var desktop = Native.Desktop; BitmapSource? native = null;
            Check("native-desktop-capture", () => { native = Native.Capture(desktop); Require(native.PixelWidth == desktop.Width && native.PixelHeight == desktop.Height, "Desktop pixel bounds"); });
            Check("native-hotkey-collision-preserves-old-binding", () =>
            {
                using var hwnd = new HwndSource(new HwndSourceParameters("LumenSnap smoke hotkeys"));
                using var first = new Hotkey(hwnd); using var second = new HwndSource(new HwndSourceParameters("LumenSnap conflict"));
                Require(first.TrySet(0x78, 3), "Ctrl+Alt+F9 registration");
                Require(Native.RegisterHotKey(second.Handle, 77, 3, 0x79), "Conflict fixture");
                try { Require(!first.TrySet(0x79, 3), "Conflicting key rejected"); Require(!Native.RegisterHotKey(second.Handle, 78, 3, 0x78), "Old binding still reserved"); }
                finally { Native.UnregisterHotKey(second.Handle, 77); Native.UnregisterHotKey(second.Handle, 78); }
            });
            // Synthetic background makes compositor assertions independent of runner wallpaper or private data.
            var drawing = new DrawingVisual();
            using (var dc = drawing.RenderOpen())
            {
                dc.DrawRectangle(Rendering.Brush("#263B51"), null, new Rect(0, 0, 1280, 900));
                dc.DrawRectangle(Rendering.Brush("#EAF3FF"), null, new Rect(100, 100, 325, 400));
                dc.DrawRectangle(Rendering.Brush("#263B51"), null, new Rect(425, 100, 325, 400));
            }
            var fixture = new RenderTargetBitmap(1280, 900, 96, 96, PixelFormats.Pbgra32); fixture.Render(drawing); fixture.Freeze();
            overlay = new Overlay(fixture, new Rect(0, 0, 1280, 900), new Settings(), _ => { });
            overlay.Show(); Pump(); overlay.PrepareSmokeScene(); Pump();
            Check("actual-wpf-overlay-render", () => { Rendering.Save(overlay.RenderUi(), Path.Combine(output, "application-ui.png")); Rendering.Save(overlay.ToolbarPreview(), Path.Combine(output, "toolbar-rest-ui.png")); });
            Check("selection-export-and-unicode-formats", () =>
            {
                var image = overlay.Export(); Require(image.PixelWidth == 650 && image.PixelHeight == 400, "Exact crop dimensions");
                foreach (var format in new[] { "png", "jpg", "bmp" })
                {
                    string file = Path.Combine(output, "Скриншот." + format); Rendering.Save(image, file);
                    var reload = new BitmapImage(); reload.BeginInit(); reload.CacheOption = BitmapCacheOption.OnLoad; reload.UriSource = new Uri(file); reload.EndInit();
                    Require(reload.PixelWidth == 650 && reload.PixelHeight == 400, "Round trip " + format);
                }
                var crop = Rendering.Export(fixture, new Rect(100, 100, 650, 400), []);
                byte[] pixel = new byte[4]; crop.CopyPixels(new Int32Rect(0, 0, 1, 1), pixel, 4, 0);
                Require(pixel[0] == 255 && pixel[1] == 243 && pixel[2] == 234, "No selection glow, UI border or dimming in output");
            });
            Check("native-image-clipboard-roundtrip", () => { Clipboard.SetImage(overlay.Export()); var read = Clipboard.GetImage(); Require(read != null && read.PixelWidth == 650 && read.PixelHeight == 400, "CF_BITMAP round trip"); });
            Check("in-place-cyrillic-text-editor-render", () => { overlay.PrepareSmokeScene(true); Pump(); Require(Keyboard.FocusedElement is TextBox, "TextBox receives keyboard focus"); Rendering.Save(overlay.RenderUi(), Path.Combine(output, "text-editor-ui.png")); });
            Check("native-text-copy-retains-capture-and-Escape-commits-once", () =>
            {
                var text = (TextBox)Keyboard.FocusedElement; text.Select(0, 6);
                ApplicationCommands.Copy.Execute(null, text);
                Require(Clipboard.GetText() == "Привет", "Native TextBox copy preserves Cyrillic characters");
                Require(overlay.IsVisible && overlay.Document.Items.Length == 3, "Text copy does not rasterize, close or duplicate objects");
                var escape = new KeyEventArgs(Keyboard.PrimaryDevice, PresentationSource.FromVisual(overlay), 0, Key.Escape) { RoutedEvent = Keyboard.PreviewKeyDownEvent };
                overlay.RaiseEvent(escape); Pump();
                Require(escape.Handled && overlay.IsVisible && Keyboard.FocusedElement is not TextBox, "First Escape leaves text editing only");
                Require(overlay.Document.Items.Length == 3, "Re-editing commits one existing object");
            });
            Check("contextual-text-properties-render", () =>
            {
                var content = overlay.OpenPropertiesForSmoke(); Pump();
                Rendering.Save(RenderElement(content), Path.Combine(output, "text-properties-ui.png"));
                Require(content.ActualWidth <= 240 && content.ActualHeight <= 400, "Compact text properties fit without oversized cards");
                var palette = Descendants<ColorPalette>(content).Single();
                Require(palette.Children.Count == 10, "Ten named basic colors");
                Require(!Descendants<TextBox>(content).Any(x => x.Text.StartsWith('#')), "No HEX input in tool properties");
                foreach (Button swatch in palette.Children)
                { swatch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Require(overlay.Document.Items.Last().Color == palette.SelectedColor, "Palette updates actual editable text"); }
                ((Button)palette.Children[0]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                var fonts = Descendants<ComboBox>(content).First(x => x.IsEditable);
                Require(fonts.Text == "Segoe UI", "Editable font selector retains selected value");
                fonts.IsDropDownOpen = true; Pump(); Require(fonts.IsDropDownOpen && fonts.Items.Count > 0, "Native themed font dropdown opens");
                fonts.IsDropDownOpen = false;
                var scroll = Descendants<ScrollViewer>(content).First(); scroll.ScrollToBottom(); Pump();
                Require(scroll.ScrollableHeight == 0 || scroll.VerticalOffset > 0, "Text formatting controls remain reachable by scrolling");
                Rendering.Save(RenderElement(content), Path.Combine(output, "text-properties-bottom-ui.png"));
                overlay.ClosePropertiesForSmoke(); Pump();
            });
            Check("settings-window-structure-and-render", () =>
            {
                var window = new SettingsWindow(new Settings(), _ => throw new InvalidOperationException("Smoke must not change settings"));
                try
                {
                    window.Show(); Pump();
                    var root = (FrameworkElement)window.Content;
                    Require(window.Width == 440 && Descendants<ColorPalette>(window).Single().Children.Count == 10, "Compact settings use the same ten-color palette");
                    Require(!Descendants<TextBox>(window).Any(x => x.Text.StartsWith('#')), "No HEX input in settings");
                    var save = Descendants<Button>(window).Single(x => Equals(x.Content, "Сохранить"));
                    var saveBounds = save.TransformToAncestor(root).TransformBounds(new Rect(0, 0, save.ActualWidth, save.ActualHeight));
                    Require(save.IsVisible && new Rect(0, 0, root.ActualWidth, root.ActualHeight).Contains(saveBounds), "Save is fully within the visible settings client");
                    // Render the window root: rendering its inset Content alone clips the footer
                    // and omits the actual window background from the evidence bitmap.
                    Rendering.Save(RenderElement(window), Path.Combine(output, "settings-ui.png"));
                    var scroll = Descendants<ScrollViewer>(window).First(); scroll.ScrollToBottom(); Pump();
                    Require(scroll.ScrollableHeight == 0 || scroll.VerticalOffset > 0, "Settings content scrolls to the lower sections");
                    Rendering.Save(RenderElement(window), Path.Combine(output, "settings-bottom-ui.png"));
                }
                finally { window.Close(); overlay.Activate(); Pump(); }
            });
            Check("native-active-button-hover-press-and-keyboard-focus", () =>
            {
                var button = overlay.ActiveButton; button.ApplyTemplate();
                var chrome = (Border)button.Template.FindName("Chrome", button);
                Native.GetCursorPos(out var previous);
                try
                {
                    Native.SetCursorPos(15, 80); PumpInput();
                    string rest = chrome.Background.ToString();
                    var center = button.PointToScreen(new Point(button.ActualWidth / 2, button.ActualHeight / 2));
                    Require(Native.SetCursorPos((int)center.X, (int)center.Y), "Move runner pointer to active tool"); PumpInput();
                    Require(button.IsMouseOver && chrome.Background.ToString() != rest, "Selected tool shows real hover feedback");
                    var glow = (Border)button.Template.FindName("Glow", button);
                    Require(glow.Opacity > 0 && DependencyPropertyHelper.GetValueSource(glow, UIElement.OpacityProperty).IsAnimated, "Hover runs a real glow animation");
                    string hover = chrome.Background.ToString(); Rendering.Save(overlay.RenderUi(), Path.Combine(output, "button-hover-ui.png"));
                    Rendering.Save(overlay.ToolbarPreview(), Path.Combine(output, "toolbar-hover-ui.png"));
                    Native.mouse_event(2, 0, 0, 0, UIntPtr.Zero); PumpInput();
                    Require(button.IsPressed && chrome.Background.ToString() != hover, "Real mouse-down changes button chrome");
                    var motion = (FrameworkElement)button.Template.FindName("Motion", button);
                    Require(((ScaleTransform)motion.RenderTransform).ScaleX < .99, "Press runs a scale animation");
                    Rendering.Save(overlay.RenderUi(), Path.Combine(output, "button-pressed-ui.png"));
                    Rendering.Save(overlay.ToolbarPreview(), Path.Combine(output, "toolbar-pressed-ui.png"));
                    // Release outside to avoid opening the properties popup while testing the button state.
                    Native.SetCursorPos(15, 80); Native.mouse_event(4, 0, 0, 0, UIntPtr.Zero); PumpInput();
                    button.Focus(); Pump(); Require(button.IsKeyboardFocused, "Toolbar supports keyboard focus");
                }
                finally { Native.mouse_event(4, 0, 0, 0, UIntPtr.Zero); Native.SetCursorPos(previous.X, previous.Y); }
            });
            Check("full-screen-panels-inside-with-original-pixel-export", () =>
            {
                overlay.PrepareFullScreenForSmoke(); Pump();
                var view = overlay.DisplayWorkspace;
                Require(view.Zoom == 1 && view.Panels.Inside && view.Panels.Visible, "Full screen keeps unscaled scene and internal panels");
                Require(overlay.Selection.Contains(view.Panels.Vertical) && overlay.Selection.Contains(view.Panels.Horizontal), "Panels are fully within the selected region");
                Rendering.Save(overlay.RenderUi(), Path.Combine(output, "full-screen-ui.png"));
                var exported = overlay.Export(); Require(exported.PixelWidth == 1280 && exported.PixelHeight == 900, "Full-screen export preserves original dimensions");
                var point = view.Panels.Horizontal.BottomRight - new Vector(8, 8);
                var pixel = new byte[4]; exported.CopyPixels(new Int32Rect((int)point.X, (int)point.Y, 1, 1), pixel, 4, 0);
                Require(pixel[0] == 0x51 && pixel[1] == 0x3B && pixel[2] == 0x26, "Internal toolbar does not contaminate exported pixels");
                overlay.RestoreRegionForSmoke(); Pump();
            });
            Check("compact-toolbar-and-close-after-save-cancels-capture", () =>
            {
                var button = overlay.CloseButton; var row = (StackPanel)button.Parent;
                Require(Equals(((Button)row.Children[^2]).Content, "Сохранить") && Equals(button.Content, "Закрыть"), "Close immediately follows Save");
                Require(Math.Abs(button.ActualHeight - 40 * 2d / 3) < .01, "Toolbar buttons are 1.5 times smaller");
                bool completed = false; overlay.Completed += _ => completed = true;
                button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); Pump();
                Require(!overlay.IsVisible && !completed, "Close cancels capture without copy/save completion");
            });
            overlay = null; Pump();
            Check("real-desktop-overlay-open-close", () =>
            {
                for (int i = 0; i < 5; i++)
                { var w = new Overlay(native!, desktop, new Settings(), _ => { }); w.Show(); Pump(); w.Close(); Pump(); }
                Require(app.Windows.Count == 0, "No overlay window leaked");
            });
        }
        catch { code = 1; }
        finally
        {
            overlay?.Close();
            File.WriteAllText(Path.Combine(output, "smoke.json"), JsonSerializer.Serialize(new
            {
                source = typeof(Smoke).Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyInformationalVersionAttribute), false).Cast<System.Reflection.AssemblyInformationalVersionAttribute>().FirstOrDefault()?.InformationalVersion,
                os = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                animationTestMode = "Motion explicitly enabled in smoke only; normal launch honors Windows client-area animation preference",
                elapsedMs = timer.Elapsed.TotalMilliseconds, results,
                limitations = "Automated Windows runner smoke, not Windows 11 manual acceptance; synthetic desktop in UI PNGs. No Lightshot performance comparison."
            }, new JsonSerializerOptions { WriteIndented = true }));
            app.Shutdown();
        }
        return code;
    }
    internal static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        { var child = VisualTreeHelper.GetChild(parent, i); if (child is T match) yield return match; foreach (var nested in Descendants<T>(child)) yield return nested; }
    }
    private static BitmapSource RenderElement(FrameworkElement element)
    {
        element.UpdateLayout(); var bitmap = new RenderTargetBitmap((int)Math.Ceiling(element.ActualWidth), (int)Math.Ceiling(element.ActualHeight), 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(element); bitmap.Freeze(); return bitmap;
    }
    private static void PumpInput()
    {
        var frame = new DispatcherFrame(); var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(80) };
        timer.Tick += (_, _) => { timer.Stop(); frame.Continue = false; }; timer.Start(); Dispatcher.PushFrame(frame);
    }
    private static void Pump() { var frame = new DispatcherFrame(); Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, () => frame.Continue = false); Dispatcher.PushFrame(frame); }
}
