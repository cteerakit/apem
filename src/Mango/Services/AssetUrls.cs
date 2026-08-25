namespace Mango.Services;

public static class AssetUrls
{
  private const string HeroCdn = "https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes";
  private const string ItemCdn = "https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/items";
  private const string RankIconCdn = "https://www.opendota.com/assets/images/dota2/rank_icons";

  public static string HeroIcon(string heroInternalName) =>
      string.IsNullOrWhiteSpace(heroInternalName)
          ? string.Empty
          : $"{HeroCdn}/{heroInternalName}.png";

  public static string ItemIcon(string itemInternalName) =>
      string.IsNullOrWhiteSpace(itemInternalName)
          ? string.Empty
          : $"{ItemCdn}/{itemInternalName}.png";

  /// <summary>OpenDota medal icon for a <c>rank_tier</c> (e.g. 55 = Legend 5).</summary>
  public static string RankIcon(int? rankTier)
  {
      var medal = rankTier is null or <= 0
          ? 0
          : Math.Clamp(rankTier.Value / 10, 0, 8);
      return $"{RankIconCdn}/rank_icon_{medal}.png";
  }

  /// <summary>Star overlay for Herald–Divine; empty for unranked/Immortal.</summary>
  public static string RankStar(int? rankTier)
  {
      if (rankTier is null or <= 0)
      {
          return string.Empty;
      }

      var medal = rankTier.Value / 10;
      var stars = Math.Clamp(rankTier.Value % 10, 0, 5);
      if (medal is < 1 or > 7 || stars < 1)
      {
          return string.Empty;
      }

      return $"{RankIconCdn}/rank_star_{stars}.png";
  }
}
