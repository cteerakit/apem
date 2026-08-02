using Apem.Models;
using Apem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Apem.ViewModels;

public sealed partial class ShellViewModel : ObservableObject
{
    private readonly AppSettings _settings = AppServices.Settings;

    [ObservableProperty]
    private string _gsiStatus = "Starting…";

    [ObservableProperty]
    private string _installMessage = string.Empty;

    [ObservableProperty]
    private bool _gsiConfigInstalled;

    [ObservableProperty]
    private string _connectionStatus = "Waiting for Dota 2";

    [ObservableProperty]
    private string _lastUpdate = "—";

    [ObservableProperty]
    private double _overlayOpacity;

    [ObservableProperty]
    private bool _isTurboMode;

    [ObservableProperty]
    private bool _timerSoundsEnabled;

    [ObservableProperty]
    private bool _showScoreboardPanel;

    [ObservableProperty]
    private bool _showPlayerPanel;

    [ObservableProperty]
    private bool _showItemsPanel;

    [ObservableProperty]
    private bool _showAbilitiesPanel;

    [ObservableProperty]
    private bool _showTimersPanel;

    [ObservableProperty]
    private bool _showDraftPanel;

    [ObservableProperty]
    private bool _showBuildPanel;

    public ShellViewModel()
    {
        OverlayOpacity = _settings.OverlayOpacity;
        IsTurboMode = _settings.IsTurboMode;
        TimerSoundsEnabled = _settings.TimerSoundsEnabled;
        ShowScoreboardPanel = _settings.ShowScoreboardPanel;
        ShowPlayerPanel = _settings.ShowPlayerPanel;
        ShowItemsPanel = _settings.ShowItemsPanel;
        ShowAbilitiesPanel = _settings.ShowAbilitiesPanel;
        ShowTimersPanel = _settings.ShowTimersPanel;
        ShowDraftPanel = _settings.ShowDraftPanel;
        ShowBuildPanel = _settings.ShowBuildPanel;

        GsiStatus = $"Listening on 127.0.0.1:{_settings.GsiPort}";
        AppServices.GsiListener.StatusChanged += status => GsiStatus = status;
        AppServices.MatchStore.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(MatchStore.ConnectionStatus) or null)
            {
                ConnectionStatus = AppServices.MatchStore.ConnectionStatus;
            }

            if (e.PropertyName is nameof(MatchStore.LastPayloadUtc) or null)
            {
                LastUpdate = AppServices.MatchStore.LastPayloadUtc?.ToLocalTime().ToString("T") ?? "—";
            }
        };

        var install = AppServices.EnsureGsiConfigInstalled();
        GsiConfigInstalled = install.Success;
        InstallMessage = install.Message;
    }

    [RelayCommand]
    private void ReinstallGsiConfig()
    {
        var result = AppServices.GsiInstaller.Install(_settings);
        GsiConfigInstalled = result.Success;
        InstallMessage = result.Message;
    }

    [RelayCommand]
    private void ShowOverlay()
    {
        AppServices.OverlayService.ShowOverlay();
    }

    [RelayCommand]
    private void HideOverlay()
    {
        AppServices.OverlayService.HideOverlay();
    }

    [RelayCommand]
    private void SaveSettings()
    {
        _settings.OverlayOpacity = OverlayOpacity;
        _settings.IsTurboMode = IsTurboMode;
        _settings.TimerSoundsEnabled = TimerSoundsEnabled;
        _settings.ShowScoreboardPanel = ShowScoreboardPanel;
        _settings.ShowPlayerPanel = ShowPlayerPanel;
        _settings.ShowItemsPanel = ShowItemsPanel;
        _settings.ShowAbilitiesPanel = ShowAbilitiesPanel;
        _settings.ShowTimersPanel = ShowTimersPanel;
        _settings.ShowDraftPanel = ShowDraftPanel;
        _settings.ShowBuildPanel = ShowBuildPanel;
        _settings.Save();
        AppServices.OverlayService.ApplySettings();
    }
}
