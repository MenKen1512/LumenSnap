namespace LumenSnap;

internal sealed class SettingsWindow : Window
{
    internal SettingsWindow(Settings current, Func<Settings, bool> apply)
    {
        Title = "LumenSnap — Настройки"; Width = 440; Height = 570;
        MaxHeight = SystemParameters.WorkArea.Height; MinWidth = 410; MinHeight = Math.Min(390, MaxHeight);
        WindowStartupLocation = WindowStartupLocation.CenterScreen; Background = Theme.Panel; Foreground = Theme.Foreground;
        var layout = new DockPanel { Margin = new Thickness(16) }; Content = layout;
        var header = new TextBlock { Text = "Настройки", FontSize = 20, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 10) };
        DockPanel.SetDock(header, Dock.Top); layout.Children.Add(header);
        var footer = new StackPanel { Margin = new Thickness(0, 8, 0, 0) }; DockPanel.SetDock(footer, Dock.Bottom); layout.Children.Add(footer);
        var error = new TextBlock { Foreground = Brushes.Orange, TextWrapping = TextWrapping.Wrap, FontSize = 11 }; footer.Children.Add(error);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right }; footer.Children.Add(actions);
        actions.Children.Add(Theme.Button("Отмена", "Закрыть без сохранения", Close, 82));
        var body = new StackPanel { Margin = new Thickness(0, 0, 4, 0) }; layout.Children.Add(new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        uint key = current.HotkeyCode, mods = current.HotkeyModifiers;
        var binding = new TextBox { Text = current.HotkeyLabel, IsReadOnly = true, FontSize = 14, Padding = new Thickness(6, 4, 6, 4), ToolTip = "Нажмите нужное сочетание клавиш" };
        binding.PreviewKeyDown += (_, e) =>
        {
            var k = e.Key == Key.System ? e.SystemKey : e.Key; if (k == Key.Tab) return; e.Handled = true;
            if (k is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin) return;
            key = (uint)KeyInterop.VirtualKeyFromKey(k); var m = Keyboard.Modifiers;
            mods = ((m & ModifierKeys.Alt) != 0 ? 1u : 0) | ((m & ModifierKeys.Control) != 0 ? 2u : 0) | ((m & ModifierKeys.Shift) != 0 ? 4u : 0) | ((m & ModifierKeys.Windows) != 0 ? 8u : 0);
            binding.Text = (current with { HotkeyCode = key, HotkeyModifiers = mods }).HotkeyLabel;
        };
        var hotkey = new DockPanel(); var reset = Theme.Button("", "Сбросить: Ctrl+Alt+S", () => { key = 0x53; mods = 3; binding.Text = "Ctrl+Alt+S"; }, 30); reset.Content = Theme.Icon("Undo", 13); DockPanel.SetDock(reset, Dock.Right); hotkey.Children.Add(reset); hotkey.Children.Add(binding);
        body.Children.Add(Theme.Section("Захват", hotkey, new TextBlock { Text = "Если сочетание занято, прежнее сохранится.", Foreground = Theme.Muted, FontSize = 10.5, Margin = new Thickness(0, 5, 0, 0) }));
        var login = new CheckBox { Content = "Запускать при входе в Windows", IsChecked = current.StartAtLogin };
        var notifications = new CheckBox { Content = "Уведомлять о копировании и сохранении", IsChecked = current.Notifications };
        body.Children.Add(Theme.Section("Приложение", login, notifications));
        var palette = new ColorPalette(current.Color, _ => { }, 10);
        double lineWidth = current.Width, fontSize = current.FontSize;
        var glow = new CheckBox { Content = "Свечение стрелок", IsChecked = current.Glow };
        body.Children.Add(Theme.Section("Рисование", palette, Theme.Range("Линия, px", lineWidth, 1, 40, 1, n => lineWidth = n), glow));
        var font = new ComboBox { IsEditable = true, Text = current.Font, ItemsSource = Theme.FontNames, ToolTip = "Шрифт текста" };
        body.Children.Add(Theme.Section("Текст", font, Theme.Range("Размер, px", fontSize, 8, 200, 1, n => fontSize = n)));
        var save = Theme.Button("Сохранить", "Сохранить настройки", () =>
        {
            if (!Settings.ValidFont(font.Text)) { error.Text = "Проверьте название шрифта."; return; }
            var next = current with { HotkeyCode = key, HotkeyModifiers = mods, StartAtLogin = login.IsChecked == true,
                Notifications = notifications.IsChecked == true, Glow = glow.IsChecked == true, Color = palette.SelectedColor, Width = lineWidth, Font = font.Text, FontSize = fontSize };
            try { if (!apply(next)) { error.Text = "Сочетание занято. Прежнее сохранено."; return; } Close(); }
            catch (Exception e) { error.Text = "Не удалось сохранить настройки: " + e.Message; }
        }, 104);
        save.Tag = "primary"; actions.Children.Add(save);
    }
}
