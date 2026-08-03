using System.ComponentModel;
using System.Runtime.CompilerServices;
using Apem.Models.Gsi;

namespace Apem.Services;

public sealed class TimerEntry : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private int? _secondsUntil;
    private int? _spawnAtClock;

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public int? SecondsUntil
    {
        get => _secondsUntil;
        set
        {
            if (SetField(ref _secondsUntil, value))
            {
                OnPropertyChanged(nameof(CountdownDisplay));
            }
        }
    }

    public int? SpawnAtClock
    {
        get => _spawnAtClock;
        set => SetField(ref _spawnAtClock, value);
    }

    /// <summary>MM:SS countdown until the next spawn, or empty when not applicable.</summary>
    public string CountdownDisplay
    {
        get
        {
            if (SecondsUntil is not int seconds || seconds < 0)
            {
                return string.Empty;
            }

            return GsiNormalizer.FormatClock(seconds);
        }
    }

    /// <summary>True when a spawn countdown should be shown on the overlay.</summary>
    public bool IsWithinLeadWindow(int leadSeconds) =>
        SecondsUntil is int seconds && seconds >= 0 && seconds <= leadSeconds;

    public void ApplySpawn(string name, int secondsUntil, int spawnAtClock)
    {
        Name = name;
        SpawnAtClock = spawnAtClock;
        SecondsUntil = secondsUntil;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public static class ObjectiveTimerRules
{
    private const int BountyIntervalSeconds = 240;
    private const int PowerIntervalSeconds = 120;
    private const int WisdomIntervalSeconds = 420;
    private const int LotusIntervalSeconds = 180;
    private const int TurboLotusIntervalSeconds = 90;

    public static int NextBountyRune(int clock)
    {
        // Bounty runes are already on the map when the horn sounds.
        if (clock < 0)
        {
            return 0;
        }

        return NextInterval(clock, BountyIntervalSeconds);
    }

    public static int NextPowerRune(int clock) => NextInterval(clock, PowerIntervalSeconds);

    public static int NextWisdomRune(int clock) => NextInterval(clock, WisdomIntervalSeconds);

    public static int NextLotus(int clock, bool turbo) =>
        NextInterval(clock, turbo ? TurboLotusIntervalSeconds : LotusIntervalSeconds);

    /// <summary>Clock of the next multiple of <paramref name="interval"/>, treating pre-horn time as 0:00.</summary>
    private static int NextInterval(int clock, int interval)
    {
        var elapsed = Math.Max(0, clock);
        return ((elapsed / interval) + 1) * interval;
    }
}
