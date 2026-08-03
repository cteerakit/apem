using Apem.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Apem.Services;

public sealed partial class TimerService : ObservableObject, IDisposable
{
    private readonly MatchStore _store;
    private readonly AppSettings _settings;
    private readonly TimerEntry[] _all;
    private readonly DispatcherQueueTimer _tickTimer;

    private int _lastClock;
    private DateTimeOffset _lastClockUtc;

    public TimerEntry BountyRune { get; } = new();
    public TimerEntry PowerRune { get; } = new();
    public TimerEntry WisdomRune { get; } = new();
    public TimerEntry LotusPool { get; } = new();

    [ObservableProperty]
    private string? _lastAlert;

    private readonly HashSet<string> _firedAlerts = new(StringComparer.Ordinal);

    public TimerService(MatchStore store, AppSettings settings, DispatcherQueue dispatcherQueue)
    {
        _store = store;
        _settings = settings;
        _all = [BountyRune, PowerRune, WisdomRune, LotusPool];
        _store.SnapshotUpdated += OnSnapshotUpdated;

        _tickTimer = dispatcherQueue.CreateTimer();
        _tickTimer.Interval = TimeSpan.FromSeconds(1);
        _tickTimer.Tick += (_, _) => RefreshTimers(CurrentClock());
        _tickTimer.Start();

        RefreshTimers(_store.Snapshot.ClockTimeSeconds);
    }

    private void OnSnapshotUpdated(MatchSnapshot snapshot)
    {
        _lastClock = snapshot.ClockTimeSeconds;
        _lastClockUtc = DateTimeOffset.UtcNow;
        RefreshTimers(_lastClock);
    }

    private int CurrentClock()
    {
        if (_lastClockUtc == default)
        {
            return _lastClock;
        }

        var elapsed = (int)(DateTimeOffset.UtcNow - _lastClockUtc).TotalSeconds;
        return _lastClock + Math.Max(0, elapsed);
    }

    private void RefreshTimers(int clock)
    {
        var turbo = _settings.IsTurboMode;

        ApplySpawn(BountyRune, "Bounty Rune", ObjectiveTimerRules.NextBountyRune(clock, turbo), clock);
        ApplySpawn(PowerRune, "Power Rune", ObjectiveTimerRules.NextPowerRune(clock, turbo), clock);
        ApplySpawn(WisdomRune, "Wisdom Rune", ObjectiveTimerRules.NextWisdomRune(clock), clock);
        ApplySpawn(LotusPool, "Lotus Pool", ObjectiveTimerRules.NextLotus(clock), clock);

        CheckAlerts();
    }

    private static void ApplySpawn(TimerEntry entry, string name, int spawnAt, int clock) =>
        entry.ApplySpawn(name, spawnAt - clock, spawnAt);

    private void CheckAlerts()
    {
        if (!_settings.TimerSoundsEnabled)
        {
            return;
        }

        foreach (var entry in _all)
        {
            if (entry.SecondsUntil is not int seconds || seconds > 5 || seconds < 0)
            {
                continue;
            }

            var key = $"{entry.Name}:{entry.SpawnAtClock}";
            if (_firedAlerts.Add(key))
            {
                LastAlert = $"{entry.Name} in {seconds}s";
            }
        }
    }

    public void Dispose() => _tickTimer.Stop();
}
