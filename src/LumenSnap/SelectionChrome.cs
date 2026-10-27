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
            var outer = selection; outer.Inflate(14 * scale, 14 * scale);
            exterior = new CombinedGeometry(GeometryCombineMode.Exclude, new RectangleGeometry(outer), new RectangleGeometry(selection)); exterior.Freeze();
            if (scale != previousScale)
            {
                // Nested gradient strokes approximate a smooth falloff without blurring the desktop.
                pens = Enumerable.Range(1, 12).Reverse().Select(radius =>
                { var p = new Pen(Gradient(.09 * Math.Exp(-radius * radius / 24d)), radius * 2 * scale); p.Freeze(); return p; }).ToArray();
                edge = new Pen(Gradient(.95), scale); edge.Freeze();
            }
            previous = selection; previousScale = scale;
        }
        dc.PushClip(exterior!);
        foreach (var pen in pens) dc.DrawRoundedRectangle(null, pen, selection, 2.5 * scale, 2.5 * scale);
        dc.Pop();
        dc.DrawRoundedRectangle(null, edge, selection, 2.5 * scale, 2.5 * scale);
    }
    private static Brush Gradient(double opacity)
    {
        var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(1, 1), Opacity = opacity };
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(112, 243, 225), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(107, 194, 255), .42));
        brush.GradientStops.Add(new GradientStop(Color.FromRgb(155, 131, 255), 1));
        brush.Freeze(); return brush;
    }
}
