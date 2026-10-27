namespace LumenSnap;

// Contextual controls share the same mutation path as the in-place editor and undo history.
internal sealed class ToolProperties : Border
{
    internal ToolProperties(Annotation active, Tool tool, Action<Func<Annotation, Annotation>> change, Action done, Action? edit)
    {
        Background = Theme.Panel; BorderBrush = Theme.Muted; BorderThickness = new Thickness(1);
        CornerRadius = new CornerRadius(10); Padding = new Thickness(12); Width = 296;
        var body = new StackPanel();
        var names = new[] { "Карандаш / маркер", "Стрелка", "Текст", "Линия", "Прямоугольник", "Эллипс" };
        body.Children.Add(new TextBlock { Text = names[(int)tool], Foreground = Theme.Foreground, FontSize = 16, FontWeight = FontWeights.SemiBold, Margin = new Thickness(2, 0, 0, 3) });
        body.Children.Add(Theme.Caption("Параметры выбранного инструмента"));
        var colors = new WrapPanel { Margin = new Thickness(-2, 0, 0, 7) };
        var hex = new TextBox { Text = active.Color, MaxLength = 7, ToolTip = "Цвет в формате #RRGGBB" };
        var swatches = new Dictionary<string, Button>();
        void MarkColor(string value) { foreach (var (color, b) in swatches) b.BorderBrush = string.Equals(color, value, StringComparison.OrdinalIgnoreCase) ? Theme.Foreground : Brushes.Transparent; }
        foreach (string color in new[] { "#65B7FF", "#FF635C", "#FFD166", "#63D7A1", "#FFFFFF", "#141F2E", active.Color }.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var b = Theme.Button("", color, () => { active = active with { Color = color }; hex.Text = color; MarkColor(color); change(a => a with { Color = color }); }, 28);
            b.Height = 28; b.Padding = new Thickness(5); b.Content = new System.Windows.Shapes.Ellipse { Width = 14, Height = 14, Fill = Rendering.Brush(color), Stroke = Theme.Muted, StrokeThickness = .5 };
            swatches.Add(color, b); colors.Children.Add(b);
        }
        MarkColor(active.Color);
        hex.LostKeyboardFocus += (_, _) =>
        {
            if (Settings.ValidColor(hex.Text)) { active = active with { Color = hex.Text.ToUpperInvariant() }; MarkColor(active.Color); change(a => a with { Color = active.Color }); }
            else hex.Text = active.Color;
        };
        body.Children.Add(Theme.Section("Цвет", colors, Theme.Field("HEX · #RRGGBB", hex)));
        UIElement Range(string label, double value, double min, double max, double step, Action<double> apply)
        {
            var panel = new Grid(); panel.ColumnDefinitions.Add(new ColumnDefinition()); panel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(54) });
            var slider = new Slider { Minimum = min, Maximum = max, Value = value, TickFrequency = step, IsSnapToTickEnabled = true, Margin = new Thickness(0, 0, 10, 0) };
            var number = new TextBox { Text = value.ToString("0.##"), ToolTip = $"{min}–{max}", VerticalContentAlignment = VerticalAlignment.Center };
            Grid.SetColumn(number, 1); panel.Children.Add(slider); panel.Children.Add(number);
            slider.ValueChanged += (_, _) => number.Text = slider.Value.ToString("0.##");
            void Commit() => apply(slider.Value);
            slider.PreviewMouseLeftButtonUp += (_, _) => Commit(); slider.KeyUp += (_, _) => Commit();
            number.LostKeyboardFocus += (_, _) => { if (double.TryParse(number.Text, out double n) && double.IsFinite(n) && n >= min && n <= max) { slider.Value = n; Commit(); } else number.Text = slider.Value.ToString("0.##"); };
            return Theme.Field(label, panel);
        }
        var opacity = Range("Непрозрачность · %", active.Opacity * 100, 10, 100, 10, n => change(a => a with { Opacity = n / 100 }));
        if (tool == Tool.Text)
        {
            var fonts = new ComboBox { ItemsSource = Theme.FontNames, Text = active.Font, IsEditable = true };
            fonts.LostKeyboardFocus += (_, _) => { if (Settings.ValidFont(fonts.Text)) { active = active with { Font = fonts.Text }; change(a => a with { Font = active.Font }); } else fonts.Text = active.Font; };
            var size = Range("Размер · px", active.FontSize, 8, 200, 1, n => change(a => a with { FontSize = n }));
            var bold = new CheckBox { Content = "Полужирный", IsChecked = active.Bold }; bold.Click += (_, _) => change(a => a with { Bold = bold.IsChecked == true });
            var alignments = new[] { TextAlignment.Left, TextAlignment.Center, TextAlignment.Right };
            var align = new ComboBox { ItemsSource = new[] { "По левому краю", "По центру", "По правому краю" }, SelectedIndex = Array.IndexOf(alignments, active.Alignment) };
            align.SelectionChanged += (_, _) => { if (align.SelectedIndex >= 0) change(a => a with { Alignment = alignments[align.SelectedIndex] }); };
            body.Children.Add(Theme.Section("Типографика", Theme.Field("Шрифт", fonts), size, bold, Theme.Field("Выравнивание", align)));
            var background = new CheckBox { Content = "Подложка текста", IsChecked = active.Background }; background.Click += (_, _) => change(a => a with { Background = background.IsChecked == true });
            var outline = new CheckBox { Content = "Обводка текста", IsChecked = active.Outline }; outline.Click += (_, _) => change(a => a with { Outline = outline.IsChecked == true });
            body.Children.Add(Theme.Section("Оформление", background, outline, opacity));
        }
        else
        {
            var line = new List<UIElement> { Range("Толщина · px", active.Width, 1, 40, 1, n => change(a => a with { Width = n })), opacity };
            if (tool == Tool.Arrow)
            {
                var glow = new CheckBox { Content = "Мягкое свечение стрелки", IsChecked = active.Glow }; glow.Click += (_, _) => change(a => a with { Glow = glow.IsChecked == true }); line.Add(glow);
            }
            body.Children.Add(Theme.Section("Линия", line.ToArray()));
        }
        var footer = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        if (edit != null) footer.Children.Add(Theme.Button("Изменить текст", "Редактировать выбранный текст", edit, 132));
        footer.Children.Add(Theme.Button("Готово", "Закрыть параметры", done, 82));
        var layout = new DockPanel(); DockPanel.SetDock(footer, Dock.Bottom); layout.Children.Add(footer);
        layout.Children.Add(new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }); Child = layout;
    }
}
