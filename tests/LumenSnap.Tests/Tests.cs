using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LumenSnap;

internal static class Tests
{
    private static int failed, passed;
    private static void Test(string name, Action body)
    {
        try { body(); passed++; Console.WriteLine("PASS " + name); }
        catch (Exception e) { failed++; Console.WriteLine("FAIL " + name + ": " + e); }
    }
    private static void Assert(bool condition, string message = "Assertion failed") { if (!condition) throw new Exception(message); }
    [STAThread]
    private static int Main()
    {
        Test("selection reverse drag clips to bounds", () => Assert(Coordinates.Region(new(300, 240), new(-50, -70), new(0, 0, 500, 400)) == new Rect(0, 0, 300, 240)));
        Test("selection negative desktop origin", () => Assert(Coordinates.Region(new(-1900, -100), new(-1600, 300), new(-1920, -200, 3840, 1280)) == new Rect(-1900, -100, 300, 400)));
        Test("selection move clamps every boundary", () => { var r = Coordinates.Move(new(20, 30, 100, 60), new(-999, 999), new(0, 0, 200, 200)); Assert(r == new Rect(0, 140, 100, 60)); });
        Test("subpixel crop rounds outwards", () => Assert(Coordinates.Crop(new(2.2, 3.7, 10.1, 8.1), 100, 100) == new Int32Rect(2, 3, 11, 9)));
        Test("minimum crop stays nonempty", () => Assert(Coordinates.Crop(new(99, 99, .01, .01), 100, 100) == new Int32Rect(99, 99, 1, 1)));
        foreach (double dpi in new[] { 96d, 120, 144, 192 }) Test("DPI round trip " + dpi, () => { var p = new Point(-1920, 371); Assert((Coordinates.Pixels(Coordinates.Dips(p, dpi), dpi) - p).Length < .00001); });
        Test("aligned compact outside panels", () => { var s = new Rect(100, 100, 650, 400); var p = PanelLayout.Place(s, new(0, 0, 1280, 900)); Assert(p.Visible); Assert(p.Vertical.Bottom == p.Horizontal.Bottom); Assert(p.Vertical.Left - p.Horizontal.Right == 6); Assert(!p.Vertical.IntersectsWith(s) && !p.Horizontal.IntersectsWith(s)); });
        Test("panel edge fallback is visible and outside", () => { var s = new Rect(600, 400, 650, 400); var p = PanelLayout.Place(s, new(0, 0, 1280, 900)); Assert(p.Visible); Assert(!p.Vertical.IntersectsWith(s) && !p.Horizontal.IntersectsWith(s)); });
        Test("full-desktop impossible geometry is explicit", () => Assert(!PanelLayout.Place(new(0, 0, 1280, 900), new(0, 0, 1280, 900)).Visible));
        Test("panels never silently overlap for randomized selections", () =>
        {
            var random = new Random(42); var screen = new Rect(0, 0, 3840, 2160);
            for (int i = 0; i < 3000; i++)
            {
                var s = new Rect(random.Next(0, 3000), random.Next(0, 1500), random.Next(1, 840), random.Next(1, 660));
                var p = PanelLayout.Place(s, screen, new[] { 1d, 1.25, 1.5, 2 }[i % 4]);
                if (p.Visible) Assert(screen.Contains(p.Vertical) && screen.Contains(p.Horizontal) && !s.IntersectsWith(p.Vertical) && !s.IntersectsWith(p.Horizontal) && !p.Vertical.IntersectsWith(p.Horizontal));
            }
        });
        Test("document create style move delete undo redo", () =>
        {
            var d = new Document(); var a = new Annotation { Tool = Tool.Text, Text = "Привет", Box = new(10, 20, 200, 80) };
            d.Put(a); d.Put(a with { Bold = true }); d.Put(d.Items[0].Move(new(30, 40))); d.Delete(a.Id);
            Assert(d.Items.Length == 0); d.Undo(); Assert(d.Items[0].Box.X == 40); d.Undo(); Assert(d.Items[0].Bold); d.Undo(); Assert(!d.Items[0].Bold); d.Redo(); Assert(d.Items[0].Bold);
            d.Put(d.Items[0] with { Text = "Изменено" }); Assert(!d.CanRedo); Assert(d.Items[0].Id == a.Id);
        });
        Test("resize preserves font size and transforms geometry", () => { var a = new Annotation { Box = new(10, 10, 100, 50), Points = [new(10, 10), new(110, 60)], FontSize = 26 }; var b = a.Resize(new(20, 20, 200, 100)); Assert(b.Points[1] == new Point(220, 120) && b.FontSize == 26 && a.Points[1] == new Point(110, 60)); });
        Test("text copy undo and navigation remain native", () => { foreach (var key in new[] { Key.C, Key.Z, Key.Y, Key.Delete, Key.Left }) Assert(ShortcutPolicy.Resolve(key, ModifierKeys.Control, true, true) == CaptureCommand.None); });
        Test("context Escape hierarchy", () => { Assert(ShortcutPolicy.Resolve(Key.Escape, ModifierKeys.None, true, true) == CaptureCommand.TextEscape); Assert(ShortcutPolicy.Resolve(Key.Escape, ModifierKeys.None, false, false) == CaptureCommand.Cancel); });
        Test("capture copy save redo shortcuts", () => { Assert(ShortcutPolicy.Resolve(Key.C, ModifierKeys.Control, false, false) == CaptureCommand.Copy); Assert(ShortcutPolicy.Resolve(Key.S, ModifierKeys.Control, true, true) == CaptureCommand.Save); Assert(ShortcutPolicy.Resolve(Key.Z, ModifierKeys.Control | ModifierKeys.Shift, false, false) == CaptureCommand.Redo); });
        Test("corrupt and future settings recover defaults", () => WithTemp(dir => { string file = Path.Combine(dir, "settings.json"); File.WriteAllText(file, "{broken"); Assert(Settings.Load(file) == new Settings()); File.WriteAllText(file, "{\"Schema\":99}"); Assert(Settings.Load(file) == new Settings()); File.WriteAllText(file, "{\"Color\":\"nope\"}"); Assert(Settings.Load(file) == new Settings()); }));
        Test("settings Unicode roundtrip with safe defaults", () => WithTemp(dir => { string file = Path.Combine(dir, "settings.json"); var s = new Settings { LastFolder = "C:\\Снимки", Font = "Segoe UI", Width = 6 }; s.Save(file); Assert(Settings.Load(file) == s); Assert(!s.StartAtLogin); }));
        Test("export exact pixels excludes overlay chrome", () =>
        {
            var pixels = Enumerable.Repeat((byte)255, 32 * 32 * 4).ToArray(); var image = BitmapSource.Create(32, 32, 96, 96, PixelFormats.Bgra32, null, pixels, 128);
            var output = Rendering.Export(image, new(4, 5, 12, 10), []); byte[] result = new byte[12 * 10 * 4]; output.CopyPixels(result, 48, 0);
            Assert(output.PixelWidth == 12 && output.PixelHeight == 10 && result.All(b => b == 255));
        });
        Test("arrow glow rasterizes into export", () =>
        {
            var black = BitmapSource.Create(100, 100, 96, 96, PixelFormats.Bgr32, null, new byte[40000], 400);
            var arrow = new Annotation { Tool = Tool.Arrow, Box = new(20, 50, 60, 0), Points = [new(20, 50), new(80, 50)], Width = 4, Glow = true };
            byte[] p = new byte[40000], q = new byte[40000]; Rendering.Export(black, new(0, 0, 100, 100), [arrow]).CopyPixels(p, 400, 0);
            Rendering.Export(black, new(0, 0, 100, 100), [arrow with { Glow = false }]).CopyPixels(q, 400, 0);
            Assert(p[46 * 400 + 40 * 4] > q[46 * 400 + 40 * 4], "Glow outside shaft must be exported");
        });
        Test("failed save preserves original file", () => WithTemp(dir =>
        {
            string file = Path.Combine(dir, "original.png"); File.WriteAllText(file, "keep");
            var black = BitmapSource.Create(2, 2, 96, 96, PixelFormats.Bgr32, null, new byte[16], 8);
            using (var locked = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None))
            { bool failed = false; try { Rendering.Save(black, file); } catch (IOException) { failed = true; } Assert(failed); }
            Assert(File.ReadAllText(file) == "keep"); Assert(Directory.GetFiles(dir).Length == 1);
        }));
        Test("object geometry hit and z-order", () => { var d = new Document(); var a = new Annotation { Tool = Tool.Line, Points = [new(10, 10), new(100, 10)], Box = new(10, 10, 90, 0) }; d.Put(a); var b = a with { Id = Guid.NewGuid(), Color = "#FFFFFF" }; d.Put(b); Assert(d.Items.Last(x => Rendering.Hit(x, new(50, 12))).Id == b.Id); Assert(!Rendering.Hit(a, new(50, 60))); });
        Console.WriteLine($"RESULT passed={passed} failed={failed}"); return failed == 0 ? 0 : 1;
    }
    private static void WithTemp(Action<string> body)
    { string dir = Path.Combine(Path.GetTempPath(), "LumenSnap-tests-" + Guid.NewGuid()); Directory.CreateDirectory(dir); try { body(dir); } finally { Directory.Delete(dir, true); } }
}
