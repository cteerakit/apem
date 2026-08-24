using Apem.Models;
using Apem.Models.Gsi;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Dispatching;

namespace Apem.Services;

public sealed partial class MatchStore : ObservableObject
{
    private static readonly TimeSpan MinPublishInterval = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MinRosterFetchInterval = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan RosterFetchTimeout = TimeSpan.FromSeconds(25);

    private DateTimeOffset _lastPublishUtc = DateTimeOffset.MinValue;
    private DateTimeOffset _lastRosterFetchUtc = DateTimeOffset.MinValue;
    private string _lastRosterFetchKey = string.Empty;
    private CancellationTokenSource? _rosterFetchCts;
    private DispatcherQueue? _dispatcherQueue;
    private Func<string, ulong?, CancellationToken, Task<IReadOnlyList<MatchPlayer>?>>? _rosterResolver;

    [ObservableProperty]
    private MatchSnapshot _snapshot = new();

    [ObservableProperty]
    private bool _isGsiConnected;

    [ObservableProperty]
    private bool _isDebugPreview;

    [ObservableProperty]
    private string _connectionStatus = "Waiting for Dota 2";

    [ObservableProperty]
    private DateTimeOffset? _lastPayloadUtc;

    public event Action<MatchSnapshot>? SnapshotUpdated;

    public void Configure(
        DispatcherQueue dispatcherQueue,
        Func<string, ulong?, CancellationToken, Task<IReadOnlyList<MatchPlayer>?>> rosterResolver)
    {
        _dispatcherQueue = dispatcherQueue;
        _rosterResolver = rosterResolver;
    }

    public void ApplyPayload(GsiPayload payload)
    {
        var snapshot = GsiNormalizer.Normalize(payload);
        IsDebugPreview = false;
        PublishThrottled(snapshot, snapshot.IsInGame || snapshot.IsInDraft ? "Live match" : "Connected (menu/lobby)", connected: true);
        QueueExternalRosterFetch(snapshot);
    }

    public void ApplyDebugPreview()
    {
        CancelRosterFetch();
        IsDebugPreview = true;
        var snapshot = MockMatchData.CreateSnapshot();
        snapshot.MatchId = "debug-" + Guid.NewGuid().ToString("N");
        Publish(snapshot, "Debug preview", connected: true);
    }

    public void ClearDebugPreview()
    {
        if (!IsDebugPreview)
        {
            return;
        }

        IsDebugPreview = false;
        MarkDisconnected();
    }

    public void MarkDisconnected()
    {
        CancelRosterFetch();
        IsGsiConnected = false;
        IsDebugPreview = false;
        ConnectionStatus = "Waiting for Dota 2";
        Snapshot = new MatchSnapshot { IsConnected = false };
        LastPayloadUtc = null;
        _lastPublishUtc = DateTimeOffset.MinValue;
        _lastRosterFetchKey = string.Empty;
        _lastRosterFetchUtc = DateTimeOffset.MinValue;
        SnapshotUpdated?.Invoke(Snapshot);
    }

