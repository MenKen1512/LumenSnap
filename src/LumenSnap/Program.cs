using System.Diagnostics;
using System.Threading;
using System.Windows.Interop;
using System.Windows.Threading;

namespace LumenSnap;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length == 2 && args[0] == "--smoke") return Smoke.Run(Path.GetFullPath(args[1]));
        uint activation = Native.RegisterWindowMessage("LumenSnap.Activate.v1");
        if (args.Contains("--quit")) { Native.PostMessage(new IntPtr(0xffff), activation, new IntPtr(1), IntPtr.Zero); return 0; }
        using var single = new Mutex(true, "Local\\LumenSnap-" + Environment.UserName, out bool first);
        if (!first) { Native.PostMessage(new IntPtr(0xffff), activation, IntPtr.Zero, IntPtr.Zero); return 0; }
        var app = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown }; Theme.Install(app);
        Settings settings = Settings.Load(); Overlay? capture = null; SettingsWindow? preferences = null;
        using var source = new HwndSource(new HwndSourceParameters("LumenSnap.Tray") { Width = 0, Height = 0, WindowStyle = 0 });
        using var hotkey = new Hotkey(source);
        using var trayIcon = new System.Drawing.Icon(typeof(Program).Assembly.GetManifestResourceStream("LumenSnap.app.ico") ?? throw new InvalidOperationException("Missing application icon"));
        using var tray = new System.Windows.Forms.NotifyIcon { Icon = trayIcon, Text = "LumenSnap", Visible = true };
        var menu = TrayTheme.Menu();
        void Notify(string message)
        { tray.BalloonTipTitle = "LumenSnap"; tray.BalloonTipText = message; tray.ShowBalloonTip(4000); }
        void Persist(Settings next)
        {
            settings = next;
            try { settings.Save(); }
            catch (Exception e) { Notify("Не удалось сохранить настройки: " + e.Message); }
        }
        void Capture()
        {
            if (capture != null) { capture.Activate(); return; }
            if (preferences != null) { preferences.Activate(); return; }
            // Deferred until the tray menu has disappeared; snapshot is taken before constructing overlay UI.
            var delay = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(160) };
            delay.Tick += (_, _) =>
            {
                delay.Stop(); if (capture != null) return;
                try
                {
                    var desktop = Native.Desktop; var bitmap = Native.Capture(desktop);
                    capture = new Overlay(bitmap, desktop, settings, Persist);
                    capture.Completed += message => { if (settings.Notifications) Notify(message); };
                    capture.Closed += (_, _) => capture = null; capture.Show();
                }
                catch (Exception e) { capture = null; Notify("Не удалось захватить экран: " + e.Message); }
            };
            delay.Start();
        }
        void Preferences()
        {
            if (preferences != null) { preferences.Activate(); return; }
            preferences = new SettingsWindow(settings, next =>
            {
                var old = settings;
                if (!hotkey.TrySet(next.HotkeyCode, next.HotkeyModifiers)) return false;
                try { next.Save(); next.ApplyStartup(); settings = next; return true; }
                catch { hotkey.TrySet(old.HotkeyCode, old.HotkeyModifiers); old.Save(); throw; }
            });
            preferences.Closed += (_, _) => preferences = null; preferences.Show();
        }
        menu.Items.Add("Новый скриншот", null, (_, _) => app.Dispatcher.BeginInvoke(Capture));
        menu.Items.Add("Настройки", null, (_, _) => app.Dispatcher.BeginInvoke(Preferences));
        menu.Items.Add("Выход", null, (_, _) => app.Shutdown()); tray.ContextMenuStrip = menu;
        tray.DoubleClick += (_, _) => app.Dispatcher.BeginInvoke(Capture);
        hotkey.Pressed += Capture;
        source.AddHook((IntPtr h, int msg, IntPtr w, IntPtr l, ref bool handled) =>
        {
            if ((uint)msg == activation) { handled = true; if (w == new IntPtr(1)) app.Shutdown(); else Capture(); }
            return IntPtr.Zero;
        });
        app.Exit += (_, _) => { capture?.Close(); preferences?.Close(); tray.Visible = false; menu.Dispose(); };
        if (!hotkey.TrySet(settings.HotkeyCode, settings.HotkeyModifiers))
            app.Dispatcher.BeginInvoke(() => Notify("Сочетание " + settings.HotkeyLabel + " занято. Используйте меню трея и выберите другую клавишу в настройках."));
        try { return app.Run(); }
        finally { single.ReleaseMutex(); }
    }
}
