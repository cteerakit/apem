using Mango.Interop;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Mango.Services;

public sealed class HotkeyService : IDisposable
{
    private readonly OverlayWindowService _overlayService;
    private readonly AppSettings _settings;
    private readonly DispatcherQueue _dispatcherQueue;
    private Window? _messageWindow;
    private bool _registered;

    public HotkeyService(OverlayWindowService overlayService, AppSettings settings, DispatcherQueue dispatcherQueue)
    {
        _overlayService = overlayService;
        _settings = settings;
        _dispatcherQueue = dispatcherQueue;
    }

    public void Register(Window shellWindow)
    {
        _messageWindow = shellWindow;
        ApplyRegistrations();
    }

    public bool ApplyRegistrations()
    {
        if (_messageWindow is null)
        {
            return false;
        }

        var hwnd = Win32WindowHelper.GetHandle(_messageWindow);
        UnregisterAll(hwnd);

        var overlayOk = RegisterBinding(hwnd, NativeMethods.HotkeyToggleOverlay, _settings.ToggleOverlayHotkey);
        var interactiveOk = RegisterBinding(hwnd, NativeMethods.HotkeyToggleInteractive, _settings.ToggleInteractiveHotkey);
        _registered = overlayOk || interactiveOk;
        return overlayOk && interactiveOk;
    }

    public bool TryHandleHotkeyMessage(nint hwnd, uint message, nint wParam)
    {
        if (message != NativeMethods.WmHotkey)
        {
            return false;
        }

        var id = wParam.ToInt32();
        _dispatcherQueue.TryEnqueue(() =>
        {
            switch (id)
            {
                case NativeMethods.HotkeyToggleOverlay:
                    _overlayService.ToggleOverlayVisibility();
                    break;
                case NativeMethods.HotkeyToggleInteractive:
                    _overlayService.ToggleInteractive();
                    break;
            }
        });

        return true;
    }

    public void Dispose()
    {
        if (_messageWindow is null)
        {
            return;
        }

        UnregisterAll(Win32WindowHelper.GetHandle(_messageWindow));
        _registered = false;
    }

    private static bool RegisterBinding(nint hwnd, int id, HotkeyBinding binding)
    {
        if (binding.VirtualKey == 0)
        {
            return false;
        }

        var modifiers = NativeMethods.ModNorepeat;
        if (binding.Alt)
        {
            modifiers |= NativeMethods.ModAlt;
        }

        if (binding.Ctrl)
        {
            modifiers |= NativeMethods.ModControl;
        }

        if (binding.Shift)
        {
            modifiers |= NativeMethods.ModShift;
        }

        if (binding.Win)
        {
            modifiers |= NativeMethods.ModWin;
        }

        // Require at least one modifier for global hotkeys safety.
        if ((modifiers & ~NativeMethods.ModNorepeat) == 0)
        {
            return false;
        }

        return NativeMethods.RegisterHotKey(hwnd, id, modifiers, (uint)binding.VirtualKey);
    }

    private static void UnregisterAll(nint hwnd)
    {
        NativeMethods.UnregisterHotKey(hwnd, NativeMethods.HotkeyToggleOverlay);
        NativeMethods.UnregisterHotKey(hwnd, NativeMethods.HotkeyToggleInteractive);
    }
}
