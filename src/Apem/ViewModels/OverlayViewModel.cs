using System.Collections.ObjectModel;
using Apem.Models;
using Apem.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Apem.ViewModels;

public sealed partial class OverlayViewModel : ObservableObject
{
    private readonly MatchStore _store = AppServices.MatchStore;
    private readonly TimerService _timers = AppServices.TimerService;
    private readonly OpenDotaService _openDota = AppServices.OpenDota;
    private readonly AppSettings _settings = AppServices.Settings;

    [ObservableProperty]
    private MatchSnapshot _snapshot = new();

    [ObservableProperty]
    private double _overlayOpacity = 0.92;

    [ObservableProperty]
    private bool _isInteractive;

    [ObservableProperty]
    private bool _showScoreboardPanel = true;

    [ObservableProperty]
    private bool _showPlayerPanel = true;

    [ObservableProperty]
    private bool _showItemsPanel = true;

    [ObservableProperty]
    private bool _showAbilitiesPanel = true;

    [ObservableProperty]
    private bool _showTimersPanel = true;

    [ObservableProperty]
    private bool _showDraftPanel = true;

    [ObservableProperty]
    private bool _showBuildPanel = true;

    public ObservableCollection<SlotSnapshot> Items { get; } = [];
    public ObservableCollection<SlotSnapshot> Abilities { get; } = [];
    public ObservableCollection<TimerEntry> ObjectiveTimers { get; } = [];
    public ObservableCollection<CounterSuggestion> CounterSuggestions { get; } = [];
    public ObservableCollection<BuildSuggestionItem> BuildSuggestions { get; } = [];

    public PanelLayoutSettings Layout => _settings.PanelLayout;

    public OverlayViewModel()
    {
        OverlayOpacity = _settings.OverlayOpacity;
        IsInteractive = _settings.OverlayInteractive;
        ShowScoreboardPanel = _settings.ShowScoreboardPanel;
        ShowPlayerPanel = _settings.ShowPlayerPanel;
        ShowItemsPanel = _settings.ShowItemsPanel;
        ShowAbilitiesPanel = _settings.ShowAbilitiesPanel;
        ShowTimersPanel = _settings.ShowTimersPanel;
        ShowDraftPanel = _settings.ShowDraftPanel;
        ShowBuildPanel = _settings.ShowBuildPanel;

        _store.SnapshotUpdated += OnSnapshotUpdated;
        _timers.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(TimerService.Timers) or null)
            {
                SyncTimers();
            }
        };

        SyncTimers();
    }

    private async void OnSnapshotUpdated(MatchSnapshot snapshot)
    {
        Snapshot = snapshot;
        Items.Clear();
        foreach (var item in snapshot.Items)
        {
            Items.Add(item);
        }

        Abilities.Clear();
        foreach (var ability in snapshot.Abilities)
        {
            Abilities.Add(ability);
        }

        if (snapshot.IsInDraft)
        {
            await RefreshDraftDataAsync(snapshot);
        }

        if (!string.IsNullOrWhiteSpace(snapshot.HeroName))
        {
            await RefreshBuildDataAsync(snapshot.HeroName);
        }
    }

    private async Task RefreshDraftDataAsync(MatchSnapshot snapshot)
    {
        CounterSuggestions.Clear();
        var suggestions = await _openDota.GetCounterSuggestionsAsync(snapshot.Draft.EnemyHeroIds);
        foreach (var suggestion in suggestions)
        {
            CounterSuggestions.Add(suggestion);
        }
    }

    private async Task RefreshBuildDataAsync(string heroInternalName)
    {
        var heroes = await _openDota.GetHeroesAsync();
        var heroId = _openDota.ResolveHeroId(heroInternalName, heroes);
        if (heroId is null)
        {
            return;
        }

        BuildSuggestions.Clear();
        var items = await _openDota.GetSuggestedItemsAsync(heroId.Value);
        foreach (var item in items)
        {
            BuildSuggestions.Add(new BuildSuggestionItem { Name = item });
        }
    }

    private void SyncTimers()
    {
        ObjectiveTimers.Clear();
        foreach (var timer in _timers.Timers)
        {
            ObjectiveTimers.Add(timer);
        }
    }

    [RelayCommand]
    private void MarkRoshanDead() => _timers.MarkRoshanDead();

    [RelayCommand]
    private void ClearRoshan() => _timers.ClearRoshan();

    public void RefreshSettings()
    {
        OverlayOpacity = _settings.OverlayOpacity;
        IsInteractive = _settings.OverlayInteractive;
        ShowScoreboardPanel = _settings.ShowScoreboardPanel;
        ShowPlayerPanel = _settings.ShowPlayerPanel;
        ShowItemsPanel = _settings.ShowItemsPanel;
        ShowAbilitiesPanel = _settings.ShowAbilitiesPanel;
        ShowTimersPanel = _settings.ShowTimersPanel;
        ShowDraftPanel = _settings.ShowDraftPanel;
        ShowBuildPanel = _settings.ShowBuildPanel;
    }
}
