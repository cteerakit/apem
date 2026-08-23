using Apem.Models.Gsi;
using Apem.Services;
using Xunit;

namespace Apem.Tests;

public class GsiNormalizerTests
{
    [Fact]
    public void Normalize_FormatsClockAndKda()
    {
        var payload = new GsiPayload
        {
            Map = new GsiMap { ClockTime = 754, GameState = "DOTA_GAMERULES_STATE_GAME_IN_PROGRESS" },
            Player = new GsiPlayer { Kills = 3, Deaths = 1, Assists = 5, Gpm = 420, Xpm = 510, Gold = 1250 },
            Hero = new GsiHero { Name = "npc_dota_hero_invoker", Level = 12, Health = 800, MaxHealth = 1000, Mana = 300, MaxMana = 400 },
        };

        var snapshot = GsiNormalizer.Normalize(payload);

        Assert.Equal("12:34", snapshot.FormattedClock);
        Assert.Equal("3/1/5", snapshot.Kda);
        Assert.Equal("invoker", snapshot.HeroName);
        Assert.Equal(0.8, snapshot.HealthPercent, 3);
        Assert.Single(snapshot.Players);
        Assert.Equal("invoker", snapshot.Players[0].DisplayHeroName);
    }

    [Fact]
    public void Normalize_MapsAllPlayersRoster()
    {
        var payload = new GsiPayload
        {
            AllPlayers = new Dictionary<string, GsiPlayer>
            {
                ["player1"] = new GsiPlayer
                {
                    Name = "Dire Mid",
                    Team = 3,
                    TeamSlot = 1,
                    Kills = 5,
                    Deaths = 2,
                    Assists = 4,
                    LastHits = 120,
                    Denies = 8,
                    Gpm = 500,
                    Xpm = 600,
                    Gold = 1800,
                    Hero = new GsiHero { Name = "npc_dota_hero_invoker", Level = 16 },
                },
                ["player0"] = new GsiPlayer
                {
                    Name = "Radiant Carry",
                    TeamName = "radiant",
                    TeamSlot = 0,
                    Kills = 8,
                    Deaths = 1,
                    Assists = 3,
                    LastHits = 200,
                    Denies = 12,
                    Gpm = 620,
                    Xpm = 710,
                    Gold = 2400,
                    Hero = new GsiHero { Name = "npc_dota_hero_juggernaut", Level = 18 },
                },
            },
        };

        var snapshot = GsiNormalizer.Normalize(payload);

        Assert.Equal(2, snapshot.Players.Count);
        Assert.Equal("Radiant Carry", snapshot.Players[0].Name);
        Assert.Equal("radiant", snapshot.Players[0].TeamName);
        Assert.Equal("juggernaut", snapshot.Players[0].DisplayHeroName);
        Assert.Equal("Dire Mid", snapshot.Players[1].Name);
        Assert.Equal("dire", snapshot.Players[1].TeamName);
        Assert.Equal("5/2/4", snapshot.Players[1].Kda);
    }

    [Fact]
    public void ObjectiveTimerRules_BountyRuneSpawnsEveryFourMinutes()
    {
        Assert.Equal(0, ObjectiveTimerRules.NextBountyRune(-30));
        Assert.Equal(240, ObjectiveTimerRules.NextBountyRune(100));
        Assert.Equal(480, ObjectiveTimerRules.NextBountyRune(240));
    }

    [Fact]
    public void ObjectiveTimerRules_PowerRuneSpawnsEveryTwoMinutes()
    {
        Assert.Equal(120, ObjectiveTimerRules.NextPowerRune(0));
        Assert.Equal(240, ObjectiveTimerRules.NextPowerRune(120));
        Assert.Equal(360, ObjectiveTimerRules.NextPowerRune(300));
    }

    [Fact]
    public void ObjectiveTimerRules_LotusHalvesItsIntervalInTurbo()
    {
        Assert.Equal(180, ObjectiveTimerRules.NextLotus(100, turbo: false));
        Assert.Equal(360, ObjectiveTimerRules.NextLotus(180, turbo: false));
        Assert.Equal(90, ObjectiveTimerRules.NextLotus(0, turbo: true));
        Assert.Equal(180, ObjectiveTimerRules.NextLotus(90, turbo: true));
    }
}
