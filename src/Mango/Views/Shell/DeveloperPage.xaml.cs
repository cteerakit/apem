using Mango.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Mango.Views.Shell;

public sealed partial class DeveloperPage : Page
{
    private ShellViewModel? _viewModel;
    private bool _suppressEvents;

    public DeveloperPage()
    {
        InitializeComponent();
        ShellContentLayout.Attach(LayoutRoot, ContentColumn);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not ShellViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
        _suppressEvents = true;
        BuildToggle.IsOn = viewModel.ShowBuildPanel;
        TimerAlertsToggle.IsOn = viewModel.TimerSoundsEnabled;
        DebugPreviewToggle.IsOn = viewModel.DebugOverlayPreview;
        _suppressEvents = false;

        BuildToggle.Toggled += OnSettingChanged;
        TimerAlertsToggle.Toggled += OnSettingChanged;
        DebugPreviewToggle.Toggled += OnSettingChanged;
    }

    private void OnSettingChanged(object sender, RoutedEventArgs e)
    {
        if (_suppressEvents || _viewModel is null)
        {
            return;
        }

        _viewModel.ShowBuildPanel = BuildToggle.IsOn;
        _viewModel.TimerSoundsEnabled = TimerAlertsToggle.IsOn;
        _viewModel.DebugOverlayPreview = DebugPreviewToggle.IsOn;
        _viewModel.SaveSettingsCommand.Execute(null);
    }
}
