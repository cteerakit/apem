using System.Runtime.InteropServices;

namespace Mango.Interop;

internal static class NativeMethods
{
    public const int GwlExStyle = -20;
    public const int WsExLayered = 0x00080000;
    public const int WsExTransparent = 0x00000020;
    public const int WsExNoActivate = 0x08000000;
    public const int WsExToolWindow = 0x00000080;

    public const uint LwaAlpha = 0x00000002;

    public const uint ModAlt = 0x0001;
    public const uint ModControl = 0x0002;
    public const uint ModShift = 0x0004;
    public const uint ModWin = 0x0008;
    public const uint ModNorepeat = 0x4000;
    public const int WmHotkey = 0x0312;

    public const int HotkeyToggleOverlay = 1;
    public const int HotkeyToggleInteractive = 2;

    public static readonly nint HwndTopmost = new(-1);
    public static readonly nint HwndNotTopmost = new(-2);

    public const uint SwpNomove = 0x0002;
    public const uint SwpNosize = 0x0001;
    public const uint SwpNoactivate = 0x0010;
    public const uint SwpShowwindow = 0x0040;
    public const uint SwpNozorder = 0x0004;
    public const uint SwpFramechanged = 0x0020;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern nint GetWindowLongPtr64(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern nint GetWindowLongPtr32(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern nint SetWindowLongPtr64(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern nint SetWindowLongPtr32(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetLayeredWindowAttributes(nint hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool UnregisterHotKey(nint hWnd, int id);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool SetWindowPos(
        nint hWnd,
        nint hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

    public static nint GetWindowLongPtr(nint hWnd, int nIndex) =>
        nint.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : GetWindowLongPtr32(hWnd, nIndex);

    public static nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong) =>
        nint.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
}
