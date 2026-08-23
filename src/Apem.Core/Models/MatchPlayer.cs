namespace Apem.Models;

public sealed class MatchPlayer
{
    public string SteamId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int TeamSlot { get; set; }
    public string HeroName { get; set; } = string.Empty;
    public int HeroLevel { get; set; }
    public int Kills { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int LastHits { get; set; }
    public int Denies { get; set; }
    public int Gpm { get; set; }
    public int Xpm { get; set; }
    public int Gold { get; set; }

    public string Note { get; set; } = string.Empty;

    public string Kda => $"{Kills}/{Deaths}/{Assists}";
    public string LastHitsDenies => $"{LastHits}/{Denies}";
    public string DisplayHeroName => FormatHeroName(HeroName);
    public string NoteKey => string.IsNullOrWhiteSpace(SteamId) ? Name : SteamId;
    public string NoteButtonLabel => string.IsNullOrWhiteSpace(Note) ? "Add note" : "Edit note";

    private static string FormatHeroName(string heroName)
    {
        const string prefix = "npc_dota_hero_";
        if (heroName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return heroName[prefix.Length..]
            .Replace('_', ' ');
        }

        return heroName;
    }
}
