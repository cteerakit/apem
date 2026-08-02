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
    public void ObjectiveTimerRules_ReturnsNextBountyRune()
    {
        Assert.Equal(240, ObjectiveTimerRules.NextBountyRune(100, turbo: false));
        Assert.Equal(180, ObjectiveTimerRules.NextBountyRune(100, turbo: true));
    }
}
