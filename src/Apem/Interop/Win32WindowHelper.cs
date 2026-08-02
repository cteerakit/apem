using WinRT.Interop;

namespace Apem.Interop;

internal static class Win32WindowHelper
{
    public static nint GetHandle(Microsoft.UI.Xaml.Window window) =>
        WindowNative.GetWindowHandle(window);

    public static void SetClickThrough(nint hwnd, bool clickThrough)
    {
        var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GwlExStyle).ToInt64();
        exStyle |= NativeMethods.WsExLayered | NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow;

        if (clickThrough)
        {
            exStyle |= NativeMethods.WsExTransparent;
        }
        else
        {
            exStyle &= ~NativeMethods.WsExTransparent;
        }

        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GwlExStyle, new nint(exStyle));
        NativeMethods.SetLayeredWindowAttributes(hwnd, 0, 255, NativeMethods.LwaAlpha);

        var margins = new NativeMethods.Margins { Left = -1, Right = -1, Top = -1, Bottom = -1 };
        NativeMethods.DwmExtendFrameIntoClientArea(hwnd, ref margins);
    }

    public static void ConfigureOverlayWindow(Microsoft.UI.Xaml.Window window, bool clickThrough)
    {
        var hwnd = GetHandle(window);
        if (hwnd == 0)
        {
            return;
        }

        SetClickThrough(hwnd, clickThrough);
    }
}
