using System.Windows.Automation;

namespace LumenSnap;

internal static class Theme
{
    internal static readonly Brush Panel = Rendering.Brush("#172234"), Accent = Rendering.Brush("#65B7FF"), Foreground = Rendering.Brush("#EAF3FF");
    internal static void Install(Application app)
    {
        // Original text/vector UI only; no external icon packs or copied artwork.
        var style = new Style(typeof(Button));
        style.Setters.Add(new Setter(Control.ForegroundProperty, Foreground));
        style.Setters.Add(new Setter(Control.BackgroundProperty, Rendering.Brush("#25354B")));
        style.Setters.Add(new Setter(Control.BorderBrushProperty, Rendering.Brush("#3F5874")));
        style.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(1)));
        style.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(6)));
        style.Setters.Add(new Setter(Control.FontSizeProperty, 14d));
        var template = new ControlTemplate(typeof(Button));
        var border = new FrameworkElementFactory(typeof(Border));
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(7));
        border.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        var content = new FrameworkElementFactory(typeof(ContentPresenter));
        content.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        content.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        content.SetValue(FrameworkElement.MarginProperty, new Thickness(4)); border.AppendChild(content); template.VisualTree = border;
        style.Setters.Add(new Setter(Control.TemplateProperty, template));
        var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(Control.BackgroundProperty, Rendering.Brush("#365574"))); style.Triggers.Add(hover);
        var focus = new Trigger { Property = UIElement.IsKeyboardFocusedProperty, Value = true };
        focus.Setters.Add(new Setter(Control.BorderBrushProperty, Brushes.White)); focus.Setters.Add(new Setter(Control.BorderThicknessProperty, new Thickness(2))); style.Triggers.Add(focus);
        var disabled = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
        disabled.Setters.Add(new Setter(UIElement.OpacityProperty, .35)); style.Triggers.Add(disabled);
        app.Resources.Add(typeof(Button), style);
    }
    internal static Button Button(string text, string hint, Action click, double width = 40)
    {
        var b = new Button { Content = text, ToolTip = hint, Width = width, Height = 40, Margin = new Thickness(2) };
        AutomationProperties.SetName(b, hint); b.Click += (_, _) => click(); return b;
    }
    internal static Border Surface(UIElement child) => new()
    { Background = Panel, BorderBrush = Rendering.Brush("#3E526D"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(12), Padding = new Thickness(3), Child = child };
}
