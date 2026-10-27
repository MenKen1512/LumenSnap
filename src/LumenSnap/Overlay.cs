using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;

namespace LumenSnap;

internal sealed class Overlay : Window
{
    private readonly Canvas canvas = new() { Background = Brushes.Transparent };
    private readonly Scene scene;
    private readonly BitmapSource frozen;
    private readonly Rect desktop;
    private readonly Document document = new();
    private readonly Action<Settings> settingsChanged;
    private Settings settings;
    private readonly Border vertical, horizontal;
    private readonly Button undo, redo, closeCapture;
    private readonly System.Windows.Shapes.Ellipse colorPreview = new() { Width = 14, Height = 14, Stroke = Theme.Foreground, StrokeThickness = 1 };
    private Tool? paintedTool;
    private string? displayedColor;
    private static readonly Brush ActiveTool = Rendering.Brush("#305B7E"), PrimaryTool = Rendering.Brush("#20364C"), SecondaryTool = Rendering.Brush("#1B2A3B");
    private readonly Dictionary<Tool, Button> tools = [];
    private readonly TextBlock status = new() { Foreground = Brushes.White, Background = Theme.Panel, Padding = new Thickness(8), TextWrapping = TextWrapping.Wrap, MaxWidth = 650 };
    private Popup? properties;
    private TextBox? editor;
    private Annotation? textOriginal, selected, draft, dragOriginal;
    private readonly List<Point> stroke = [];
    private Rect selection = Rect.Empty, originalSelection;
    private Point start;
    private string? drag;
    private Tool tool = Tool.Arrow;
    private Annotation defaults = new();
    private readonly Dictionary<Tool, Annotation> toolDefaults = [];
    private double uiScale = 1;
    private Workspace workspace = new(1, new Vector(), default);
    private bool closing;
    internal Document Document => document;
    internal Rect Selection => selection;
    internal event Action<string>? Completed;

    internal Overlay(BitmapSource image, Rect desktop, Settings settings, Action<Settings> settingsChanged)
    {
        this.frozen = image; this.desktop = desktop; this.settings = settings; this.settingsChanged = settingsChanged;
        defaults = new Annotation { Color = settings.Color, Width = settings.Width, Font = settings.Font, FontSize = settings.FontSize, Glow = settings.Glow, Background = true };
        Title = "LumenSnap — Захват"; WindowStyle = WindowStyle.None; ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false; Topmost = true; Background = Brushes.Black; Content = canvas;
        canvas.Width = image.PixelWidth; canvas.Height = image.PixelHeight;
        scene = new Scene(this) { Width = image.PixelWidth, Height = image.PixelHeight, Focusable = true };
        canvas.Children.Add(scene);
        var v = new StackPanel();
        var names = new[] { "Карандаш / маркер", "Стрелка", "Текст", "Линия", "Прямоугольник", "Эллипс" };
        for (int i = 0; i < 6; i++)
        {
            var t = (Tool)i;
            var b = Theme.Toolbar(Theme.Icon(t.ToString()), names[i] + "; повторное нажатие — свойства", () => SelectTool(t));
            tools.Add(t, b); v.Children.Add(b);
        }
        colorPreview.Fill = Rendering.Brush(defaults.Color);
        var options = Theme.Toolbar(colorPreview, "Цвет и параметры", ShowProperties); v.Children.Add(options);
        vertical = Theme.Surface(v); vertical.Width = ToolbarMetrics.VerticalWidth; vertical.Height = ToolbarMetrics.VerticalHeight; canvas.Children.Add(vertical);
        var h = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        undo = Theme.Toolbar(Theme.Icon("Undo"), "Отменить · Ctrl+Z", Undo); redo = Theme.Toolbar(Theme.Icon("Redo"), "Повторить · Ctrl+Y", Redo);
        h.Children.Add(undo); h.Children.Add(redo); h.Children.Add(Theme.Toolbar(Theme.Icon("Print"), "Печать · Ctrl+P", Print));
        foreach (var b in new[] { Theme.Toolbar("Копировать", "Копировать · Ctrl+C", Copy, 118), Theme.Toolbar("Сохранить", "Сохранить как… · Ctrl+S", Save, 118) })
        { b.Background = Rendering.Brush("#284F76"); b.BorderBrush = Theme.Accent; h.Children.Add(b); }
        closeCapture = Theme.Toolbar("Закрыть", "Закрыть захват · Esc", Close, 70); h.Children.Add(closeCapture);
        horizontal = Theme.Surface(h); horizontal.Width = ToolbarMetrics.HorizontalWidth; horizontal.Height = ToolbarMetrics.HorizontalHeight; canvas.Children.Add(horizontal);
        canvas.Children.Add(status); Canvas.SetLeft(status, 16); Canvas.SetTop(status, 16);
        scene.MouseLeftButtonDown += MouseDownOnImage; scene.MouseMove += MouseMoveOnImage; scene.MouseLeftButtonUp += MouseUpOnImage;
        scene.MouseRightButtonDown += (_, e) => { CommitText(); properties?.SetCurrentValue(Popup.IsOpenProperty, false); selected = document.Items.LastOrDefault(x => Rendering.Hit(x, e.GetPosition(scene))); Refresh(); e.Handled = true; };
        PreviewKeyDown += KeyPressed;
        SourceInitialized += (_, _) =>
        {
            var source = (HwndSource)PresentationSource.FromVisual(this); source.AddHook(WindowMessage);
            PositionWindow();
        };
        Loaded += (_, _) => { PositionWindow(); scene.Focus(); Activate(); Refresh(); };
        Closed += (_, _) => { closing = true; if (properties != null) properties.IsOpen = false; };
        Refresh();
    }

