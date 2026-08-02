using System.Text.Json.Serialization;

namespace Apem.Models.Gsi;

public sealed class GsiPayload
{
    [JsonPropertyName("provider")]
    public GsiProvider? Provider { get; set; }

    [JsonPropertyName("map")]
    public GsiMap? Map { get; set; }

    [JsonPropertyName("player")]
    public GsiPlayer? Player { get; set; }

    [JsonPropertyName("hero")]
    public GsiHero? Hero { get; set; }

    [JsonPropertyName("items")]
    public Dictionary<string, GsiItemSlot>? Items { get; set; }

    [JsonPropertyName("abilities")]
    public Dictionary<string, GsiAbilitySlot>? Abilities { get; set; }

    [JsonPropertyName("draft")]
    public GsiDraft? Draft { get; set; }

    [JsonPropertyName("auth")]
    public GsiAuth? Auth { get; set; }
}

public sealed class GsiProvider
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("appid")]
    public int AppId { get; set; }

    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

public sealed class GsiMap
{
    [JsonPropertyName("game_state")]
    public string? GameState { get; set; }

    [JsonPropertyName("clock_time")]
    public int ClockTime { get; set; }

    [JsonPropertyName("game_time")]
    public int GameTime { get; set; }

    [JsonPropertyName("daytime")]
    public bool Daytime { get; set; }

    [JsonPropertyName("radiant_score")]
    public int RadiantScore { get; set; }

    [JsonPropertyName("dire_score")]
    public int DireScore { get; set; }

    [JsonPropertyName("radiant_gold_adv")]
    public int[]? RadiantGoldAdv { get; set; }

    [JsonPropertyName("win_team")]
    public string? WinTeam { get; set; }
}

public sealed class GsiPlayer
{
    [JsonPropertyName("steamid")]
    public string? SteamId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("kills")]
    public int Kills { get; set; }

    [JsonPropertyName("deaths")]
    public int Deaths { get; set; }

    [JsonPropertyName("assists")]
    public int Assists { get; set; }

    [JsonPropertyName("last_hits")]
    public int LastHits { get; set; }

    [JsonPropertyName("denies")]
    public int Denies { get; set; }

    [JsonPropertyName("gpm")]
    public int Gpm { get; set; }

    [JsonPropertyName("xpm")]
    public int Xpm { get; set; }

    [JsonPropertyName("gold")]
    public int Gold { get; set; }

    [JsonPropertyName("gold_reliable")]
    public int GoldReliable { get; set; }

    [JsonPropertyName("gold_unreliable")]
    public int GoldUnreliable { get; set; }

    [JsonPropertyName("team_name")]
    public string? TeamName { get; set; }
}

public sealed class GsiHero
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("health")]
    public int Health { get; set; }

    [JsonPropertyName("max_health")]
    public int MaxHealth { get; set; }

    [JsonPropertyName("mana")]
    public int Mana { get; set; }

    [JsonPropertyName("max_mana")]
    public int MaxMana { get; set; }

    [JsonPropertyName("alive")]
    public bool Alive { get; set; }
}

public sealed class GsiItemSlot
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("cooldown")]
    public double Cooldown { get; set; }

    [JsonPropertyName("can_cast")]
    public bool CanCast { get; set; }
}

public sealed class GsiAbilitySlot
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }

    [JsonPropertyName("cooldown")]
    public double Cooldown { get; set; }

    [JsonPropertyName("can_cast")]
    public bool CanCast { get; set; }
}

public sealed class GsiDraft
{
    [JsonPropertyName("activeteam")]
    public int ActiveTeam { get; set; }

    [JsonPropertyName("pick")]
    public bool Pick { get; set; }

    [JsonPropertyName("activeteam_time_remaining")]
    public int ActiveTeamTimeRemaining { get; set; }

    [JsonPropertyName("radiant_bonus_time")]
    public int RadiantBonusTime { get; set; }

    [JsonPropertyName("dire_bonus_time")]
    public int DireBonusTime { get; set; }

    [JsonPropertyName("team2")]
    public GsiDraftTeam? Team2 { get; set; }

    [JsonPropertyName("team3")]
    public GsiDraftTeam? Team3 { get; set; }
}

public sealed class GsiDraftTeam
{
    [JsonPropertyName("home_team")]
    public bool HomeTeam { get; set; }

    [JsonPropertyName("pick0_id")]
    public int Pick0Id { get; set; }

    [JsonPropertyName("pick1_id")]
    public int Pick1Id { get; set; }

    [JsonPropertyName("pick2_id")]
    public int Pick2Id { get; set; }

    [JsonPropertyName("pick3_id")]
    public int Pick3Id { get; set; }

    [JsonPropertyName("pick4_id")]
    public int Pick4Id { get; set; }

    [JsonPropertyName("ban0_id")]
    public int Ban0Id { get; set; }

    [JsonPropertyName("ban1_id")]
    public int Ban1Id { get; set; }

    [JsonPropertyName("ban2_id")]
    public int Ban2Id { get; set; }

    [JsonPropertyName("ban3_id")]
    public int Ban3Id { get; set; }

    [JsonPropertyName("ban4_id")]
    public int Ban4Id { get; set; }

    [JsonPropertyName("ban5_id")]
    public int Ban5Id { get; set; }

    [JsonPropertyName("ban6_id")]
    public int Ban6Id { get; set; }
}

public sealed class GsiAuth
{
    [JsonPropertyName("token")]
    public string? Token { get; set; }
}
