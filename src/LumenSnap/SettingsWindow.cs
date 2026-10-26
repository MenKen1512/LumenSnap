namespace LumenSnap;

internal sealed class SettingsWindow : Window
{
    internal SettingsWindow(Settings current, Func<Settings, bool> apply)
    {
        Title = "LumenSnap — Настройки"; Width = 450; Height = 580; ResizeMode = ResizeMode.NoResize;
        WindowStartupLocation = WindowStartupLocation.CenterScreen; Background = Theme.Panel; Foreground = Theme.Foreground;
        var stack = new StackPanel { Margin = new Thickness(24) }; Content = stack;
        void Label(string t) => stack.Children.Add(new TextBlock { Text = t, Margin = new Thickness(0, 12, 0, 5), TextWrapping = TextWrapping.Wrap });
        Label("Горячая клавиша захвата (нажмите сочетание)");
        uint key = current.HotkeyCode, mods = current.HotkeyModifiers;
        var binding = new TextBox { Text = current.HotkeyLabel, IsReadOnly = true, FontSize = 18, Padding = new Thickness(8) };
        binding.PreviewKeyDown += (_, e) =>
        {
            e.Handled = true; var k = e.Key == Key.System ? e.SystemKey : e.Key;
            if (k is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin) return;
            key = (uint)KeyInterop.VirtualKeyFromKey(k); var m = Keyboard.Modifiers;
            mods = ((m & ModifierKeys.Alt) != 0 ? 1u : 0) | ((m & ModifierKeys.Control) != 0 ? 2u : 0) |
                ((m & ModifierKeys.Shift) != 0 ? 4u : 0) | ((m & ModifierKeys.Windows) != 0 ? 8u : 0);
            binding.Text = (current with { HotkeyCode = key, HotkeyModifiers = mods }).HotkeyLabel;
        };
        stack.Children.Add(binding);
        stack.Children.Add(Theme.Button("Сбросить: Ctrl+Alt+S", "Сбросить сочетание", () => { key = 0x53; mods = 3; binding.Text = "Ctrl+Alt+S"; }, 250));
        var login = new CheckBox { Content = "Запускать при входе в Windows", IsChecked = current.StartAtLogin, Margin = new Thickness(0, 12, 0, 8) }; stack.Children.Add(login);
        var notifications = new CheckBox { Content = "Уведомлять после копирования и сохранения", IsChecked = current.Notifications }; stack.Children.Add(notifications);
        var glow = new CheckBox { Content = "Мягкое свечение стрелок", IsChecked = current.Glow, Margin = new Thickness(0, 8, 0, 0) }; stack.Children.Add(glow);
        Label("Цвет HEX / толщина / размер текста");
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        var color = new TextBox { Text = current.Color, Width = 100, Margin = new Thickness(2) };
        var width = new TextBox { Text = current.Width.ToString(System.Globalization.CultureInfo.InvariantCulture), Width = 60, Margin = new Thickness(2) };
        var size = new TextBox { Text = current.FontSize.ToString(System.Globalization.CultureInfo.InvariantCulture), Width = 60, Margin = new Thickness(2) };
        row.Children.Add(color); row.Children.Add(width); row.Children.Add(size); stack.Children.Add(row);
        Label("Шрифт текста");
        var font = new ComboBox { IsEditable = true, Text = current.Font, ItemsSource = Fonts.SystemFontFamilies.Select(x => x.Source).OrderBy(x => x).ToArray() }; stack.Children.Add(font);
        var error = new TextBlock { Foreground = Brushes.Orange, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 0) }; stack.Children.Add(error);
        Label("Если клавиша занята, прежнее сочетание сохранится. Клавиши производителя доступны только если Windows получает событие клавиатуры.");
        stack.Children.Add(Theme.Button("Сохранить", "Сохранить настройки", () =>
        {
            if (!Settings.ValidColor(color.Text) || !double.TryParse(width.Text, out double w) || !double.IsFinite(w) || w is < 1 or > 40 || !double.TryParse(size.Text, out double s) || !double.IsFinite(s) || s is < 8 or > 200 || !Settings.ValidFont(font.Text))
            { error.Text = "Проверьте цвет #RRGGBB, толщину 1–40 и размер 8–200."; return; }
            var next = current with { HotkeyCode = key, HotkeyModifiers = mods, StartAtLogin = login.IsChecked == true,
                Notifications = notifications.IsChecked == true, Glow = glow.IsChecked == true, Color = color.Text, Width = w, Font = font.Text, FontSize = s };
            try { if (!apply(next)) { error.Text = "Не удалось зарегистрировать сочетание: оно занято или зарезервировано."; return; } Close(); }
            catch (Exception e) { error.Text = "Не удалось сохранить настройки: " + e.Message; }
        }, 150));
    }
}
