using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Mango.Services;

public sealed class GsiConfigInstaller
{
    private const string ConfigFileName = "gamestate_integration_mango.cfg";
    private const string LegacyConfigFileName = "gamestate_integration_apem.cfg";

    public GsiInstallResult Install(AppSettings settings)
    {
        var dotaPaths = FindDotaInstallPaths();
        if (dotaPaths.Count == 0)
        {
            return new GsiInstallResult(false, "Could not find Dota 2 installation. Install Dota via Steam first.");
        }

        var writtenPaths = new List<string>();
        foreach (var dotaPath in dotaPaths)
        {
            var cfgDir = Path.Combine(dotaPath, "game", "dota", "cfg", "gamestate_integration");
            Directory.CreateDirectory(cfgDir);
            var cfgPath = Path.Combine(cfgDir, ConfigFileName);
            File.WriteAllText(cfgPath, BuildConfigContent(settings), Encoding.UTF8);
            writtenPaths.Add(cfgPath);

            var legacyPath = Path.Combine(cfgDir, LegacyConfigFileName);
            if (File.Exists(legacyPath))
            {
                File.Delete(legacyPath);
            }
        }

        return new GsiInstallResult(true, $"Installed GSI config to {writtenPaths.Count} location(s).", writtenPaths);
    }

    public static string BuildConfigContent(AppSettings settings) =>
        $$"""
        "Mango"
        {
            "uri"               "http://127.0.0.1:{{settings.GsiPort}}/"
            "timeout"           "5.0"
            "buffer"            "1.0"
            "throttle"          "1.0"
            "heartbeat"         "30.0"
            "data"
            {
                "provider"      "1"
                "map"           "1"
                "player"        "1"
                "allplayers"    "1"
                "hero"          "1"
                "abilities"     "1"
                "items"         "1"
                "buildings"     "1"
                "draft"         "1"
                "events"        "1"
            }
            "auth"
            {
                "token"         "{{settings.GsiToken}}"
            }
        }
        """;

    public static List<string> FindDotaInstallPaths()
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var steamRoot in GetSteamInstallPaths())
        {
            var defaultDota = Path.Combine(steamRoot, "steamapps", "common", "dota 2 beta");
            if (Directory.Exists(defaultDota))
            {
                paths.Add(defaultDota);
            }

            var libraryFile = Path.Combine(steamRoot, "steamapps", "libraryfolders.vdf");
            if (!File.Exists(libraryFile))
            {
                continue;
            }

            foreach (var libraryPath in ParseLibraryFolders(libraryFile))
            {
                var dotaPath = Path.Combine(libraryPath, "steamapps", "common", "dota 2 beta");
                if (Directory.Exists(dotaPath))
                {
                    paths.Add(dotaPath);
                }
            }
        }

        return paths.ToList();
    }

    private static IEnumerable<string> GetSteamInstallPaths()
    {
        var roots = new List<string>();

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            var steamPath = key?.GetValue("SteamPath") as string;
            if (!string.IsNullOrWhiteSpace(steamPath))
            {
                roots.Add(steamPath);
            }
        }
        catch
        {
            // Ignore registry failures.
        }

        foreach (var envPath in new[] { @"C:\Program Files (x86)\Steam", @"C:\Program Files\Steam" })
        {
            if (Directory.Exists(envPath))
            {
                roots.Add(envPath);
            }
        }

        return roots.Distinct(StringComparer.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> ParseLibraryFolders(string vdfPath)
    {
        var content = File.ReadAllText(vdfPath);
        var matches = System.Text.RegularExpressions.Regex.Matches(content, "\"path\"\\s+\"([^\"]+)\"");
        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var path = match.Groups[1].Value.Replace(@"\\", @"\", StringComparison.Ordinal);
            if (Directory.Exists(path))
            {
                yield return path;
            }
        }
    }
}

public sealed record GsiInstallResult(bool Success, string Message, IReadOnlyList<string>? Paths = null);
