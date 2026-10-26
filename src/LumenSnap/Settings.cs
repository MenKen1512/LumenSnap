using System.Text.Json;
using Microsoft.Win32;

namespace LumenSnap;

public sealed record Settings
{
    public int Schema { get; init; } = 1;
    public uint HotkeyCode { get; init; } = 0x53;
    public uint HotkeyModifiers { get; init; } = 3; // Ctrl+Alt+S; never silently claim PrintScreen.
    public bool StartAtLogin { get; init; }
    public bool Notifications { get; init; }
    public string Color { get; init; } = "#65B7FF";
    public double Width { get; init; } = 4;
    public string Font { get; init; } = "Segoe UI";
    public double FontSize { get; init; } = 26;
    public bool Glow { get; init; } = true;
    public string LastFolder { get; init; } = "";
    public int Format { get; init; } = 1;
    public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LumenSnap", "settings.json");
    public static Settings Load(string? path = null)
    {
        try
        {
            var s = JsonSerializer.Deserialize<Settings>(File.ReadAllText(path ?? DefaultPath));
            if (s is null || s.Schema != 1 || !ValidColor(s.Color) || !double.IsFinite(s.Width) || s.Width is < 1 or > 40 ||
                !double.IsFinite(s.FontSize) || s.FontSize is < 8 or > 200 || string.IsNullOrWhiteSpace(s.Font) || s.Font.Length > 200 ||
                s.Format is < 1 or > 3 || s.HotkeyCode is < 1 or > 254 || s.HotkeyModifiers > 15) return new();
            return s;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException or ArgumentException) { return new(); }
    }
    public static bool ValidColor(string? value) => value is { Length: 7 } && value[0] == '#' && value.Skip(1).All(Uri.IsHexDigit);
    public void Save(string? path = null)
    {
        path ??= DefaultPath; Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
        string temp = path + ".tmp";
        try { File.WriteAllText(temp, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true })); File.Move(temp, path, true); }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }
    internal void ApplyStartup()
    {
        using var run = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
        if (StartAtLogin) run.SetValue("LumenSnap", "\"" + Environment.ProcessPath + "\"");
        else run.DeleteValue("LumenSnap", false);
    }
    public string HotkeyLabel => string.Join("+", new[] { (HotkeyModifiers & 2) != 0 ? "Ctrl" : null,
        (HotkeyModifiers & 1) != 0 ? "Alt" : null, (HotkeyModifiers & 4) != 0 ? "Shift" : null,
        (HotkeyModifiers & 8) != 0 ? "Win" : null, KeyInterop.KeyFromVirtualKey((int)HotkeyCode).ToString() }.Where(x => x != null));
}
