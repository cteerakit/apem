using Apem.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Apem.Services;

public sealed partial class TimerService : ObservableObject
{
    private readonly MatchStore _store;
    private readonly AppSettings _settings;

    [ObservableProperty]
    private IReadOnlyList<TimerEntry> _timers = Array.Empty<TimerEntry>();

    [ObservableProperty]
    private string? _lastAlert;

    private int? _roshanDeathClock;
    private readonly HashSet<string> _firedAlerts = new(StringComparer.Ordinal);

    public TimerService(MatchStore store, AppSettings settings)
    {
        _store = store;
        _settings = settings;
        _store.SnapshotUpdated += OnSnapshotUpdated;
    }

    public void MarkRoshanDead()
    {
        _roshanDeathClock = _store.Snapshot.ClockTimeSeconds;
        RefreshTimers();
    }

    public void ClearRoshan()
    {
        _roshanDeathClock = null;
        RefreshTimers();
    }

    private void OnSnapshotUpdated(MatchSnapshot snapshot)
    {
        RefreshTimers(snapshot);
    }

    private void RefreshTimers() => RefreshTimers(_store.Snapshot);

    private void RefreshTimers(MatchSnapshot snapshot)
    {
        var clock = snapshot.ClockTimeSeconds;
        var turbo = _settings.IsTurboMode;
        var entries = new List<TimerEntry>
        {
            BuildSpawnTimer("Bounty Rune", ObjectiveTimerRules.NextBountyRune(clock, turbo), clock),
            BuildSpawnTimer("Power Rune", ObjectiveTimerRules.NextPowerRune(clock, turbo), clock),
            BuildSpawnTimer("Wisdom Rune", ObjectiveTimerRules.NextWisdomRune(clock), clock),
            BuildSpawnTimer("Lotus Pool", ObjectiveTimerRules.NextLotus(clock), clock),
        };

        if (_roshanDeathClock is int deathClock)
        {
            var minRespawn = deathClock + ObjectiveTimerRules.RoshanMinRespawnSeconds;
            var maxRespawn = deathClock + ObjectiveTimerRules.RoshanMaxRespawnSeconds;
            entries.Add(new TimerEntry
            {
                Name = "Roshan",
                SecondsUntil = minRespawn - clock,
                WindowEndSeconds = maxRespawn - clock,
                IsManual = true,
            });
        }
        else
        {
            entries.Add(new TimerEntry
            {
                Name = "Roshan",
                Label = "Press Mark when Rosh dies",
                IsManual = true,
            });
        }

        Timers = entries;
        CheckAlerts(entries);
    }

    private static TimerEntry BuildSpawnTimer(string name, int spawnAt, int clock) =>
        new()
        {
            Name = name,
            SecondsUntil = spawnAt - clock,
            SpawnAtClock = spawnAt,
        };

    private void CheckAlerts(IReadOnlyList<TimerEntry> entries)
    {
        if (!_settings.TimerSoundsEnabled)
        {
            return;
        }

        foreach (var entry in entries)
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
}
