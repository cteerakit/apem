using Microsoft.UI.Xaml;
using Windows.Foundation;
using WinUIEx;

namespace Apem.Interop;

/// <summary>
/// Click-through for the WinUI overlay. Window regions keep empty screen out of the
/// hit-test area entirely; watch mode additionally makes the panels themselves pass
/// clicks to the game.
/// </summary>
internal static class OverlayClickThroughHelper
{
    private const long ClickThroughStyles = NativeMethods.WsExTransparent | NativeMethods.WsExLayered;

    public static void Configure(Window window, bool clickThrough, bool topmost, IReadOnlyList<FrameworkElement> hitElements)
    {
        var hwnd = Win32WindowHelper.GetHandle(window);
        if (hwnd == 0)
        {
            return;
        }

        Win32WindowHelper.SetTopmost(hwnd, topmost);
        ApplyToolWindowStyles(hwnd);

        // Clip the HWND to panel bounds so empty screen is not a black/opaque hit surface.
        ApplyPanelRegion(window, hitElements);

        // Watch mode: pass all clicks (including over panels) to the game.
        // Interactive: panels receive input; empty areas still pass through via the region.
        SetClickThroughStyles(hwnd, clickThrough);
    }

    public static void Reset(Window window)
    {
        var hwnd = Win32WindowHelper.GetHandle(window);
        if (hwnd == 0)
        {
            return;
        }

        SetClickThroughStyles(hwnd, clickThrough: false);
        window.SetRegion(null);
    }

    private static void ApplyPanelRegion(Window window, IReadOnlyList<FrameworkElement> hitElements)
    {
        Region? combined = null;

        foreach (var element in hitElements)
        {
            if (element.Visibility != Visibility.Visible ||
                element.ActualWidth <= 0 ||
                element.ActualHeight <= 0)
            {
                continue;
            }

            var transform = element.TransformToVisual(window.Content);
            var bounds = transform.TransformBounds(
                new Rect(0, 0, element.ActualWidth, element.ActualHeight));

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                continue;
            }

            // Slight padding so rounded panel edges stay interactive.
            var padded = new Rect(
                Math.Max(0, bounds.X - 2),
                Math.Max(0, bounds.Y - 2),
                bounds.Width + 4,
                bounds.Height + 4);

            var part = Region.CreateRectangle(padded);
            combined = combined is null ? part : combined + part;
        }

        // Tiny 1x1 region keeps the HWND alive if nothing is visible yet.
        combined ??= Region.CreateRectangle(new Rect(0, 0, 1, 1));
        window.SetRegion(combined);
    }

    private static void ApplyToolWindowStyles(nint hwnd)
    {
        var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GwlExStyle).ToInt64();
        exStyle |= NativeMethods.WsExNoActivate | NativeMethods.WsExToolWindow;
        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GwlExStyle, new nint(exStyle));
    }

    /// <summary>
    /// WS_EX_TRANSPARENT is only honoured for hit-testing when the window is also
    /// layered, so watch mode needs both styles; with only WS_EX_TRANSPARENT the XAML
    /// content window still swallows every click over a widget. A fully opaque layer
    /// alpha leaves the rendered content untouched.
    /// </summary>
    private static void SetClickThroughStyles(nint hwnd, bool clickThrough)
    {
        var exStyle = NativeMethods.GetWindowLongPtr(hwnd, NativeMethods.GwlExStyle).ToInt64();
        var updated = clickThrough
            ? exStyle | ClickThroughStyles
            : exStyle & ~ClickThroughStyles;

        if (updated == exStyle)
        {
            return;
        }

        NativeMethods.SetWindowLongPtr(hwnd, NativeMethods.GwlExStyle, new nint(updated));

        if (clickThrough)
        {
            NativeMethods.SetLayeredWindowAttributes(hwnd, 0, 255, NativeMethods.LwaAlpha);
        }

        NativeMethods.SetWindowPos(
            hwnd,
            0,
            0,
            0,
            0,
            0,
            NativeMethods.SwpNomove |
            NativeMethods.SwpNosize |
            NativeMethods.SwpNozorder |
            NativeMethods.SwpFramechanged |
            NativeMethods.SwpNoactivate);
    }
}
