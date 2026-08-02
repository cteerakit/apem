using Apem.Interop;
using Apem.Views.Overlay;
using Microsoft.UI.Xaml;

namespace Apem.Services;

public sealed class OverlayWindowService
{
    private readonly AppSettings _settings;
    private OverlayWindow? _overlayWindow;

    public OverlayWindowService(AppSettings settings)
    {
        _settings = settings;
    }

    public OverlayWindow EnsureOverlay()
    {
        if (_overlayWindow is not null)
        {
            return _overlayWindow;
        }

        _overlayWindow = new OverlayWindow();
        ConfigureOverlayBounds();
        return _overlayWindow;
    }

    public void ShowOverlay()
    {
        var window = EnsureOverlay();
        window.ConfigureNativeWindow(clickThrough: !_settings.OverlayInteractive);
        window.AppWindow.Show();
        _settings.OverlayVisible = true;
        _settings.Save();
    }

    public void HideOverlay()
    {
        if (_overlayWindow is null)
        {
            _settings.OverlayVisible = false;
            _settings.Save();
            return;
        }

        _overlayWindow.AppWindow.Hide();
        _settings.OverlayVisible = false;
        _settings.Save();
    }

    public void ToggleOverlayVisibility()
    {
        if (_settings.OverlayVisible)
        {
            HideOverlay();
        }
        else
        {
            ShowOverlay();
        }
    }

    public void SetInteractive(bool interactive)
    {
        _settings.OverlayInteractive = interactive;
        _settings.Save();

        if (_overlayWindow is null)
        {
            return;
        }

        _overlayWindow.ConfigureNativeWindow(clickThrough: !interactive);
        _overlayWindow.SetInteractiveMode(interactive);
    }

    public void ApplySettings()
    {
        if (_overlayWindow is not null)
        {
            _overlayWindow.ApplySettingsFromStore();
            _overlayWindow.ConfigureNativeWindow(clickThrough: !_settings.OverlayInteractive);
        }
    }

    public void ToggleInteractive() => SetInteractive(!_settings.OverlayInteractive);

    public void Close()
    {
        if (_overlayWindow is null)
        {
            return;
        }

        _overlayWindow.Close();
        _overlayWindow = null;
        _settings.OverlayVisible = false;
    }

    private void ConfigureOverlayBounds()
    {
        if (_overlayWindow is null)
        {
            return;
        }

        var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(
            _overlayWindow.AppWindow.Id,
            Microsoft.UI.Windowing.DisplayAreaFallback.Primary);

        if (displayArea is not null)
        {
            _overlayWindow.AppWindow.MoveAndResize(displayArea.WorkArea);
        }

        _overlayWindow.AppWindow.IsShownInSwitchers = false;
    }
}
