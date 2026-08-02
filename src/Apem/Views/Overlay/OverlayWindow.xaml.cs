using Apem.Interop;
using Apem.Services;
using Apem.ViewModels;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace Apem.Views.Overlay;

public sealed partial class OverlayWindow : Window
{
    private readonly OverlayViewModel _viewModel = new();
    private readonly AppSettings _settings = AppServices.Settings;
    private OverlayPanelHost? _dragTarget;
    private Point _dragStart;
    private double _dragOffsetX;
    private double _dragOffsetY;

    public OverlayWindow()
    {
        InitializeComponent();

        SystemBackdrop = null;
        RootGrid.Background = null;

        var presenter = AppWindow.Presenter as OverlappedPresenter;
        if (presenter is not null)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsAlwaysOnTop = true;
        }

        MarkRoshButton.Click += (_, _) => _viewModel.MarkRoshanDeadCommand.Execute(null);
        ClearRoshButton.Click += (_, _) => _viewModel.ClearRoshanCommand.Execute(null);

        TimersList.ItemsSource = _viewModel.ObjectiveTimers;
        CountersList.ItemsSource = _viewModel.CounterSuggestions;
        BuildList.ItemsSource = _viewModel.BuildSuggestions;

        AppServices.MatchStore.SnapshotUpdated += OnSnapshotUpdated;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(OverlayViewModel.IsInteractive) or null)
            {
                InteractiveBanner.Visibility = _viewModel.IsInteractive ? Visibility.Visible : Visibility.Collapsed;
            }

            if (e.PropertyName?.StartsWith("Show", StringComparison.Ordinal) == true || e.PropertyName is null)
            {
                ApplyPanelVisibility();
            }
        };

        ApplyLayoutPositions();
        ApplyPanelVisibility();
        ApplyOpacity();
        HookPanelDragging();

        Activated += OnOverlayActivated;
    }

    public void ConfigureNativeWindow(bool clickThrough)
    {
        Win32WindowHelper.ConfigureOverlayWindow(this, clickThrough);
    }

    private void OnOverlayActivated(object sender, WindowActivatedEventArgs e)
    {
        ConfigureNativeWindow(clickThrough: !_settings.OverlayInteractive);
    }

    public void SetInteractiveMode(bool interactive)
    {
        _viewModel.IsInteractive = interactive;
        InteractiveBanner.Visibility = interactive ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnSnapshotUpdated(Models.MatchSnapshot snapshot)
    {
        ClockText.Text = snapshot.FormattedClock;
        ScoreText.Text = $"{snapshot.RadiantScore} - {snapshot.DireScore}";
        NetWorthText.Text = $"NW {snapshot.NetWorthLead:+#;-#;0}";
        GameStateText.Text = snapshot.GameState;
        HeroText.Text = $"{snapshot.HeroName} L{snapshot.HeroLevel}";
        KdaText.Text = snapshot.Kda;
        EconomyText.Text = $"GPM {snapshot.Gpm}  XPM {snapshot.Xpm}  Gold {snapshot.Gold}";
        HealthBar.Value = snapshot.HealthPercent;
        ManaBar.Value = snapshot.ManaPercent;
        ItemsList.ItemsSource = snapshot.Items;
        AbilitiesList.ItemsSource = snapshot.Abilities;
    }

    private void ApplyPanelVisibility()
    {
        ScoreboardPanel.Visibility = _settings.ShowScoreboardPanel ? Visibility.Visible : Visibility.Collapsed;
        PlayerPanel.Visibility = _settings.ShowPlayerPanel ? Visibility.Visible : Visibility.Collapsed;
        ItemsPanel.Visibility = _settings.ShowItemsPanel ? Visibility.Visible : Visibility.Collapsed;
        AbilitiesPanel.Visibility = _settings.ShowAbilitiesPanel ? Visibility.Visible : Visibility.Collapsed;
        TimersPanel.Visibility = _settings.ShowTimersPanel ? Visibility.Visible : Visibility.Collapsed;
        DraftPanel.Visibility = _settings.ShowDraftPanel ? Visibility.Visible : Visibility.Collapsed;
        BuildPanel.Visibility = _settings.ShowBuildPanel ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplyLayoutPositions()
    {
        var layout = _settings.PanelLayout;
        Canvas.SetLeft(ScoreboardPanel, layout.ScoreboardX);
        Canvas.SetTop(ScoreboardPanel, layout.ScoreboardY);
        Canvas.SetLeft(PlayerPanel, layout.PlayerX);
        Canvas.SetTop(PlayerPanel, layout.PlayerY);
        Canvas.SetLeft(ItemsPanel, layout.ItemsX);
        Canvas.SetTop(ItemsPanel, layout.ItemsY);
        Canvas.SetLeft(AbilitiesPanel, layout.AbilitiesX);
        Canvas.SetTop(AbilitiesPanel, layout.AbilitiesY);
        Canvas.SetLeft(TimersPanel, layout.TimersX);
        Canvas.SetTop(TimersPanel, layout.TimersY);
        Canvas.SetLeft(DraftPanel, layout.DraftX);
        Canvas.SetTop(DraftPanel, layout.DraftY);
        Canvas.SetLeft(BuildPanel, layout.BuildX);
        Canvas.SetTop(BuildPanel, layout.BuildY);
    }

    public void ApplySettingsFromStore()
    {
        ApplyPanelVisibility();
        ApplyOpacity();
        SetInteractiveMode(_settings.OverlayInteractive);
        ConfigureNativeWindow(clickThrough: !_settings.OverlayInteractive);
    }

    private void ApplyOpacity()
    {
        RootGrid.Opacity = _settings.OverlayOpacity;
    }

    private void HookPanelDragging()
    {
        foreach (var panel in new[] { ScoreboardPanel, PlayerPanel, ItemsPanel, AbilitiesPanel, TimersPanel, DraftPanel, BuildPanel })
        {
            panel.PointerPressed += OnPanelPointerPressed;
            panel.PointerMoved += OnPanelPointerMoved;
            panel.PointerReleased += OnPanelPointerReleased;
            panel.PointerCanceled += OnPanelPointerReleased;
        }
    }

    private void OnPanelPointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (!_settings.OverlayInteractive || sender is not OverlayPanelHost panel)
        {
            return;
        }

        _dragTarget = panel;
        _dragStart = e.GetCurrentPoint(PanelCanvas).Position;
        _dragOffsetX = Canvas.GetLeft(panel);
        _dragOffsetY = Canvas.GetTop(panel);
        panel.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void OnPanelPointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_dragTarget is null || sender is not OverlayPanelHost panel || panel != _dragTarget)
        {
            return;
        }

        var position = e.GetCurrentPoint(PanelCanvas).Position;
        var newX = _dragOffsetX + (position.X - _dragStart.X);
        var newY = _dragOffsetY + (position.Y - _dragStart.Y);
        Canvas.SetLeft(panel, newX);
        Canvas.SetTop(panel, newY);
        e.Handled = true;
    }

    private void OnPanelPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_dragTarget is null)
        {
            return;
        }

        SavePanelPosition(_dragTarget);
        _dragTarget.ReleasePointerCapture(e.Pointer);
        _dragTarget = null;
        e.Handled = true;
    }

    private void SavePanelPosition(OverlayPanelHost panel)
    {
        var layout = _settings.PanelLayout;
        var x = Canvas.GetLeft(panel);
        var y = Canvas.GetTop(panel);

        if (panel == ScoreboardPanel)
        {
            layout.ScoreboardX = x;
            layout.ScoreboardY = y;
        }
        else if (panel == PlayerPanel)
        {
            layout.PlayerX = x;
            layout.PlayerY = y;
        }
        else if (panel == ItemsPanel)
        {
            layout.ItemsX = x;
            layout.ItemsY = y;
        }
        else if (panel == AbilitiesPanel)
        {
            layout.AbilitiesX = x;
            layout.AbilitiesY = y;
        }
        else if (panel == TimersPanel)
        {
            layout.TimersX = x;
            layout.TimersY = y;
        }
        else if (panel == DraftPanel)
        {
            layout.DraftX = x;
            layout.DraftY = y;
        }
        else if (panel == BuildPanel)
        {
            layout.BuildX = x;
            layout.BuildY = y;
        }

        _settings.Save();
    }
}