    private IntPtr WindowMessage(IntPtr h, int msg, IntPtr w, IntPtr l, ref bool handled)
    {
        if (msg == 0x2E0) { handled = true; Dispatcher.BeginInvoke(PositionWindow); }
        // A topology change invalidates frozen desktop coordinates; cancel rather than exporting misaligned pixels.
        if (msg == 0x7E) Dispatcher.BeginInvoke(Close);
        return IntPtr.Zero;
    }
    private void PositionWindow()
    {
        if (closing) return;
        var hwnd = new WindowInteropHelper(this).Handle;
        Native.SetWindowPos(hwnd, new IntPtr(-1), (int)desktop.X, (int)desktop.Y, (int)desktop.Width, (int)desktop.Height, 0x0040);
        double dpi = Math.Max(96, Native.GetDpiForWindow(hwnd));
        canvas.LayoutTransform = new ScaleTransform(96 / dpi, 96 / dpi);
    }
    private Rect ImageBounds => new(0, 0, frozen.PixelWidth, frozen.PixelHeight);
    private Point Clamp(Point p) => new(Math.Clamp(p.X, 0, frozen.PixelWidth), Math.Clamp(p.Y, 0, frozen.PixelHeight));

    private void SelectTool(Tool next)
    {
        CommitText();
        if (tool == next) { ShowProperties(); return; }
        toolDefaults[tool] = defaults;
        defaults = toolDefaults.GetValueOrDefault(next, defaults);
        tool = next; selected = null; Refresh(); scene.Focus();
    }
    private void MouseDownOnImage(object sender, MouseButtonEventArgs e)
    {
        CommitText(); if (properties != null) properties.IsOpen = false;
        var p = Clamp(e.GetPosition(scene)); start = p; originalSelection = selection; dragOriginal = selected;
        scene.Focus(); e.Handled = true;
        // Selection handles have priority over the drawing tool. Space+drag repositions the region.
        string? handle = SelectionHandle(p);
        if (handle != null) drag = "region:" + handle;
        else if (!selection.IsEmpty && Keyboard.IsKeyDown(Key.Space) && selection.Contains(p)) drag = "move-region";
        else if (selected != null && new Rect(selected.Box.BottomRight - new Vector(8, 8), new Size(16, 16)).Contains(p)) drag = "resize-object";
        else
        {
            var hit = document.Items.LastOrDefault(x => Rendering.Hit(x, p));
            if (hit != null && selection.Contains(p))
            {
                selected = hit; dragOriginal = hit;
                if (hit.Tool == Tool.Text && e.ClickCount == 2) { BeginText(hit); return; }
                drag = "move-object";
            }
            else if (selection.IsEmpty || !selection.Contains(p)) { selected = null; drag = "select"; selection = new Rect(p, p); }
            else if (tool == Tool.Text)
            {
                var a = defaults with { Id = Guid.NewGuid(), Tool = Tool.Text,
                    Box = new Rect(p, new Size(Math.Max(40, Math.Min(360, selection.Right - p.X)), Math.Max(35, Math.Min(160, selection.Bottom - p.Y)))) };
                BeginText(a); return;
            }
            else
            {
                selected = null; drag = "draw"; stroke.Clear(); stroke.Add(p);
                draft = defaults with { Id = Guid.NewGuid(), Tool = tool, Points = [p, p], Box = new Rect(p, p) };
            }
        }
        scene.CaptureMouse(); Refresh();
    }
    private void MouseMoveOnImage(object sender, MouseEventArgs e)
    {
        Point p = Clamp(e.GetPosition(scene));
        if (drag == null)
        {
            scene.Cursor = SelectionHandle(p) != null ? Cursors.SizeAll : Keyboard.IsKeyDown(Key.Space) ? Cursors.Hand : Cursors.Cross;
            return;
        }
        Vector delta = p - start;
        if (drag == "select") selection = Coordinates.Region(start, p, ImageBounds);
        else if (drag == "move-region") selection = Coordinates.Move(originalSelection, delta, ImageBounds);
        else if (drag.StartsWith("region:"))
        {
            string handle = drag[7..];
            double l = originalSelection.Left, t = originalSelection.Top, r = originalSelection.Right, b = originalSelection.Bottom;
            if (handle.Contains('l')) l = Math.Min(p.X, r - 1);
            if (handle.Contains('r')) r = Math.Max(p.X, l + 1);
            if (handle.Contains('t')) t = Math.Min(p.Y, b - 1);
            if (handle.Contains('b')) b = Math.Max(p.Y, t + 1);
            selection = Coordinates.Region(new Point(l, t), new Point(r, b), ImageBounds);
        }
        else if (drag == "move-object" && dragOriginal != null) selected = dragOriginal.Move(delta);
        else if (drag == "resize-object" && dragOriginal != null)
            selected = dragOriginal.Resize(new Rect(dragOriginal.Box.TopLeft, new Size(Math.Max(16, p.X - dragOriginal.Box.X), Math.Max(16, p.Y - dragOriginal.Box.Y))));
        else if (drag == "draw" && draft != null)
        {
            p = new Point(Math.Clamp(p.X, selection.Left, selection.Right), Math.Clamp(p.Y, selection.Top, selection.Bottom));
            if (tool == Tool.Pencil)
            {
                if ((p - stroke[^1]).Length >= 1.5) stroke.Add(p);
                var bounds = new Rect(stroke[0], stroke[0]); foreach (var point in stroke) bounds.Union(point);
                draft = draft with { Points = stroke.ToArray(), Box = bounds };
            }
            else draft = draft with { Points = [start, p], Box = new Rect(start, p) };
        }
        Refresh();
    }
    private void MouseUpOnImage(object sender, MouseButtonEventArgs e)
    {
        if (drag == null) return;
        if (drag == "draw" && draft != null && (draft.Box.Width > 1 || draft.Box.Height > 1)) { document.Put(draft); selected = draft; }
        else if (drag is "move-object" or "resize-object" && selected != null) document.Put(selected);
        if (selection.Width < 1 || selection.Height < 1) selection = Rect.Empty;
        draft = null; drag = null; dragOriginal = null; scene.ReleaseMouseCapture(); Refresh();
    }
    private IEnumerable<(string name, Point point)> Handles()
    {
        if (selection.IsEmpty) yield break;
        double x = selection.X, y = selection.Y, r = selection.Right, b = selection.Bottom, mx = (x + r) / 2, my = (y + b) / 2;
        yield return ("lt", new(x, y)); yield return ("t", new(mx, y)); yield return ("rt", new(r, y));
        yield return ("r", new(r, my)); yield return ("rb", new(r, b)); yield return ("b", new(mx, b));
        yield return ("lb", new(x, b)); yield return ("l", new(x, my));
    }
    private string? SelectionHandle(Point p) => Handles().Where(h => (h.point - p).Length <= 7 * uiScale).Select(h => h.name).FirstOrDefault();

