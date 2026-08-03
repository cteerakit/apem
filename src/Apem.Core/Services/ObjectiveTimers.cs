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
