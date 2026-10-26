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
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; Theme.Install(app);
        var timer = Stopwatch.StartNew(); Overlay? overlay = null; int code = 0;
        try
        {
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
            Check("actual-wpf-overlay-render", () => Rendering.Save(overlay.RenderUi(), Path.Combine(output, "application-ui.png")));
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
            overlay.Close(); overlay = null; Pump();
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
                elapsedMs = timer.Elapsed.TotalMilliseconds, results,
                limitations = "Automated Windows runner smoke, not Windows 11 manual acceptance; synthetic desktop in UI PNGs. No Lightshot performance comparison."
            }, new JsonSerializerOptions { WriteIndented = true }));
            app.Shutdown();
        }
        return code;
    }
    internal static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Pump() { var frame = new DispatcherFrame(); Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, () => frame.Continue = false); Dispatcher.PushFrame(frame); }
}