    private void QueueExternalRosterFetch(MatchSnapshot snapshot)
    {
        if (_rosterResolver is null || _dispatcherQueue is null || IsDebugPreview || !snapshot.IsConnected)
        {
            return;
        }

        if (MatchRosterMerger.CountIdentified(snapshot.Players) >= 10)
        {
            return;
        }

        var localAccountId = SteamIdConverter.ToAccountId(snapshot.SteamId);
        if (!PlayerNotesMatch.IsUsableMatchId(snapshot.MatchId) && localAccountId is null)
        {
            return;
        }

        var fetchKey = $"{snapshot.MatchId}|{localAccountId}|{MatchRosterMerger.CountIdentified(snapshot.Players)}";
        var now = DateTimeOffset.UtcNow;
        if (string.Equals(fetchKey, _lastRosterFetchKey, StringComparison.Ordinal)
            && now - _lastRosterFetchUtc < MinRosterFetchInterval)
        {
            return;
        }

        CancelRosterFetch();
        _rosterFetchCts = new CancellationTokenSource(RosterFetchTimeout);
        var token = _rosterFetchCts.Token;
        var matchId = snapshot.MatchId;
        var gsiPlayers = snapshot.Players;
        _lastRosterFetchKey = fetchKey;
        _lastRosterFetchUtc = DateTimeOffset.UtcNow;

        _ = Task.Run(async () =>
        {
            try
            {
                var external = await _rosterResolver(matchId, localAccountId, token);
                if (external is null || MatchRosterMerger.CountIdentified(external) <= MatchRosterMerger.CountIdentified(gsiPlayers))
                {
                    return;
                }

                var merged = MatchRosterMerger.Merge(gsiPlayers, external);
                token.ThrowIfCancellationRequested();

                _dispatcherQueue.TryEnqueue(() =>
                {
                    if (token.IsCancellationRequested || IsDebugPreview || !Snapshot.IsConnected)
                    {
                        return;
                    }

                    if (PlayerNotesMatch.IsUsableMatchId(matchId)
                        && !string.Equals(Snapshot.MatchId, matchId, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    if (MatchRosterMerger.CountIdentified(merged) <= MatchRosterMerger.CountIdentified(Snapshot.Players))
                    {
                        return;
                    }

                    var updated = CloneWithPlayers(Snapshot, merged);
                    Publish(updated, ConnectionStatus, connected: true);
                });
            }
            catch (OperationCanceledException)
            {
                // Newer fetch or disconnect.
            }
            catch
            {
                // OpenDota unavailable; GSI roster remains.
            }
        }, token);
    }

    private static MatchSnapshot CloneWithPlayers(MatchSnapshot source, IReadOnlyList<MatchPlayer> players) =>
        new()
        {
            ReceivedAtUtc = source.ReceivedAtUtc,
            IsConnected = source.IsConnected,
            GameState = source.GameState,
            ClockTimeSeconds = source.ClockTimeSeconds,
            GameTimeSeconds = source.GameTimeSeconds,
            IsDaytime = source.IsDaytime,
            MatchId = source.MatchId,
            RadiantScore = source.RadiantScore,
            DireScore = source.DireScore,
            NetWorthLead = source.NetWorthLead,
            PlayerName = source.PlayerName,
            SteamId = source.SteamId,
            Kills = source.Kills,
            Deaths = source.Deaths,
            Assists = source.Assists,
            LastHits = source.LastHits,
            Denies = source.Denies,
            Gpm = source.Gpm,
            Xpm = source.Xpm,
            Gold = source.Gold,
            TeamName = source.TeamName,
            HeroName = source.HeroName,
            HeroLevel = source.HeroLevel,
            Health = source.Health,
            MaxHealth = source.MaxHealth,
            Mana = source.Mana,
            MaxMana = source.MaxMana,
            HeroAlive = source.HeroAlive,
            Items = source.Items,
            Abilities = source.Abilities,
            Draft = source.Draft,
            Players = players,
        };

    private void CancelRosterFetch()
    {
        if (_rosterFetchCts is null)
        {
            return;
        }

        _rosterFetchCts.Cancel();
        _rosterFetchCts.Dispose();
        _rosterFetchCts = null;
    }

    private void PublishThrottled(MatchSnapshot snapshot, string connectionStatus, bool connected)
    {
        var now = DateTimeOffset.UtcNow;
        if (now - _lastPublishUtc < MinPublishInterval)
        {
            return;
        }

        Publish(snapshot, connectionStatus, connected);
    }

    private void Publish(MatchSnapshot snapshot, string connectionStatus, bool connected)
    {
        _lastPublishUtc = DateTimeOffset.UtcNow;
        Snapshot = snapshot;
        IsGsiConnected = connected;
        ConnectionStatus = connectionStatus;
        LastPayloadUtc = snapshot.ReceivedAtUtc;
        SnapshotUpdated?.Invoke(snapshot);
    }
}