    private void BeginText(Annotation a)
    {
        CommitText(); selected = a; textOriginal = document.Items.FirstOrDefault(x => x.Id == a.Id);
        editor = new TextBox
        {
            Text = a.Text, FontFamily = new FontFamily(a.Font), FontSize = a.FontSize, FontWeight = a.Bold ? FontWeights.Bold : FontWeights.Normal,
            Foreground = Rendering.Brush(a.Color), Background = a.Background ? new SolidColorBrush(Color.FromArgb(200, 16, 24, 38)) : Brushes.Transparent,
            CaretBrush = Theme.Foreground, SelectionBrush = Theme.Accent, SelectionOpacity = .45,
            BorderBrush = Theme.Accent, BorderThickness = new Thickness(1), Padding = new Thickness(3),
            AcceptsReturn = true, AcceptsTab = false, TextWrapping = TextWrapping.Wrap, TextAlignment = a.Alignment,
            Width = a.Box.Width, Height = a.Box.Height, Opacity = a.Opacity, VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
        PositionEditor(); canvas.Children.Add(editor);
        editor.TextChanged += (_, _) => { if (editor != null && selected != null) selected = selected with { Text = editor.Text }; };
        Refresh(); editor.Focus(); editor.CaretIndex = editor.Text.Length;
    }
    private void CommitText()
    {
        if (editor == null) return;
        if (selected != null)
        {
            var next = selected with { Text = editor.Text };
            if (next.Text.Length > 0) document.Put(next);
            else if (textOriginal != null) document.Delete(textOriginal.Id);
            selected = next.Text.Length > 0 ? next : null;
        }
        canvas.Children.Remove(editor); editor = null; textOriginal = null; scene.Focus(); Refresh();
    }
    private void PositionEditor()
    {
        if (editor == null || selected == null) return;
        Canvas.SetLeft(editor, selected.Box.X * workspace.Zoom + workspace.Offset.X);
        Canvas.SetTop(editor, selected.Box.Y * workspace.Zoom + workspace.Offset.Y);
        editor.RenderTransform = new ScaleTransform(workspace.Zoom, workspace.Zoom);
    }
    private void Undo() { CommitText(); document.Undo(); selected = null; Refresh(); }
    private void Redo() { CommitText(); document.Redo(); selected = null; Refresh(); }
    private void KeyPressed(object sender, KeyEventArgs e)
    {
        bool textFocus = Keyboard.FocusedElement is TextBoxBase || Keyboard.FocusedElement is ComboBox { IsEditable: true };
        var command = ShortcutPolicy.Resolve(e.Key == Key.System ? e.SystemKey : e.Key, Keyboard.Modifiers, textFocus, editor != null);
        switch (command)
        {
            case CaptureCommand.None: return;
            case CaptureCommand.TextEscape: CommitText(); break;
            case CaptureCommand.Cancel:
                if (properties?.IsOpen == true) { properties.IsOpen = false; scene.Focus(); } else Close(); break;
            case CaptureCommand.Copy: Copy(); break;
            case CaptureCommand.Save: Save(); break;
            case CaptureCommand.Undo: Undo(); break;
            case CaptureCommand.Redo: Redo(); break;
            case CaptureCommand.Print: Print(); break;
            case CaptureCommand.Delete: if (selected != null) { document.Delete(selected.Id); selected = null; Refresh(); } break;
        }
        e.Handled = true;
    }

    private void ChangeStyle(Func<Annotation, Annotation> change)
    {
        defaults = change(defaults);
        if (selected != null)
        {
            selected = change(selected);
            if (editor != null)
            {
                editor.FontFamily = new FontFamily(selected.Font); editor.FontSize = selected.FontSize;
                editor.FontWeight = selected.Bold ? FontWeights.Bold : FontWeights.Normal;
                editor.TextAlignment = selected.Alignment; editor.Foreground = Rendering.Brush(selected.Color);
                editor.Opacity = selected.Opacity;
                editor.Background = selected.Background ? new SolidColorBrush(Color.FromArgb(200, 16, 24, 38)) : Brushes.Transparent;
            }
            else document.Put(selected);
        }
        settings = settings with { Color = defaults.Color, Width = defaults.Width, Font = defaults.Font, FontSize = defaults.FontSize, Glow = defaults.Glow };
        settingsChanged(settings); Refresh();
    }
    private void ShowProperties()
    {
        if (properties?.IsOpen == true) { properties.IsOpen = false; return; }
        var content = new ToolProperties(selected ?? defaults, selected?.Tool ?? tool, ChangeStyle,
            () => { properties!.IsOpen = false; scene.Focus(); },
            selected?.Tool == Tool.Text && editor == null ? () => { properties!.IsOpen = false; if (selected != null) BeginText(selected); } : null)
        { MaxHeight = Math.Max(160, Math.Min(ActualHeight, SystemParameters.WorkArea.Height) - 64) };
        properties = new Popup { PlacementTarget = vertical, Placement = PlacementMode.Left, HorizontalOffset = -8, AllowsTransparency = true, StaysOpen = false, Child = content };
        properties.IsOpen = true;
    }

    internal BitmapSource Export()
    { CommitText(); return Rendering.Export(frozen, selection, document.Items); }
    private bool HasSelection => !selection.IsEmpty && selection.Width >= 1 && selection.Height >= 1;
    private void Copy()
    {
        if (!HasSelection) return;
        try { var image = Export(); Clipboard.SetImage(image); Close(); Completed?.Invoke("Скопировано"); }
        catch (Exception e) { Error("Буфер обмена занят. Повторите копирование. " + e.Message); }
    }
    private void Save()
    {
        if (!HasSelection) return;
        CommitText(); if (properties != null) properties.IsOpen = false;
        var dialog = new SaveFileDialog { Title = "Сохранить скриншот", Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg|BMP (*.bmp)|*.bmp", FilterIndex = settings.Format,
            AddExtension = true, OverwritePrompt = true, FileName = "Скриншот " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"),
            InitialDirectory = Directory.Exists(settings.LastFolder) ? settings.LastFolder : Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) };
        try
        {
            if (dialog.ShowDialog(this) != true) return;
            Rendering.Save(Export(), dialog.FileName);
            settings = settings with { LastFolder = Path.GetDirectoryName(dialog.FileName)!, Format = dialog.FilterIndex };
            settingsChanged(settings); Close(); Completed?.Invoke("Сохранено: " + dialog.FileName);
        }
        catch (Exception e) { Error("Не удалось сохранить файл. " + e.Message); }
    }
    private void Print()
    {
        if (!HasSelection) return;
        CommitText(); if (properties != null) properties.IsOpen = false;
        try
        {
            Topmost = false;
            var dialog = new PrintDialog(); if (dialog.ShowDialog() != true) return;
            var bitmap = Export(); var image = new Image { Source = bitmap, Stretch = Stretch.Uniform, Width = dialog.PrintableAreaWidth, Height = dialog.PrintableAreaHeight };
            image.Measure(new Size(image.Width, image.Height)); image.Arrange(new Rect(0, 0, image.Width, image.Height));
            dialog.PrintVisual(image, "LumenSnap"); scene.Focus();
        }
        catch (Exception e) { Error("Не удалось напечатать. " + e.Message); }
        finally { if (!closing) { Topmost = true; Activate(); scene.Focus(); } }
    }
    private void Error(string message) { status.Text = message; status.Visibility = Visibility.Visible; }

