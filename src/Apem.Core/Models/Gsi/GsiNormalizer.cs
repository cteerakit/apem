namespace Apem.Models.Gsi;

public static class GsiNormalizer
{
    public static MatchSnapshot Normalize(GsiPayload payload)
    {
        var snapshot = new MatchSnapshot
        {
            ReceivedAtUtc = DateTimeOffset.UtcNow,
            IsConnected = true,
            GameState = payload.Map?.GameState ?? string.Empty,
            ClockTimeSeconds = payload.Map?.ClockTime ?? 0,
            GameTimeSeconds = payload.Map?.GameTime ?? 0,
            IsDaytime = payload.Map?.Daytime ?? true,
            RadiantScore = payload.Map?.RadiantScore ?? 0,
            DireScore = payload.Map?.DireScore ?? 0,
            NetWorthLead = payload.Map?.RadiantGoldAdv is { Length: > 0 } adv ? adv[^1] : 0,
            PlayerName = payload.Player?.Name ?? string.Empty,
            Kills = payload.Player?.Kills ?? 0,
            Deaths = payload.Player?.Deaths ?? 0,
            Assists = payload.Player?.Assists ?? 0,
            LastHits = payload.Player?.LastHits ?? 0,
            Denies = payload.Player?.Denies ?? 0,
            Gpm = payload.Player?.Gpm ?? 0,
            Xpm = payload.Player?.Xpm ?? 0,
            Gold = payload.Player?.Gold ?? 0,
            TeamName = payload.Player?.TeamName ?? string.Empty,
            HeroName = FormatInternalName(payload.Hero?.Name),
            HeroLevel = payload.Hero?.Level ?? 0,
            Health = payload.Hero?.Health ?? 0,
            MaxHealth = payload.Hero?.MaxHealth ?? 0,
            Mana = payload.Hero?.Mana ?? 0,
            MaxMana = payload.Hero?.MaxMana ?? 0,
            HeroAlive = payload.Hero?.Alive ?? true,
        };

        snapshot.Items = NormalizeItems(payload.Items);
        snapshot.Abilities = NormalizeAbilities(payload.Abilities);
        snapshot.Draft = NormalizeDraft(payload.Draft, snapshot.TeamName);

        return snapshot;
    }

    private static IReadOnlyList<SlotSnapshot> NormalizeItems(Dictionary<string, GsiItemSlot>? items)
    {
        if (items is null)
        {
            return Array.Empty<SlotSnapshot>();
        }

        return items
            .OrderBy(static kv => kv.Key, StringComparer.Ordinal)
            .Select(static kv => new SlotSnapshot
            {
                Slot = kv.Key,
                Name = FormatInternalName(kv.Value.Name),
                Cooldown = kv.Value.Cooldown,
                CanCast = kv.Value.CanCast,
            })
            .Where(static s => !string.IsNullOrWhiteSpace(s.Name) && s.Name != "empty")
            .ToList();
    }

    private static IReadOnlyList<SlotSnapshot> NormalizeAbilities(Dictionary<string, GsiAbilitySlot>? abilities)
    {
        if (abilities is null)
        {
            return Array.Empty<SlotSnapshot>();
        }

        return abilities
            .OrderBy(static kv => kv.Key, StringComparer.Ordinal)
            .Select(static kv => new SlotSnapshot
            {
                Slot = kv.Key,
                Name = FormatInternalName(kv.Value.Name),
                Level = kv.Value.Level,
                Cooldown = kv.Value.Cooldown,
                CanCast = kv.Value.CanCast,
            })
            .Where(static s => !string.IsNullOrWhiteSpace(s.Name) && s.Name != "empty")
            .ToList();
    }

    private static DraftSnapshot NormalizeDraft(GsiDraft? draft, string teamName)
    {
        if (draft is null)
        {
            return new DraftSnapshot();
        }

        var radiant = draft.Team2;
        var dire = draft.Team3;
        var isRadiant = teamName.Contains("radiant", StringComparison.OrdinalIgnoreCase);

        return new DraftSnapshot
        {
            IsActive = true,
            ActiveTeam = draft.ActiveTeam,
            IsPickPhase = draft.Pick,
            TimeRemaining = draft.ActiveTeamTimeRemaining,
            RadiantHeroIds = ExtractHeroIds(radiant),
            DireHeroIds = ExtractHeroIds(dire),
            PlayerIsRadiant = isRadiant,
        };
    }

    private static IReadOnlyList<int> ExtractHeroIds(GsiDraftTeam? team)
    {
        if (team is null)
        {
            return Array.Empty<int>();
        }

        return new[]
        {
            team.Pick0Id, team.Pick1Id, team.Pick2Id, team.Pick3Id, team.Pick4Id,
        }.Where(static id => id > 0).ToList();
    }

    public static string FormatInternalName(string? internalName)
    {
        if (string.IsNullOrWhiteSpace(internalName))
        {
            return string.Empty;
        }

        return internalName
            .Replace("npc_dota_hero_", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("item_", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("ability_", string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    public static string FormatClock(int clockSeconds)
    {
        var abs = Math.Abs(clockSeconds);
        var minutes = abs / 60;
        var seconds = abs % 60;
        var sign = clockSeconds < 0 ? "-" : string.Empty;
        return $"{sign}{minutes:00}:{seconds:00}";
    }
}
