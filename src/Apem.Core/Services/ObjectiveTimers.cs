using Apem.Models.Gsi;

namespace Apem.Services;

public sealed class TimerEntry
{
    public string Name { get; set; } = string.Empty;
    public string? Label { get; set; }
    public int? SecondsUntil { get; set; }
    public int? WindowEndSeconds { get; set; }
    public int? SpawnAtClock { get; set; }
    public bool IsManual { get; set; }

    public string Display
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(Label))
            {
                return Label;
            }

            if (SecondsUntil is null)
            {
                return "—";
            }

            var main = GsiNormalizer.FormatClock(SecondsUntil.Value);
            if (WindowEndSeconds is int end && end > SecondsUntil)
            {
                return $"{main} – {GsiNormalizer.FormatClock(end)}";
            }

            return main;
        }
    }
}

public static class ObjectiveTimerRules
{
    public const int RoshanMinRespawnSeconds = 8 * 60;
    public const int RoshanMaxRespawnSeconds = 11 * 60;

    public static int NextBountyRune(int clock, bool turbo)
    {
        var interval = turbo ? 180 : 240;
        if (clock < 0)
        {
            return 0;
        }

        return ((clock / interval) + 1) * interval;
    }

    public static int NextPowerRune(int clock, bool turbo)
    {
        if (turbo)
        {
            return NextBountyRune(clock, true);
        }

        if (clock < 120)
        {
            return 120;
        }

        if (clock < 360)
        {
            return 360;
        }

        var sinceSix = clock - 360;
        var interval = 120;
        return 360 + (((sinceSix / interval) + 1) * interval);
    }

    public static int NextWisdomRune(int clock)
    {
        if (clock < 420)
        {
            return 420;
        }

        var since = clock - 420;
        return 420 + (((since / 420) + 1) * 420);
    }

    public static int NextLotus(int clock)
    {
        if (clock < 180)
        {
            return 180;
        }

        var since = clock - 180;
        return 180 + (((since / 180) + 1) * 180);
    }
}
