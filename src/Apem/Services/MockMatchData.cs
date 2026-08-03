using Apem.Models;

namespace Apem.Services;

internal static class MockMatchData
{
    public static MatchSnapshot CreateSnapshot() => new()
    {
        ReceivedAtUtc = DateTimeOffset.UtcNow,
        IsConnected = true,
        GameState = "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS",
        ClockTimeSeconds = 228,
        GameTimeSeconds = (32 * 60) + 48,
        IsDaytime = true,
        RadiantScore = 24,
        DireScore = 19,
        NetWorthLead = 1840,
        PlayerName = "APEM Tester",
        Gpm = 612,
        Xpm = 704,
        TeamName = "radiant",
        HeroName = "npc_dota_hero_juggernaut",
        HeroLevel = 18,
        HeroAlive = true,
    };

    public static IReadOnlyList<BuildSuggestionItem> CreateBuildSuggestions() =>
    [
        new BuildSuggestionItem { Name = "quelling_blade" },
        new BuildSuggestionItem { Name = "power_treads" },
        new BuildSuggestionItem { Name = "bfury" },
        new BuildSuggestionItem { Name = "manta" },
        new BuildSuggestionItem { Name = "butterfly" },
        new BuildSuggestionItem { Name = "abyssal_blade" },
        new BuildSuggestionItem { Name = "satanic" },
    ];
}
