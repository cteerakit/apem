using Apem.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Apem.Views.Shell;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; } = new();

    public ShellPage()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (NavView.SelectedItem is null && NavView.MenuItems.Count > 0)
        {
            NavView.SelectedItem = NavView.MenuItems[0];
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is not NavigationViewItem item || item.Tag is not string tag)
        {
            return;
        }

        var pageType = tag switch
        {
            "status" => typeof(StatusPage),
            "settings" => typeof(SettingsPage),
            "shortcuts" => typeof(ShortcutsPage),
            "developer" => typeof(DeveloperPage),
            _ => typeof(SetupPage),
        };

        ContentFrame.Navigate(pageType, ViewModel);
    }
}
