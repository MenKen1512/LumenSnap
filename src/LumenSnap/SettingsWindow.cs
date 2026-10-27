namespace LumenSnap;

internal sealed class SettingsWindow : Window
{
    internal SettingsWindow(Settings current, Func<Settings, bool> apply)
    {
        Title = "LumenSnap — Настройки"; Width = 510; Height = 700;
        MaxHeight = SystemParameters.WorkArea.Height; MinWidth = 440; MinHeight = Math.Min(440, MaxHeight);
        WindowStartupLocation = WindowStartupLocation.CenterScreen; Background = Theme.Panel; Foreground = Theme.Foreground;
        var layout = new DockPanel { Margin = new Thickness(22) }; Content = layout;
        var header = new StackPanel { Margin = new Thickness(0, 0, 0, 14) };
        header.Children.Add(new TextBlock { Text = "Настройки", FontSize = 23, FontWeight = FontWeights.SemiBold });
        header.Children.Add(Theme.Caption("Захват экрана и параметры по умолчанию")); DockPanel.SetDock(header, Dock.Top); layout.Children.Add(header);
        var footer = new StackPanel { Margin = new Thickness(0, 10, 0, 0) }; DockPanel.SetDock(footer, Dock.Bottom); layout.Children.Add(footer);
        var error = new TextBlock { Foreground = Brushes.Orange, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6) }; footer.Children.Add(error);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right }; footer.Children.Add(actions);
        actions.Children.Add(Theme.Button("Отмена", "Закрыть без сохранения", Close, 92));
        var body = new StackPanel { Margin = new Thickness(0, 0, 8, 0) };
        layout.Children.Add(new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        uint key = current.HotkeyCode, mods = current.HotkeyModifiers;
        var binding = new TextBox { Text = current.HotkeyLabel, IsReadOnly = true, FontSize = 17, Padding = new Thickness(9, 7) };
        binding.PreviewKeyDown += (_, e) =>
        {
            var k = e.Key == Key.System ? e.SystemKey : e.Key;
            // Tab remains available for keyboard navigation between settings.
            if (k == Key.Tab) return;
            e.Handled = true;
            if (k is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin) return;
            key = (uint)KeyInterop.VirtualKeyFromKey(k); var m = Keyboard.Modifiers;
            mods = ((m & ModifierKeys.Alt) != 0 ? 1u : 0) | ((m & ModifierKeys.Control) != 0 ? 2u : 0) |
                ((m & ModifierKeys.Shift) != 0 ? 4u : 0) | ((m & ModifierKeys.Windows) != 0 ? 8u : 0);
            binding.Text = (current with { HotkeyCode = key, HotkeyModifiers = mods }).HotkeyLabel;
        };
        var reset = Theme.Button("Сбросить: Ctrl+Alt+S", "Сбросить сочетание", () => { key = 0x53; mods = 3; binding.Text = "Ctrl+Alt+S"; }, 190);
        reset.HorizontalAlignment = HorizontalAlignment.Left;
        body.Children.Add(Theme.Section("Захват экрана", Theme.Field("Нажмите нужное сочетание клавиш", binding), reset,
            new TextBlock { Text = "Если сочетание занято, прежнее сохранится. Клавиши производителя доступны, если Windows получает их события.", Foreground = Theme.Muted, FontSize = 11, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) }));
        var login = new CheckBox { Content = "Запускать при входе в Windows", IsChecked = current.StartAtLogin };
        var notifications = new CheckBox { Content = "Уведомлять о копировании и сохранении", IsChecked = current.Notifications };
        body.Children.Add(Theme.Section("Поведение приложения", login, notifications));
        var glow = new CheckBox { Content = "Мягкое свечение стрелок", IsChecked = current.Glow };
        var color = new TextBox { Text = current.Color, MaxLength = 7 };
        var width = new TextBox { Text = current.Width.ToString("0.##") };
        var row = new Grid(); row.ColumnDefinitions.Add(new ColumnDefinition()); row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) }); row.ColumnDefinitions.Add(new ColumnDefinition());
        row.Children.Add(Theme.Field("Цвет · #RRGGBB", color)); var widthField = Theme.Field("Толщина · 1–40 px", width); Grid.SetColumn(widthField, 2); row.Children.Add(widthField);
        body.Children.Add(Theme.Section("Рисование", row, glow));
        var font = new ComboBox { IsEditable = true, Text = current.Font, ItemsSource = Theme.FontNames };
        var size = new TextBox { Text = current.FontSize.ToString("0.##") };
        body.Children.Add(Theme.Section("Текст", Theme.Field("Шрифт", font), Theme.Field("Размер · 8–200 px", size)));
        var save = Theme.Button("Сохранить", "Сохранить настройки", () =>
        {
            if (!Settings.ValidColor(color.Text) || !double.TryParse(width.Text, out double w) || !double.IsFinite(w) || w is < 1 or > 40 || !double.TryParse(size.Text, out double s) || !double.IsFinite(s) || s is < 8 or > 200 || !Settings.ValidFont(font.Text))
            { error.Text = "Проверьте цвет #RRGGBB, толщину 1–40, шрифт и размер 8–200."; return; }
            var next = current with { HotkeyCode = key, HotkeyModifiers = mods, StartAtLogin = login.IsChecked == true,
                Notifications = notifications.IsChecked == true, Glow = glow.IsChecked == true, Color = color.Text.ToUpperInvariant(), Width = w, Font = font.Text, FontSize = s };
            try { if (!apply(next)) { error.Text = "Сочетание занято или зарезервировано. Прежнее сохранено."; return; } Close(); }
            catch (Exception e) { error.Text = "Не удалось сохранить настройки: " + e.Message; }
        }, 112);
        save.Background = Rendering.Brush("#305B7E"); save.BorderBrush = Theme.Accent; actions.Children.Add(save);
    }
}
