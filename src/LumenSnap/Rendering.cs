using System.Globalization;

namespace LumenSnap;

public static class Rendering
{
    public static Brush Brush(string hex) => new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
    public static Geometry Geometry(Annotation a)
    {
        var box = a.Box;
        if (a.Tool == Tool.Rectangle) return new RectangleGeometry(box);
        if (a.Tool == Tool.Ellipse) return new EllipseGeometry(box);
        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            if (a.Points.Length == 0) return g;
            c.BeginFigure(a.Points[0], false, false);
            if (a.Tool == Tool.Pencil)
            {
                for (int i = 1; i < a.Points.Length - 1; i++)
                {
                    var midpoint = new Point((a.Points[i].X + a.Points[i + 1].X) / 2, (a.Points[i].Y + a.Points[i + 1].Y) / 2);
                    c.QuadraticBezierTo(a.Points[i], midpoint, true, true);
                }
                if (a.Points.Length > 1) c.LineTo(a.Points[^1], true, true);
            }
            else
            {
                Point end = a.Points[^1]; c.LineTo(end, true, false);
                if (a.Tool == Tool.Arrow)
                {
                    Vector d = end - a.Points[0];
                    if (d.Length > 0.1)
                    {
                        double head = Math.Min(d.Length * .5, Math.Max(14, a.Width * 3.5)); d.Normalize();
                        var side = new Vector(-d.Y, d.X);
                        c.BeginFigure(end - d * head + side * head * .5, false, false);
                        c.LineTo(end, true, false); c.LineTo(end - d * head - side * head * .5, true, false);
                    }
                }
            }
        }
        g.Freeze(); return g;
    }

    public static void Draw(DrawingContext dc, Annotation a)
    {
        dc.PushOpacity(a.Opacity);
        if (a.Tool == Tool.Text)
        {
            dc.PushClip(new RectangleGeometry(a.Box));
            if (a.Background) dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(200, 16, 24, 38)), null, a.Box);
            var text = new FormattedText(a.Text.Length == 0 ? " " : a.Text, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface(new FontFamily(a.Font), FontStyles.Normal,
                a.Bold ? FontWeights.Bold : FontWeights.Normal, FontStretches.Normal), a.FontSize, Brush(a.Color), 1)
            { MaxTextWidth = Math.Max(1, a.Box.Width - 8), TextAlignment = a.Alignment, Trimming = TextTrimming.None };
            Point p = a.Box.TopLeft + new Vector(4, 4);
            if (a.Outline) dc.DrawGeometry(null, new Pen(Brush("#141F2E"), 2), text.BuildGeometry(p));
            dc.DrawText(text, p); dc.Pop();
        }
        else
        {
            var geometry = Geometry(a);
            Pen Stroke(Brush brush, double width) => new(brush, width)
            { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
            if (a.Tool == Tool.Arrow && a.Glow)
            {
                dc.PushOpacity(.10); dc.DrawGeometry(null, Stroke(Brush(a.Color), a.Width + 8), geometry); dc.Pop();
                dc.PushOpacity(.20); dc.DrawGeometry(null, Stroke(Brush(a.Color), a.Width + 4), geometry); dc.Pop();
            }
            dc.DrawGeometry(null, Stroke(Brush(a.Color), a.Width), geometry);
        }
        dc.Pop();
    }

    public static bool Hit(Annotation a, Point point) => a.Tool == Tool.Text
        ? a.Box.Contains(point) : Geometry(a).StrokeContains(new Pen(Brushes.Black, Math.Max(12, a.Width + 8)), point);

    public static BitmapSource Export(BitmapSource frozen, Rect selection, IEnumerable<Annotation> items)
    {
        var r = Coordinates.Crop(selection, frozen.PixelWidth, frozen.PixelHeight);
        var crop = new CroppedBitmap(frozen, r); crop.Freeze();
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawImage(crop, new Rect(0, 0, r.Width, r.Height));
            dc.PushClip(new RectangleGeometry(new Rect(0, 0, r.Width, r.Height)));
            dc.PushTransform(new TranslateTransform(-r.X, -r.Y));
            foreach (var a in items) Draw(dc, a);
            dc.Pop(); dc.Pop();
        }
        var bitmap = new RenderTargetBitmap(r.Width, r.Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual); bitmap.Freeze(); return bitmap;
    }

    public static void Save(BitmapSource bitmap, string path)
    {
        BitmapEncoder encoder = Path.GetExtension(path).ToLowerInvariant() switch
        { ".jpg" or ".jpeg" => new JpegBitmapEncoder { QualityLevel = 95 }, ".bmp" => new BmpBitmapEncoder(), _ => new PngBitmapEncoder() };
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        // Write beside destination, then replace atomically; failures never truncate an existing screenshot.
        string temp = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { using (var stream = File.Create(temp)) encoder.Save(stream); File.Move(temp, path, true); }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
}
