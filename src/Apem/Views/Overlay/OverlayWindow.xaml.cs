using Apem.Interop;
using Apem.Services;
using Apem.ViewModels;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using WinUIEx;

namespace Apem.Views.Overlay;

public sealed partial class OverlayWindow : Window
{
    private readonly OverlayViewModel _viewModel = new();
    private readonly AppSettings _settings = AppServices.Settings;
    private OverlayPanelHost? _dragTarget;
    private Point _dragStart;
    private double _dragOffsetX;
    private double _dragOffsetY;
    private bool _clickThrough = true;
    private bool _topmost = true;

    public OverlayWindow()
    {
        InitializeComponent();

        this.SetWindowStyle(WindowStyle.Tiled);

        var presenter = AppWindow.Presenter as OverlappedPresenter;
        if (presenter is not null)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsAlwaysOnTop = false;
        }

        BuildList.ItemsSource = _viewModel.BuildSuggestions;
        BindTimerRows();
        RefreshTimerDisplays();

        AppServices.MatchStore.SnapshotUpdated += OnSnapshotUpdated;
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(OverlayViewModel.IsInteractive) or null)
            {
                InteractiveBanner.Visibility = _viewModel.IsInteractive ? Visibility.Visible : Visibility.Collapsed;
                RefreshNativeState();
            }

            if (e.PropertyName?.StartsWith("Show", StringComparison.Ordinal) == true || e.PropertyName is null)
            {
                ApplyPanelVisibility();
                RefreshNativeState();
            }
        };

        ApplyLayoutPositions();
        ApplyPanelVisibility();
        ApplyOpacity();
        HookPanelDragging();
        EnsureTransparentContentBackground();

        RootGrid.Loaded += (_, _) => RefreshNativeState();
        RootGrid.SizeChanged += (_, _) => RefreshNativeState();
        Activated += OnOverlayActivated;
    }

    /// <summary>
    /// Maps each draggable panel to the settings that drive its visibility and
    /// remembered position.
    /// </summary>
    private sealed record PanelBinding(
        OverlayPanelHost Panel,
        Func<bool> IsVisible,
        Func<PanelLayoutSettings, (double? X, double? Y)> GetPosition,
        Action<PanelLayoutSettings, double, double> SetPosition,
        PanelPlacement Default);

    private enum PanelAnchor
    {
        Left,
        Right,
    }

    /// <summary>
    /// Default placement measured from a screen edge rather than as an absolute X,
    /// so the same defaults land correctly on any display width.
    /// </summary>
    private sealed record PanelPlacement(PanelAnchor Anchor, double EdgeMargin, double Top);

    private PanelBinding[]? _panelBindings;

    private PanelBinding[] PanelBindings => _panelBindings ??=
    [
        new(PlayerPanel,
            () => _settings.ShowPlayerPanel,
            static l => (l.PlayerX, l.PlayerY),
            static (l, x, y) => { l.PlayerX = x; l.PlayerY = y; },
            new PanelPlacement(PanelAnchor.Left, 0, 104)),
        new(BountyTimerPanel,
            () => IsRuneTimerVisible(_settings.ShowBountyTimer, AppServices.TimerService.BountyRune),
            static l => (l.BountyX, l.BountyY),
            static (l, x, y) => { l.BountyX = x; l.BountyY = y; },
            new PanelPlacement(PanelAnchor.Right, 0, 64)),
        new(PowerTimerPanel,
            () => IsRuneTimerVisible(_settings.ShowPowerTimer, AppServices.TimerService.PowerRune),
            static l => (l.PowerX, l.PowerY),
            static (l, x, y) => { l.PowerX = x; l.PowerY = y; },
            new PanelPlacement(PanelAnchor.Right, 0, 86)),
        new(WisdomTimerPanel,
            () => IsRuneTimerVisible(_settings.ShowWisdomTimer, AppServices.TimerService.WisdomRune),
            static l => (l.WisdomX, l.WisdomY),
            static (l, x, y) => { l.WisdomX = x; l.WisdomY = y; },
            new PanelPlacement(PanelAnchor.Right, 0, 108)),
        new(LotusTimerPanel,
            () => IsRuneTimerVisible(_settings.ShowLotusTimer, AppServices.TimerService.LotusPool),
            static l => (l.LotusX, l.LotusY),
            static (l, x, y) => { l.LotusX = x; l.LotusY = y; },
            new PanelPlacement(PanelAnchor.Right, 0, 130)),
        new(BuildPanel,
            () => _settings.ShowBuildPanel,
            static l => (l.BuildX, l.BuildY),
            static (l, x, y) => { l.BuildX = x; l.BuildY = y; },
            new PanelPlacement(PanelAnchor.Right, 114, 410)),
    ];

    private bool IsRuneTimerVisible(bool enabled, TimerEntry entry) =>
        enabled && (_settings.OverlayInteractive ||
                    entry.IsWithinLeadWindow(_settings.RuneTimerLeadSeconds));

    private void BindTimerRows()
    {
        var timers = AppServices.TimerService;
        BindRuneCountdownRow(BountyTimerRow, timers.BountyRune);
        BindRuneCountdownRow(PowerTimerRow, timers.PowerRune);
        BindRuneCountdownRow(WisdomTimerRow, timers.WisdomRune);
        BindRuneCountdownRow(LotusTimerRow, timers.LotusPool);

        foreach (var entry in new[] { timers.BountyRune, timers.PowerRune, timers.WisdomRune, timers.LotusPool })
        {
            entry.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is nameof(TimerEntry.SecondsUntil)
                    or nameof(TimerEntry.CountdownDisplay)
                    or null)
                {
                    ApplyPanelVisibility();
                    RefreshNativeState();
                }
            };
        }
    }

    private static void BindRuneCountdownRow(HudStatRow row, TimerEntry entry)
    {
        row.Value = entry.CountdownDisplay;
        entry.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(TimerEntry.CountdownDisplay)
                or nameof(TimerEntry.SecondsUntil)
                or null)
            {
                row.Value = entry.CountdownDisplay;
            }
        };
    }

    public void RefreshTimerDisplays()
    {
        var timers = AppServices.TimerService;
        BountyTimerRow.Value = timers.BountyRune.CountdownDisplay;
        PowerTimerRow.Value = timers.PowerRune.CountdownDisplay;
        WisdomTimerRow.Value = timers.WisdomRune.CountdownDisplay;
        LotusTimerRow.Value = timers.LotusPool.CountdownDisplay;
    }

    private void EnsureTransparentContentBackground()
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            foreach (var grid in FindElements<Grid>("ContentGrid", Content))
            {
                if (VisualTreeHelper.GetParent(grid) is Border)
                {
                    grid.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                }
            }
        });
    }

    private static IEnumerable<T> FindElements<T>(string targetName, object element) where T : FrameworkElement
    {
        if (element is T match &&
            string.Equals(match.Name, targetName, StringComparison.Ordinal))
        {
            yield return match;
        }

        if (element is not DependencyObject dependencyObject)
        {
            yield break;
        }

        var childCount = VisualTreeHelper.GetChildrenCount(dependencyObject);
        for (var i = 0; i < childCount; i++)
        {
            foreach (var child in FindElements<T>(targetName, VisualTreeHelper.GetChild(dependencyObject, i)))
            {
                yield return child;
            }
        }
    }

    public void ConfigureNativeWindow(bool clickThrough, bool topmost)
    {
        _clickThrough = clickThrough;
        _topmost = topmost;

        var interactive = !clickThrough;
        RootGrid.IsHitTestVisible = interactive;
        PanelCanvas.IsHitTestVisible = interactive;
        foreach (var panel in GetPanels())
        {
            panel.IsHitTestVisible = interactive;
        }

        InteractiveBanner.IsHitTestVisible = interactive;

        OverlayClickThroughHelper.Configure(this, clickThrough, topmost, GetHitElements());
    }

    public void RefreshNativeState() => ConfigureNativeWindow(_clickThrough, _topmost);

    private IReadOnlyList<FrameworkElement> GetHitElements()
    {
        var elements = new List<FrameworkElement>();
        elements.AddRange(GetPanels());
        if (InteractiveBanner.Visibility == Visibility.Visible)
        {
            elements.Add(InteractiveBanner);
        }

        return elements;
    }

    private IEnumerable<OverlayPanelHost> GetPanels() => PanelBindings.Select(static b => b.Panel);

    private void OnOverlayActivated(object sender, WindowActivatedEventArgs e)
    {
        if (e.WindowActivationState == WindowActivationState.Deactivated)
        {
            return;
        }

        ConfigureNativeWindow(clickThrough: !_settings.OverlayInteractive, topmost: _settings.OverlayVisible);
    }

    public void SetInteractiveMode(bool interactive)
    {
        _viewModel.IsInteractive = interactive;
        InteractiveBanner.Visibility = interactive ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnSnapshotUpdated(Models.MatchSnapshot snapshot)
    {
        EconomyRow.Value = $"{snapshot.Gpm} / {snapshot.Xpm}";
        RefreshTimerDisplays();
    }

    private void ApplyPanelVisibility()
    {
        foreach (var binding in PanelBindings)
        {
            binding.Panel.Visibility = binding.IsVisible() ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void ApplyLayoutPositions()
    {
        var layout = _settings.PanelLayout;
        foreach (var binding in PanelBindings)
        {
            var (storedX, storedY) = binding.GetPosition(layout);
            var (defaultX, defaultY) = ResolveDefaultPosition(binding);
            SetClampedPanelPosition(binding.Panel, storedX ?? defaultX, storedY ?? defaultY);
        }
    }

    private (double X, double Y) ResolveDefaultPosition(PanelBinding binding)
    {
        var placement = binding.Default;
        if (placement.Anchor == PanelAnchor.Left)
        {
            return (placement.EdgeMargin, placement.Top);
        }

        var panelWidth = binding.Panel.ActualWidth > 0 ? binding.Panel.ActualWidth : binding.Panel.Width;
        if (double.IsNaN(panelWidth) || panelWidth < 0)
        {
            panelWidth = 0;
        }

        var x = PanelCanvas.ActualWidth - panelWidth - placement.EdgeMargin;
        return (Math.Max(0, x), placement.Top);
    }

    /// <summary>Forgets every dragged position so the panels snap back to their default anchors.</summary>
    public void ResetPanelPositions()
    {
        _settings.PanelLayout = new PanelLayoutSettings();
        _settings.Save();
        ApplyLayoutPositions();
        RefreshNativeState();
    }

    public void ApplySettingsFromStore()
    {
        ApplyPanelVisibility();
        ApplyOpacity();
        SetInteractiveMode(_settings.OverlayInteractive);
        ConfigureNativeWindow(clickThrough: !_settings.OverlayInteractive, topmost: _settings.OverlayVisible);
    }

    private void ApplyOpacity()
    {
        RootGrid.Opacity = _settings.OverlayOpacity;
    }

    private void HookPanelDragging()
    {
        foreach (var panel in GetPanels())
        {
            panel.PointerPressed += OnPanelPointerPressed;
            panel.PointerMoved += OnPanelPointerMoved;
            panel.PointerReleased += OnPanelPointerReleased;
            panel.PointerCanceled += OnPanelPointerReleased;
            panel.SizeChanged += (_, _) => ClampPanelInPlace(panel);
        }

        // Re-anchors default-positioned panels to the new edge and re-clamps dragged ones.
        PanelCanvas.SizeChanged += (_, _) => ApplyLayoutPositions();
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
        SetClampedPanelPosition(panel, newX, newY);
        RefreshNativeState();
        e.Handled = true;
    }

    private void OnPanelPointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_dragTarget is null)
        {
            return;
        }

        ClampPanelInPlace(_dragTarget);
        SavePanelPosition(_dragTarget);
        _dragTarget.ReleasePointerCapture(e.Pointer);
        _dragTarget = null;
        RefreshNativeState();
        e.Handled = true;
    }

    private void ClampPanelInPlace(OverlayPanelHost panel) =>
        SetClampedPanelPosition(panel, Canvas.GetLeft(panel), Canvas.GetTop(panel));

    private void SetClampedPanelPosition(OverlayPanelHost panel, double x, double y)
    {
        var (clampedX, clampedY) = ClampToCanvas(panel, x, y);
        Canvas.SetLeft(panel, clampedX);
        Canvas.SetTop(panel, clampedY);
    }

    private (double X, double Y) ClampToCanvas(OverlayPanelHost panel, double x, double y)
    {
        var canvasWidth = PanelCanvas.ActualWidth;
        var canvasHeight = PanelCanvas.ActualHeight;
        if (canvasWidth <= 0 || canvasHeight <= 0)
        {
            return (x, y);
        }

        var panelWidth = panel.ActualWidth > 0 ? panel.ActualWidth : panel.Width;
        var panelHeight = panel.ActualHeight > 0 ? panel.ActualHeight : panel.DesiredSize.Height;
        if (double.IsNaN(panelWidth) || panelWidth <= 0)
        {
            panelWidth = 0;
        }

        if (double.IsNaN(panelHeight) || panelHeight <= 0)
        {
            panelHeight = 0;
        }

        var maxX = Math.Max(0, canvasWidth - panelWidth);
        var maxY = Math.Max(0, canvasHeight - panelHeight);
        return (Math.Clamp(x, 0, maxX), Math.Clamp(y, 0, maxY));
    }

    private void SavePanelPosition(OverlayPanelHost panel)
    {
        var binding = PanelBindings.FirstOrDefault(b => b.Panel == panel);
        if (binding is null)
        {
            return;
        }

        binding.SetPosition(_settings.PanelLayout, Canvas.GetLeft(panel), Canvas.GetTop(panel));
        _settings.Save();
    }
}
