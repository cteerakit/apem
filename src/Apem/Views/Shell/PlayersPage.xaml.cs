using Apem.Models;
using Apem.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Apem.Views.Shell;

public sealed partial class PlayersPage : Page
{
    public PlayersPage()
    {
        InitializeComponent();
        ShellContentLayout.Attach(LayoutRoot, ContentColumn);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        AppServices.MatchStore.SnapshotUpdated += OnSnapshotUpdated;
        BindPlayers(AppServices.MatchStore.Snapshot);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        AppServices.MatchStore.SnapshotUpdated -= OnSnapshotUpdated;
        base.OnNavigatedFrom(e);
    }

    private void OnSnapshotUpdated(MatchSnapshot snapshot) => BindPlayers(snapshot);

    private async void OnAddNoteClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: MatchPlayer player })
        {
            return;
        }

        var input = new TextBox
        {
            AcceptsReturn = true,
            Height = 140,
            Text = player.Note,
            TextWrapping = TextWrapping.Wrap,
        };

        var dialog = new ContentDialog
        {
            Title = string.IsNullOrWhiteSpace(player.Name) ? "Player note" : $"Note — {player.Name}",
            Content = input,
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot,
        };
        if (!string.IsNullOrWhiteSpace(player.Note))
        {
            dialog.SecondaryButtonText = "Clear";
        }

        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.None)
        {
            return;
        }

        var note = result == ContentDialogResult.Primary ? input.Text.Trim() : string.Empty;
        SavePlayerNote(player.NoteKey, note);
        BindPlayers(AppServices.MatchStore.Snapshot);
    }

    private void BindPlayers(MatchSnapshot snapshot)
    {
        var notes = AppServices.Settings.PlayerNotes;
        var players = snapshot.Players
            .Select(player =>
            {
                player.Note = notes.TryGetValue(player.NoteKey, out var note) ? note : string.Empty;
                return player;
            })
            .ToList();

        RadiantPlayersList.ItemsSource = players
            .Where(player => player.TeamName.Contains("radiant", StringComparison.OrdinalIgnoreCase))
            .ToList();
        DirePlayersList.ItemsSource = players
            .Where(player => player.TeamName.Contains("dire", StringComparison.OrdinalIgnoreCase))
            .ToList();
        SubtitleText.Text = DescribeRoster(snapshot, players.Count);
    }

    private static void SavePlayerNote(string key, string note)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        var settings = AppServices.Settings;
        if (string.IsNullOrWhiteSpace(note))
        {
            settings.PlayerNotes.Remove(key);
        }
        else
        {
            settings.PlayerNotes[key] = note;
        }

        settings.Save();
    }

    private static string DescribeRoster(MatchSnapshot snapshot, int playerCount)
    {
        if (AppServices.MatchStore.IsDebugPreview)
        {
            return "Debug preview roster.";
        }

        if (!snapshot.IsConnected)
        {
            return "Waiting for Dota 2.";
        }

        if (playerCount == 0)
        {
            return "Connected, but this GSI payload has no player roster yet.";
        }

        if (playerCount == 1)
        {
            return "Live GSI roster. Playing clients usually only receive your own player; spectate for the full scoreboard.";
        }

        return "Live roster from GSI.";
    }
}
