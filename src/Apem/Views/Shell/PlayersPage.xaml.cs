using Apem.Services;
using Microsoft.UI.Xaml.Controls;

namespace Apem.Views.Shell;

public sealed partial class PlayersPage : Page
{
    public PlayersPage()
    {
        InitializeComponent();
        ShellContentLayout.Attach(LayoutRoot, ContentColumn);
        LoadPlayers();
    }

    private void LoadPlayers()
    {
        var players = MockMatchData.CreatePlayers();
        RadiantPlayersList.ItemsSource = players
            .Where(player => player.TeamName.Equals("radiant", StringComparison.OrdinalIgnoreCase))
            .ToList();
        DirePlayersList.ItemsSource = players
            .Where(player => player.TeamName.Equals("dire", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
