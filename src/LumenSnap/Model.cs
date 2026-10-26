namespace LumenSnap;

public enum Tool { Pencil, Arrow, Text, Line, Rectangle, Ellipse }

// Coordinates are physical pixels relative to the frozen virtual desktop origin.
public sealed record Annotation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Tool Tool { get; init; }
    public Point[] Points { get; init; } = [];
    public Rect Box { get; init; }
    public string Color { get; init; } = "#65B7FF";
    public double Width { get; init; } = 4;
    public double Opacity { get; init; } = 1;
    public bool Glow { get; init; } = true;
    public string Text { get; init; } = "";
    public string Font { get; init; } = "Segoe UI";
    public double FontSize { get; init; } = 26;
    public bool Bold { get; init; }
    public TextAlignment Alignment { get; init; }
    public bool Background { get; init; }
    public bool Outline { get; init; }

    public Annotation Move(Vector v) => this with
    {
        Box = new Rect(Box.Location + v, Box.Size),
        Points = Points.Select(p => p + v).ToArray()
    };

    public Annotation Resize(Rect target)
    {
        double sx = target.Width / Math.Max(1, Box.Width), sy = target.Height / Math.Max(1, Box.Height);
        return this with { Box = target, Points = Points.Select(p => new Point(
            target.X + (p.X - Box.X) * sx, target.Y + (p.Y - Box.Y) * sy)).ToArray() };
    }
}

public sealed class Document
{
    private readonly List<Annotation[]> past = [];
    private readonly Stack<Annotation[]> future = new();
    public Annotation[] Items { get; private set; } = [];
    public bool CanUndo => past.Count > 0;
    public bool CanRedo => future.Count > 0;
    public void Set(IEnumerable<Annotation> items)
    {
        var next = items.ToArray();
        if (Items.SequenceEqual(next)) return;
        past.Add(Items);
        if (past.Count > 100) past.RemoveAt(0);
        Items = next;
        future.Clear();
    }
    public void Put(Annotation item) => Set(Items.Any(x => x.Id == item.Id)
        ? Items.Select(x => x.Id == item.Id ? item : x) : Items.Append(item));
    public void Delete(Guid id) => Set(Items.Where(x => x.Id != id));
    public void Undo() { if (!CanUndo) return; future.Push(Items); Items = past[^1]; past.RemoveAt(past.Count - 1); }
    public void Redo() { if (!CanRedo) return; past.Add(Items); Items = future.Pop(); }
}

public static class Coordinates
{
    public static Rect Region(Point a, Point b, Rect bounds)
    {
        var r = new Rect(a, b); r.Intersect(bounds); return r;
    }
    public static Rect Move(Rect r, Vector v, Rect bounds) => new(
        Math.Clamp(r.X + v.X, bounds.Left, Math.Max(bounds.Left, bounds.Right - r.Width)),
        Math.Clamp(r.Y + v.Y, bounds.Top, Math.Max(bounds.Top, bounds.Bottom - r.Height)), r.Width, r.Height);
    public static Int32Rect Crop(Rect r, int width, int height)
    {
        int x = Math.Clamp((int)Math.Floor(r.X), 0, width - 1), y = Math.Clamp((int)Math.Floor(r.Y), 0, height - 1);
        int right = Math.Clamp((int)Math.Ceiling(r.Right), x + 1, width), bottom = Math.Clamp((int)Math.Ceiling(r.Bottom), y + 1, height);
        return new(x, y, right - x, bottom - y);
    }
    public static Point Pixels(Point dip, double dpi) => new(dip.X * dpi / 96, dip.Y * dpi / 96);
    public static Point Dips(Point px, double dpi) => new(px.X * 96 / dpi, px.Y * 96 / dpi);
}

public readonly record struct PanelPlacement(Rect Vertical, Rect Horizontal, bool Visible);
public static class PanelLayout
{
    public static PanelPlacement Place(Rect s, Rect monitor, double scale = 1)
    {
        double vw = 52 * scale, vh = 344 * scale, hw = 386 * scale, hh = 52 * scale;
        double gap = 8 * scale, between = 6 * scale;
        // Try right/below, left/below, right/above and left/above. Panels never cover selected pixels.
        foreach (bool left in new[] { false, true })
        foreach (bool above in new[] { false, true })
        {
            double vx = left ? s.Left - gap - vw : s.Right + gap;
            double hy = above ? s.Top - gap - hh : s.Bottom + gap;
            double hx = left ? vx + vw + between : vx - between - hw;
            hx = Math.Clamp(hx, monitor.Left, Math.Max(monitor.Left, monitor.Right - hw));
            double vy = hy + hh - vh;
            var v = new Rect(vx, vy, vw, vh); var h = new Rect(hx, hy, hw, hh);
            if (monitor.Contains(v) && monitor.Contains(h) && !v.IntersectsWith(s) && !h.IntersectsWith(s) && !v.IntersectsWith(h))
                return new(v, h, true);
        }
        // Independently dock to any exterior strips if aligned layout cannot fit.
        Rect? vertical = null, horizontal = null;
        foreach (double x in new[] { s.Right + gap, s.Left - gap - vw })
        {
            var v = new Rect(x, Math.Clamp(s.Bottom + gap + hh - vh, monitor.Top, Math.Max(monitor.Top, monitor.Bottom - vh)), vw, vh);
            if (monitor.Contains(v) && !v.IntersectsWith(s)) { vertical = v; break; }
        }
        foreach (double y in new[] { s.Bottom + gap, s.Top - gap - hh })
        {
            var h = new Rect(Math.Clamp(s.Right - hw, monitor.Left, Math.Max(monitor.Left, monitor.Right - hw)), y, hw, hh);
            if (monitor.Contains(h) && !h.IntersectsWith(s) && (vertical is null || !h.IntersectsWith(vertical.Value))) { horizontal = h; break; }
        }
        return vertical is { } vr && horizontal is { } hr ? new(vr, hr, true) : new(Rect.Empty, Rect.Empty, false);
    }
}

public enum CaptureCommand { None, TextEscape, Cancel, Copy, Save, Undo, Redo, Print, Delete }
public static class ShortcutPolicy
{
    public static CaptureCommand Resolve(Key key, ModifierKeys modifiers, bool textFocus, bool editingText)
    {
        if (key == Key.Escape) return editingText ? CaptureCommand.TextEscape : CaptureCommand.Cancel;
        if (modifiers == ModifierKeys.Control && key == Key.S) return CaptureCommand.Save;
        if (textFocus) return CaptureCommand.None;
        if (modifiers == ModifierKeys.Control) return key switch
        {
            Key.C => CaptureCommand.Copy, Key.Z => CaptureCommand.Undo, Key.Y => CaptureCommand.Redo,
            Key.P => CaptureCommand.Print, _ => CaptureCommand.None
        };
        if (modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && key == Key.Z) return CaptureCommand.Redo;
        return key == Key.Delete ? CaptureCommand.Delete : CaptureCommand.None;
    }
}
