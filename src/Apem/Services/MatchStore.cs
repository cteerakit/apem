using Apem.Models;
using Apem.Models.Gsi;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Apem.Services;

public sealed partial class MatchStore : ObservableObject
{
    private static readonly TimeSpan MinPublishInterval = TimeSpan.FromSeconds(1);
    private DateTimeOffset _lastPublishUtc = DateTimeOffset.MinValue;

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

    public void ApplyPayload(GsiPayload payload)
    {
        var snapshot = GsiNormalizer.Normalize(payload);
        IsDebugPreview = false;
        PublishThrottled(snapshot, snapshot.IsInGame || snapshot.IsInDraft ? "Live match" : "Connected (menu/lobby)", connected: true);
    }

    public void ApplyDebugPreview()
    {
        IsDebugPreview = true;
        Publish(MockMatchData.CreateSnapshot(), "Debug preview", connected: true);
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
        IsGsiConnected = false;
        IsDebugPreview = false;
        ConnectionStatus = "Waiting for Dota 2";
        Snapshot = new MatchSnapshot { IsConnected = false };
        LastPayloadUtc = null;
        _lastPublishUtc = DateTimeOffset.MinValue;
        SnapshotUpdated?.Invoke(Snapshot);
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
