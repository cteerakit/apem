using Apem.Services;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Apem.Services;

public sealed class HotkeyService : IDisposable
{
    private readonly OverlayWindowService _overlayService;
    private readonly DispatcherQueue _dispatcherQueue;
    private Window? _messageWindow;
    private bool _registered;

    public HotkeyService(OverlayWindowService overlayService, AppSettings settings, DispatcherQueue dispatcherQueue)
    {
        _overlayService = overlayService;
        _dispatcherQueue = dispatcherQueue;
        _ = settings;
    }

    public void Register(Window shellWindow)
    {
        if (_registered)
        {
            return;
        }

        _messageWindow = shellWindow;
        var hwnd = Interop.Win32WindowHelper.GetHandle(shellWindow);

        Interop.NativeMethods.RegisterHotKey(
            hwnd,
            Interop.NativeMethods.HotkeyToggleOverlay,
            Interop.NativeMethods.ModAlt | Interop.NativeMethods.ModNorepeat,
            Interop.NativeMethods.VkF9);

        Interop.NativeMethods.RegisterHotKey(
            hwnd,
            Interop.NativeMethods.HotkeyToggleInteractive,
            Interop.NativeMethods.ModAlt | Interop.NativeMethods.ModNorepeat,
            Interop.NativeMethods.VkOem3);

        _registered = true;
    }

    public bool TryHandleHotkeyMessage(nint hwnd, uint message, nint wParam)
    {
        if (message != Interop.NativeMethods.WmHotkey)
        {
            return false;
        }

        var id = wParam.ToInt32();
        _dispatcherQueue.TryEnqueue(() =>
        {
            switch (id)
            {
                case Interop.NativeMethods.HotkeyToggleOverlay:
                    _overlayService.ToggleOverlayVisibility();
                    break;
                case Interop.NativeMethods.HotkeyToggleInteractive:
                    _overlayService.ToggleInteractive();
                    break;
            }
        });

        return true;
    }

    public void Dispose()
    {
        if (_messageWindow is not null)
        {
            var hwnd = Interop.Win32WindowHelper.GetHandle(_messageWindow);
            Interop.NativeMethods.UnregisterHotKey(hwnd, Interop.NativeMethods.HotkeyToggleOverlay);
            Interop.NativeMethods.UnregisterHotKey(hwnd, Interop.NativeMethods.HotkeyToggleInteractive);
        }
    }
}
