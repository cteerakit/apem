using Apem.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Apem.Views.Shell;

public sealed partial class SetupPage : Page
{
    private ShellViewModel? _viewModel;

    public SetupPage()
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
        InstallMessageText.Text = viewModel.InstallMessage;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;

        ReinstallButton.Click += (_, _) => viewModel.ReinstallGsiConfigCommand.Execute(null);
        ShowOverlayButton.Click += (_, _) => viewModel.ShowOverlayCommand.Execute(null);
        HideOverlayButton.Click += (_, _) => viewModel.HideOverlayCommand.Execute(null);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        base.OnNavigatedFrom(e);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (_viewModel is null)
        {
            return;
        }

        if (e.PropertyName is nameof(ShellViewModel.InstallMessage) or null)
        {
            InstallMessageText.Text = _viewModel.InstallMessage;
        }
    }
}
