using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace LumenSnap;

internal static class Native
{
    [DllImport("user32.dll")] internal static extern int GetSystemMetrics(int index);
    [DllImport("user32.dll")] internal static extern IntPtr GetDC(IntPtr hwnd);
    [DllImport("user32.dll")] internal static extern int ReleaseDC(IntPtr hwnd, IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern IntPtr CreateCompatibleDC(IntPtr dc);
    [DllImport("gdi32.dll")] internal static extern IntPtr CreateCompatibleBitmap(IntPtr dc, int w, int h);
    [DllImport("gdi32.dll")] internal static extern IntPtr SelectObject(IntPtr dc, IntPtr obj);
    [DllImport("gdi32.dll")] internal static extern bool DeleteObject(IntPtr obj);
    [DllImport("gdi32.dll")] internal static extern bool DeleteDC(IntPtr dc);
    [DllImport("gdi32.dll", SetLastError = true)] internal static extern bool BitBlt(IntPtr target, int x, int y, int w, int h, IntPtr source, int sx, int sy, uint flags);
    [DllImport("user32.dll", SetLastError = true)] internal static extern bool RegisterHotKey(IntPtr hwnd, int id, uint modifiers, uint key);
    [DllImport("user32.dll")] internal static extern bool UnregisterHotKey(IntPtr hwnd, int id);
    [DllImport("user32.dll")] internal static extern bool SetWindowPos(IntPtr hwnd, IntPtr after, int x, int y, int w, int h, uint flags);
    [DllImport("user32.dll")] internal static extern uint GetDpiForWindow(IntPtr hwnd);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] internal static extern uint RegisterWindowMessage(string name);
    [DllImport("user32.dll")] internal static extern bool PostMessage(IntPtr hwnd, uint msg, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] internal static extern bool GetCursorPos(out NativePoint point);
    [DllImport("user32.dll")] internal static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] internal static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extra);
    [DllImport("user32.dll")] internal static extern IntPtr MonitorFromPoint(NativePoint point, uint flags);
    [DllImport("shcore.dll")] internal static extern int GetDpiForMonitor(IntPtr monitor, int type, out uint x, out uint y);
    [StructLayout(LayoutKind.Sequential)] internal struct NativePoint { public int X, Y; }

    internal static Rect Desktop => new(GetSystemMetrics(76), GetSystemMetrics(77), GetSystemMetrics(78), GetSystemMetrics(79));
    internal static BitmapSource Capture(Rect desktop)
    {
        if (desktop.Width < 1 || desktop.Height < 1 || desktop.Width * desktop.Height > 150_000_000)
            throw new InvalidOperationException("Рабочий стол слишком велик для безопасного захвата.");
        IntPtr source = GetDC(IntPtr.Zero), memory = IntPtr.Zero, bitmap = IntPtr.Zero, previous = IntPtr.Zero;
        try
        {
            if (source == IntPtr.Zero) throw new Win32Exception();
            memory = CreateCompatibleDC(source); bitmap = CreateCompatibleBitmap(source, (int)desktop.Width, (int)desktop.Height);
            if (memory == IntPtr.Zero || bitmap == IntPtr.Zero) throw new Win32Exception();
            previous = SelectObject(memory, bitmap);
            if (!BitBlt(memory, 0, 0, (int)desktop.Width, (int)desktop.Height, source, (int)desktop.X, (int)desktop.Y, 0x40CC0020))
                throw new Win32Exception(Marshal.GetLastWin32Error());
            var image = Imaging.CreateBitmapSourceFromHBitmap(bitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            // GDI capture is opaque, even on drivers that leave the alpha channel undefined.
            var opaque = new FormatConvertedBitmap(image, PixelFormats.Bgr32, null, 0); opaque.Freeze(); return opaque;
        }
        finally
        {
            if (previous != IntPtr.Zero) SelectObject(memory, previous);
            if (bitmap != IntPtr.Zero) DeleteObject(bitmap);
            if (memory != IntPtr.Zero) DeleteDC(memory);
            if (source != IntPtr.Zero) ReleaseDC(IntPtr.Zero, source);
        }
    }
}

internal sealed class Hotkey : IDisposable
{
    private readonly HwndSource source;
    private int id = 4100;
    private uint key, modifiers;
    internal event Action? Pressed;
    internal Hotkey(HwndSource source) { this.source = source; source.AddHook(Hook); }
    internal bool TrySet(uint newKey, uint newModifiers)
    {
        if (key == newKey && modifiers == newModifiers && key != 0) return true;
        int next = id == 4100 ? 4101 : 4100;
        // Register replacement before releasing old binding. A conflict must preserve the old key.
        if (!Native.RegisterHotKey(source.Handle, next, newModifiers | 0x4000, newKey)) return false;
        Native.UnregisterHotKey(source.Handle, id); id = next; key = newKey; modifiers = newModifiers; return true;
    }
    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr w, IntPtr l, ref bool handled)
    { if (msg == 0x312 && w.ToInt32() == id) { handled = true; Pressed?.Invoke(); } return IntPtr.Zero; }
    public void Dispose() { Native.UnregisterHotKey(source.Handle, id); source.RemoveHook(Hook); }
}
