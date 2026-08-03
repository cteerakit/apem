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
