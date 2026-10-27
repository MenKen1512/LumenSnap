namespace LumenSnap;

// Cached pens and a small geometry clip avoid a full-desktop blur/effect on every pointer update.
internal sealed class SelectionChrome
{
    private Rect previous = Rect.Empty;
    private double previousScale;
    private Geometry? exterior;
    private Pen[] pens = [];
    private Pen? edge;
    internal void Draw(DrawingContext dc, Rect selection, double scale)
    {
        if (selection != previous || scale != previousScale)
        {
            var outer = selection; outer.Inflate(10 * scale, 10 * scale);
            exterior = new CombinedGeometry(GeometryCombineMode.Exclude, new RectangleGeometry(outer), new RectangleGeometry(selection)); exterior.Freeze();
            if (scale != previousScale)
            {
                pens = new[] { (14d, (byte)10), (10d, (byte)16), (7d, (byte)25), (4d, (byte)45) }.Select(x =>
                { var b = new SolidColorBrush(Color.FromArgb(x.Item2, 86, 174, 255)); b.Freeze(); var p = new Pen(b, x.Item1 * scale); p.Freeze(); return p; }).ToArray();
                edge = new Pen(Theme.Accent, scale); edge.Freeze();
            }
            previous = selection; previousScale = scale;
        }
        dc.PushClip(exterior!);
        foreach (var pen in pens) dc.DrawRoundedRectangle(null, pen, selection, 2 * scale, 2 * scale);
        dc.Pop();
        dc.DrawRoundedRectangle(null, edge, selection, 2 * scale, 2 * scale);
    }
}
