namespace Apem.Services;

public static class AssetUrls
{
  private const string HeroCdn = "https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/heroes";
  private const string ItemCdn = "https://cdn.cloudflare.steamstatic.com/apps/dota2/images/dota_react/items";

  public static string HeroIcon(string heroInternalName) =>
      string.IsNullOrWhiteSpace(heroInternalName)
          ? string.Empty
          : $"{HeroCdn}/{heroInternalName}.png";

  public static string ItemIcon(string itemInternalName) =>
      string.IsNullOrWhiteSpace(itemInternalName)
          ? string.Empty
          : $"{ItemCdn}/{itemInternalName}.png";
}
