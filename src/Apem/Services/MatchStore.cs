using Apem.Models;
using Apem.Models.Gsi;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Apem.Services;

public sealed partial class MatchStore : ObservableObject
{
    [ObservableProperty]
    private MatchSnapshot _snapshot = new();

    [ObservableProperty]
    private bool _isGsiConnected;

    [ObservableProperty]
    private string _connectionStatus = "Waiting for Dota 2";

    [ObservableProperty]
    private DateTimeOffset? _lastPayloadUtc;

    public event Action<MatchSnapshot>? SnapshotUpdated;

    public void ApplyPayload(GsiPayload payload)
    {
        var snapshot = GsiNormalizer.Normalize(payload);
        Snapshot = snapshot;
        IsGsiConnected = true;
        ConnectionStatus = snapshot.IsInGame || snapshot.IsInDraft
            ? "Live match"
            : "Connected (menu/lobby)";
        LastPayloadUtc = snapshot.ReceivedAtUtc;
        SnapshotUpdated?.Invoke(snapshot);
    }

    public void MarkDisconnected()
    {
        IsGsiConnected = false;
        ConnectionStatus = "Waiting for Dota 2";
        Snapshot = new MatchSnapshot { IsConnected = false };
    }
}
