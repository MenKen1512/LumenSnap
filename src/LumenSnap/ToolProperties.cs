namespace LumenSnap;

internal sealed class ToolProperties : Border
{
    internal ToolProperties(Annotation active, Tool tool, Action<Func<Annotation, Annotation>> change, Action done, Action? edit)
    {
        Background = Theme.Panel; BorderBrush = Rendering.Brush("#405772"); BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(10); Padding = new Thickness(8); Width = 240;
        var layout = new DockPanel(); Child = layout;
        var title = new DockPanel { Margin = new Thickness(0, 0, 0, 5) };
        var close = Theme.Button("", "Закрыть параметры · Esc", done, 24); close.Height = 20; close.Margin = new Thickness(0); close.Padding = new Thickness(2); close.Content = Theme.Icon("Close", 11); DockPanel.SetDock(close, Dock.Right); title.Children.Add(close);
        var names = new[] { "Карандаш / маркер", "Стрелка", "Текст", "Линия", "Прямоугольник", "Эллипс" };
        title.Children.Add(new TextBlock { Text = names[(int)tool], FontSize = 13, FontWeight = FontWeights.SemiBold, Foreground = Theme.Foreground, VerticalAlignment = VerticalAlignment.Center });
        DockPanel.SetDock(title, Dock.Top); layout.Children.Add(title);
        if (edit != null) { var button = Theme.Button("Изменить текст", "Редактировать выбранный текст", edit, 214); button.Height = 24; button.Margin = new Thickness(0, 4, 0, 0); DockPanel.SetDock(button, Dock.Bottom); layout.Children.Add(button); }
        var body = new StackPanel(); layout.Children.Add(new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        body.Children.Add(new ColorPalette(active.Color, value => change(a => a with { Color = value })) { HorizontalAlignment = HorizontalAlignment.Center });
        body.Children.Add(new Border { Height = 1, Background = Rendering.Brush("#2C4056"), Margin = new Thickness(0, 5, 0, 5) });
        if (tool == Tool.Text)
        {
            var fonts = new ComboBox { ItemsSource = Theme.FontNames, Text = active.Font, IsEditable = true, ToolTip = "Шрифт текста", Margin = new Thickness(0, 0, 0, 5) };
            fonts.LostKeyboardFocus += (_, _) => { if (Settings.ValidFont(fonts.Text)) { active = active with { Font = fonts.Text }; change(a => a with { Font = active.Font }); } else fonts.Text = active.Font; };
            body.Children.Add(fonts);
            body.Children.Add(Theme.Range("Размер, px", active.FontSize, 8, 200, 1, n => change(a => a with { FontSize = n })));
            var format = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 2), HorizontalAlignment = HorizontalAlignment.Center };
            var bold = Theme.Button("Ж", "Полужирный", () => { }, 32); bold.Height = 24; bold.FontWeight = FontWeights.Bold; bold.Tag = active.Bold ? "selected" : "neutral";
            bool isBold = active.Bold; bold.Click += (_, _) => { isBold = !isBold; bold.Tag = isBold ? "selected" : "neutral"; change(a => a with { Bold = isBold }); }; format.Children.Add(bold);
            var buttons = new List<(TextAlignment Alignment, Button Button)>();
            foreach (var (alignment, label) in new[] { (TextAlignment.Left, "Слева"), (TextAlignment.Center, "По центру"), (TextAlignment.Right, "Справа") })
            {
                var button = Theme.Button("", label, () => { foreach (var item in buttons) item.Button.Tag = item.Alignment == alignment ? "selected" : "neutral"; change(a => a with { Alignment = alignment }); }, 44);
                button.Height = 24; button.Content = AlignmentIcon(alignment); button.Tag = active.Alignment == alignment ? "selected" : "neutral"; buttons.Add((alignment, button)); format.Children.Add(button);
            }
            body.Children.Add(format);
            var options = new StackPanel { Orientation = Orientation.Horizontal };
            var background = new CheckBox { Content = "Подложка", IsChecked = active.Background, Margin = new Thickness(0, 3, 10, 3) }; background.Click += (_, _) => change(a => a with { Background = background.IsChecked == true }); options.Children.Add(background);
            var outline = new CheckBox { Content = "Обводка", IsChecked = active.Outline }; outline.Click += (_, _) => change(a => a with { Outline = outline.IsChecked == true }); options.Children.Add(outline); body.Children.Add(options);
        }
        else body.Children.Add(Theme.Range("Линия, px", active.Width, 1, 40, 1, n => change(a => a with { Width = n })));
        body.Children.Add(Theme.Range("Непрозр., %", active.Opacity * 100, 10, 100, 10, n => change(a => a with { Opacity = n / 100 })));
        if (tool == Tool.Arrow)
        {
            var glow = new CheckBox { Content = "Свечение стрелки", IsChecked = active.Glow }; glow.Click += (_, _) => change(a => a with { Glow = glow.IsChecked == true }); body.Children.Add(glow);
        }
    }
    private static UIElement AlignmentIcon(TextAlignment alignment)
    {
        string middle = alignment == TextAlignment.Left ? "M2,6 L10,6 M2,14 L10,14" : alignment == TextAlignment.Right ? "M6,6 L14,6 M6,14 L14,14" : "M4,6 L12,6 M4,14 L12,14";
        return new System.Windows.Shapes.Path { Data = Geometry.Parse("M2,2 L14,2 M2,10 L14,10 " + middle), Width = 14, Height = 14, Stroke = Theme.Foreground, StrokeThickness = 1.2, Stretch = Stretch.Uniform };
    }
}
