namespace Mango.Models;

public static class SteamIdConverter
{
    private const ulong SteamId64Offset = 76561197960265728UL;

    /// <summary>
    /// Converts a GSI/OpenDota player identifier to SteamID64 for Steam Web API calls.
    /// </summary>
    public static ulong? ToSteamId64(string? playerId)
    {
        var accountId = ToAccountId(playerId);
        return accountId is null ? null : accountId.Value + SteamId64Offset;
    }

    /// <summary>
    /// Converts a GSI/OpenDota player identifier to an OpenDota account ID.
    /// GSI sends the 32-bit account ID; notes and exports may use SteamID64.
    /// </summary>
    public static ulong? ToAccountId(string? playerId)
    {
        if (string.IsNullOrWhiteSpace(playerId) || !ulong.TryParse(playerId.Trim(), out var id))
        {
            return null;
        }

        if (id > SteamId64Offset)
        {
            return id - SteamId64Offset;
        }

        // GSI and OpenDota both use the 32-bit account ID directly.
        if (id > 0 && id <= uint.MaxValue)
        {
            return id;
        }

        return null;
    }
}
