using System.Text.Json;
using System.Text.Json.Serialization;

namespace Apem.Services;

public sealed class OpenDotaService
{
    private readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("https://api.opendota.com/api/"),
        Timeout = TimeSpan.FromSeconds(20),
    };

    private readonly string _cacheDirectory;
    private List<HeroConstant>? _heroes;

    public OpenDotaService()
    {
        _cacheDirectory = Path.Combine(AppSettings.SettingsDirectory, "cache");
        Directory.CreateDirectory(_cacheDirectory);
    }

    public async Task<IReadOnlyList<HeroConstant>> GetHeroesAsync(CancellationToken cancellationToken = default)
    {
        if (_heroes is not null)
        {
            return _heroes;
        }

        _heroes = await GetCachedAsync<List<HeroConstant>>(
            "heroes.json",
            static () => "/heroes",
            cancellationToken);
        return _heroes;
    }

    public async Task<IReadOnlyList<HeroMatchup>> GetHeroMatchupsAsync(int heroId, CancellationToken cancellationToken = default)
    {
        return await GetCachedAsync<List<HeroMatchup>>(
            $"matchups_{heroId}.json",
            () => $"/heroes/{heroId}/matchups",
            cancellationToken,
            TimeSpan.FromDays(1));
    }

    public async Task<ItemPopularity?> GetItemPopularityAsync(int heroId, CancellationToken cancellationToken = default)
    {
        return await GetCachedAsync<ItemPopularity>(
            $"item_popularity_{heroId}.json",
            () => $"/heroes/{heroId}/itemPopularity",
            cancellationToken,
            TimeSpan.FromDays(1));
    }

    public async Task<IReadOnlyList<CounterSuggestion>> GetCounterSuggestionsAsync(
        IEnumerable<int> enemyHeroIds,
        CancellationToken cancellationToken = default)
    {
        var heroes = await GetHeroesAsync(cancellationToken);
        var enemyIds = enemyHeroIds.Where(static id => id > 0).Distinct().ToList();
        if (enemyIds.Count == 0)
        {
            return Array.Empty<CounterSuggestion>();
        }

        var scores = new Dictionary<int, double>();
        foreach (var enemyId in enemyIds)
        {
            var matchups = await GetHeroMatchupsAsync(enemyId, cancellationToken);
            foreach (var matchup in matchups)
            {
                if (!scores.ContainsKey(matchup.HeroId))
                {
                    scores[matchup.HeroId] = 0;
                }

                scores[matchup.HeroId] += matchup.Wins / Math.Max(matchup.GamesPlayed, 1.0);
            }
        }

        return scores
            .Where(kv => !enemyIds.Contains(kv.Key))
            .OrderByDescending(static kv => kv.Value)
            .Take(5)
            .Select(kv =>
            {
                var hero = heroes.FirstOrDefault(h => h.Id == kv.Key);
                return new CounterSuggestion
                {
                    HeroId = kv.Key,
                    HeroName = hero?.LocalizedName ?? $"Hero {kv.Key}",
                    Score = kv.Value,
                };
            })
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetSuggestedItemsAsync(int heroId, CancellationToken cancellationToken = default)
    {
        var popularity = await GetItemPopularityAsync(heroId, cancellationToken);
        if (popularity is null)
        {
            return Array.Empty<string>();
        }

        return popularity.StartGameItems
            .Concat(popularity.EarlyGameItems)
            .Concat(popularity.MidGameItems)
            .Concat(popularity.LateGameItems)
            .Distinct()
            .Take(8)
            .Select(ResolveItemName)
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .ToList();
    }

    public string? GetHeroIdByInternalName(string internalHeroName, IReadOnlyList<HeroConstant> heroes)
    {
        if (string.IsNullOrWhiteSpace(internalHeroName))
        {
            return null;
        }

        var hero = heroes.FirstOrDefault(h =>
            string.Equals(h.Name, $"npc_dota_hero_{internalHeroName}", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(h.LocalizedName, internalHeroName, StringComparison.OrdinalIgnoreCase));

        return hero?.Id.ToString();
    }

    public int? ResolveHeroId(string internalHeroName, IReadOnlyList<HeroConstant> heroes)
    {
        var idText = GetHeroIdByInternalName(internalHeroName, heroes);
        return int.TryParse(idText, out var id) ? id : null;
    }

    private static string ResolveItemName(int itemId) =>
        itemId switch
        {
            > 0 => $"item_{itemId}",
            _ => string.Empty,
        };

    private async Task<T> GetCachedAsync<T>(
        string cacheFile,
        Func<string> endpointFactory,
        CancellationToken cancellationToken,
        TimeSpan? maxAge = null)
    {
        var cachePath = Path.Combine(_cacheDirectory, cacheFile);
        if (File.Exists(cachePath))
        {
            var age = DateTime.UtcNow - File.GetLastWriteTimeUtc(cachePath);
            if (maxAge is null || age <= maxAge)
            {
                var cached = await File.ReadAllTextAsync(cachePath, cancellationToken);
                var cachedValue = JsonSerializer.Deserialize<T>(cached);
                if (cachedValue is not null)
                {
                    return cachedValue;
                }
            }
        }

        var endpoint = endpointFactory();
        var json = await _httpClient.GetStringAsync(endpoint, cancellationToken);
        await File.WriteAllTextAsync(cachePath, json, cancellationToken);
        return JsonSerializer.Deserialize<T>(json) ?? throw new InvalidOperationException($"Failed to deserialize {endpoint}");
    }
}

public sealed class HeroConstant
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("localized_name")]
    public string LocalizedName { get; set; } = string.Empty;
}

public sealed class HeroMatchup
{
    [JsonPropertyName("hero_id")]
    public int HeroId { get; set; }

    [JsonPropertyName("games_played")]
    public int GamesPlayed { get; set; }

    [JsonPropertyName("wins")]
    public int Wins { get; set; }
}

public sealed class ItemPopularity
{
    [JsonPropertyName("start_game_items")]
    public List<int> StartGameItems { get; set; } = [];

    [JsonPropertyName("early_game_items")]
    public List<int> EarlyGameItems { get; set; } = [];

    [JsonPropertyName("mid_game_items")]
    public List<int> MidGameItems { get; set; } = [];

    [JsonPropertyName("late_game_items")]
    public List<int> LateGameItems { get; set; } = [];
}

public sealed class CounterSuggestion
{
    public int HeroId { get; set; }
    public string HeroName { get; set; } = string.Empty;
    public double Score { get; set; }
}
