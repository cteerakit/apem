using System.Reflection;
using Apem.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Windows.ApplicationModel;

namespace Apem.Views.Shell;

public sealed partial class AboutPage : Page
{
    private ShellViewModel? _viewModel;

    public AboutPage()
    {
        InitializeComponent();
        ShellContentLayout.Attach(LayoutRoot, ContentColumn);
        VersionText.Text = ReadAppVersion();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is ShellViewModel viewModel)
        {
            _viewModel = viewModel;
        }
    }

    private void OnDeveloperClick(object sender, RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(DeveloperPage), _viewModel);
    }

    private static string ReadAppVersion()
    {
        try
        {
            var version = Package.Current.Id.Version;
            return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        catch
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            return assemblyVersion?.ToString() ?? "1.0.0.0";
        }
    }
}