    private void Refresh()
    {
        undo.IsEnabled = document.CanUndo; redo.IsEnabled = document.CanRedo;
        if (paintedTool != tool) foreach (var (t, button) in tools)
        {
            button.Background = t == tool ? ActiveTool : t <= Tool.Text ? PrimaryTool : SecondaryTool;
            button.BorderBrush = t == tool ? Theme.Accent : Brushes.Transparent;
        }
        paintedTool = tool;
        string color = (selected ?? defaults).Color;
        if (displayedColor != color) { colorPreview.Fill = Rendering.Brush(color); displayedColor = color; }
        vertical.Visibility = horizontal.Visibility = HasSelection && drag is not "select" ? Visibility.Visible : Visibility.Collapsed;
        status.Text = HasSelection ? "Пробел + перетаскивание: переместить область · ПКМ: выбрать объект · двойной щелчок: текст · Ctrl+C / Ctrl+S · Esc" : "Выделите область мышью · Esc — отмена";
        if (HasSelection)
        {
            var screen = System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)(desktop.X + selection.Right), (int)(desktop.Y + selection.Bottom)));
            var b = screen.Bounds; var monitor = new Rect(b.X - desktop.X, b.Y - desktop.Y, b.Width, b.Height);
            var handle = Native.MonitorFromPoint(new Native.NativePoint { X = b.X + 1, Y = b.Y + 1 }, 2);
            try { if (Native.GetDpiForMonitor(handle, 0, out uint dpi, out _) == 0) uiScale = dpi / 96d; } catch (DllNotFoundException) { uiScale = 1; }
            if (drag == null)
            {
                workspace = PanelLayout.WorkspaceFor(selection, ImageBounds, monitor, uiScale);
                var transform = new TransformGroup(); transform.Children.Add(new ScaleTransform(workspace.Zoom, workspace.Zoom));
                transform.Children.Add(new TranslateTransform(workspace.Offset.X, workspace.Offset.Y)); scene.RenderTransform = transform;
                PositionEditor();
            }
            var placement = PanelLayout.Place(workspace.Display(selection), monitor, uiScale);
            if (placement.Visible)
            {
                vertical.LayoutTransform = horizontal.LayoutTransform = new ScaleTransform(uiScale, uiScale);
                Canvas.SetLeft(vertical, placement.Vertical.X); Canvas.SetTop(vertical, placement.Vertical.Y);
                Canvas.SetLeft(horizontal, placement.Horizontal.X); Canvas.SetTop(horizontal, placement.Horizontal.Y);
            }
            else
            {
                vertical.Visibility = horizontal.Visibility = Visibility.Collapsed;
                status.Text = "Недостаточно места вне области для панелей. Уменьшите область. Ctrl+C — копировать · Ctrl+S — сохранить · Ctrl+P — печать · Esc — отмена";
            }
            if (workspace.Zoom < 1 && placement.Visible) status.Text = $"Вид {workspace.Zoom:P0}; экспорт — исходные пиксели. " + status.Text;
        }
        scene.InvalidateVisual();
    }

    // Deterministic integration entry point: uses the same document, panels, editor and export as interactive capture.
    internal void PrepareSmokeScene(bool editText = false)
    {
        selection = new Rect(100, 100, 650, 400);
        if (document.Items.Length > 0)
        {
            selected = document.Items.Last(x => x.Tool == Tool.Text); tool = Tool.Text; Refresh();
            if (editText) BeginText(selected); return;
        }
        document.Put(new Annotation { Tool = Tool.Arrow, Box = new Rect(180, 180, 240, 110), Points = [new(180, 180), new(420, 290)] });
        document.Put(new Annotation { Tool = Tool.Pencil, Box = new Rect(200, 380, 170, 35), Points = [new(200, 400), new(240, 380), new(290, 415), new(370, 390)], Color = "#FFD166" });
        var text = new Annotation { Tool = Tool.Text, Box = new Rect(380, 320, 320, 140), Text = "Привет, LumenSnap!\nРедактируемый текст", FontSize = 26, Bold = true, Color = "#FFFFFF", Background = true };
        document.Put(text); selected = text; tool = Tool.Text; Refresh();
        if (editText) BeginText(text);
    }
    internal Button ActiveButton => tools[tool];
    internal Button CloseButton => closeCapture;
    internal FrameworkElement OpenPropertiesForSmoke() { ShowProperties(); return (FrameworkElement)properties!.Child; }
    internal void ClosePropertiesForSmoke() { properties!.IsOpen = false; scene.Focus(); }
    internal BitmapSource RenderUi()
    {
        canvas.Measure(new Size(frozen.PixelWidth, frozen.PixelHeight)); canvas.Arrange(new Rect(0, 0, frozen.PixelWidth, frozen.PixelHeight)); canvas.UpdateLayout();
        var bitmap = new RenderTargetBitmap(frozen.PixelWidth, frozen.PixelHeight, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(canvas); bitmap.Freeze(); return bitmap;
    }

    private sealed class Scene(Overlay owner) : FrameworkElement
    {
        private readonly SelectionChrome chrome = new();
        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawImage(owner.frozen, owner.ImageBounds);
            var s = owner.selection;
            if (!owner.HasSelection) dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(110, 0, 0, 0)), null, owner.ImageBounds);
            else
            {
                var outside = new CombinedGeometry(GeometryCombineMode.Exclude, new RectangleGeometry(owner.ImageBounds), new RectangleGeometry(s));
                dc.DrawGeometry(new SolidColorBrush(Color.FromArgb(110, 0, 0, 0)), null, outside);
                dc.PushClip(new RectangleGeometry(s));
                foreach (var a in owner.document.Items)
                {
                    if (owner.editor != null && a.Id == owner.selected?.Id) continue;
                    Rendering.Draw(dc, owner.dragOriginal?.Id == a.Id && owner.selected != null ? owner.selected : a);
                }
                if (owner.draft != null) Rendering.Draw(dc, owner.draft);
                dc.Pop();
                chrome.Draw(dc, s, owner.uiScale / owner.workspace.Zoom);
                foreach (var (_, p) in owner.Handles()) dc.DrawRectangle(Theme.Panel, new Pen(Theme.Accent, 1), new Rect(p - new Vector(3, 3), new Size(6, 6)));
                var dimensions = new System.Windows.Media.FormattedText($"{(int)Math.Ceiling(s.Width)} × {(int)Math.Ceiling(s.Height)} px", System.Globalization.CultureInfo.CurrentUICulture,
                    FlowDirection.LeftToRight, new Typeface("Segoe UI"), 13 * owner.uiScale, Brushes.White, 1);
                dc.DrawText(dimensions, new Point(s.X + 8, Math.Max(3, s.Y - 23 * owner.uiScale)));
                if (owner.selected != null && owner.editor == null)
                {
                    dc.DrawRectangle(null, new Pen(Brushes.White, 1) { DashStyle = DashStyles.Dash }, owner.selected.Box);
                    dc.DrawRectangle(Theme.Panel, new Pen(Brushes.White, 1), new Rect(owner.selected.Box.BottomRight - new Vector(5, 5), new Size(10, 10)));
                }
            }
        }
    }
}
