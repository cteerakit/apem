using System.Text.Json;
using System.Text.Json.Serialization;
using Apem.Models;

namespace Apem.Services;

public sealed class SteamWebApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(20),
    };

    private readonly Dictionary<string, CachedAvatar> _avatarCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly TimeSpan AvatarCacheDuration = TimeSpan.FromHours(12);

    public SteamWebApiService()
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mango/1.0");
    }

    public async Task<IReadOnlyDictionary<string, SteamPlayerSummary>> GetPlayerSummariesAsync(
        IEnumerable<string> playerIds,
        string apiKey,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, SteamPlayerSummary>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return result;
        }

        var steamId64ByInput = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var pendingSteamIds = new List<string>();

        foreach (var playerId in playerIds
                     .Select(static id => id?.Trim() ?? string.Empty)
                     .Where(static id => !string.IsNullOrWhiteSpace(id))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (_avatarCache.TryGetValue(playerId, out var cached)
                && DateTimeOffset.UtcNow - cached.CachedAtUtc < AvatarCacheDuration)
            {
                result[playerId] = cached.Summary;
                continue;
            }

            var steamId64 = SteamIdConverter.ToSteamId64(playerId);
            if (steamId64 is null)
            {
                continue;
            }

            var steamIdText = steamId64.Value.ToString();
            steamId64ByInput[playerId] = steamIdText;
            if (!pendingSteamIds.Contains(steamIdText, StringComparer.Ordinal))
            {
                pendingSteamIds.Add(steamIdText);
            }
        }

        if (pendingSteamIds.Count == 0)
        {
            return result;
        }

        // Steam allows up to 100 IDs per GetPlayerSummaries call.
        foreach (var batch in pendingSteamIds.Chunk(100))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var url =
                "https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/"
                + $"?key={Uri.EscapeDataString(apiKey.Trim())}"
                + $"&steamids={string.Join(',', batch)}";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<SteamSummariesResponse>(stream, JsonOptions, cancellationToken);
            var players = payload?.Response?.Players ?? [];
            var bySteamId = players
                .Where(static player => !string.IsNullOrWhiteSpace(player.SteamId))
                .ToDictionary(static player => player.SteamId, StringComparer.Ordinal);

            foreach (var (inputId, steamId64) in steamId64ByInput)
            {
                if (!batch.Contains(steamId64, StringComparer.Ordinal)
                    || !bySteamId.TryGetValue(steamId64, out var summaryDto))
                {
                    continue;
                }

                var summary = new SteamPlayerSummary
                {
                    SteamId64 = steamId64,
                    PersonaName = summaryDto.PersonaName ?? string.Empty,
                    AvatarUrl = FirstNonEmpty(summaryDto.AvatarFull, summaryDto.AvatarMedium, summaryDto.Avatar),
                    IsPrivate = summaryDto.CommunityVisibilityState is > 0 and < 3,
                };

                _avatarCache[inputId] = new CachedAvatar(summary, DateTimeOffset.UtcNow);
                result[inputId] = summary;
            }
        }

        return result;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private sealed record CachedAvatar(SteamPlayerSummary Summary, DateTimeOffset CachedAtUtc);

    private sealed class SteamSummariesResponse
    {
        [JsonPropertyName("response")]
        public SteamSummariesBody? Response { get; set; }
    }

    private sealed class SteamSummariesBody
    {
        [JsonPropertyName("players")]
        public List<SteamSummaryDto> Players { get; set; } = [];
    }

    private sealed class SteamSummaryDto
    {
        [JsonPropertyName("steamid")]
        public string SteamId { get; set; } = string.Empty;

        [JsonPropertyName("personaname")]
        public string? PersonaName { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("avatarmedium")]
        public string? AvatarMedium { get; set; }

        [JsonPropertyName("avatarfull")]
        public string? AvatarFull { get; set; }

        /// <summary>1 = private, 2 = friends only, 3 = public.</summary>
        [JsonPropertyName("communityvisibilitystate")]
        public int CommunityVisibilityState { get; set; }
    }
}

public sealed class SteamPlayerSummary
{
    public string SteamId64 { get; init; } = string.Empty;
    public string PersonaName { get; init; } = string.Empty;
    public string AvatarUrl { get; init; } = string.Empty;
    public bool IsPrivate { get; init; }
}
