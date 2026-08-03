using WinRT.Interop;

namespace Apem.Interop;

internal static class Win32WindowHelper
{
    public static nint GetHandle(Microsoft.UI.Xaml.Window window) =>
        WindowNative.GetWindowHandle(window);

    public static void SetTopmost(nint hwnd, bool topmost)
    {
        if (hwnd == 0)
        {
            return;
        }

        NativeMethods.SetWindowPos(
            hwnd,
            topmost ? NativeMethods.HwndTopmost : NativeMethods.HwndNotTopmost,
            0,
            0,
            0,
            0,
            NativeMethods.SwpNomove | NativeMethods.SwpNosize | NativeMethods.SwpNoactivate);
    }
}
