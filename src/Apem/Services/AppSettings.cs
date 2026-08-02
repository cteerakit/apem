using System.Security.Cryptography;
using System.Text.Json;

namespace Apem.Services;

public sealed class AppSettings
{
    public const int DefaultGsiPort = 40000;
    public const string SettingsFileName = "settings.json";

    public int GsiPort { get; set; } = DefaultGsiPort;
    public string GsiToken { get; set; } = string.Empty;
    public bool ShowScoreboardPanel { get; set; } = true;
    public bool ShowPlayerPanel { get; set; } = true;
    public bool ShowItemsPanel { get; set; } = true;
    public bool ShowAbilitiesPanel { get; set; } = true;
    public bool ShowTimersPanel { get; set; } = true;
    public bool ShowDraftPanel { get; set; } = true;
    public bool ShowBuildPanel { get; set; } = true;
    public bool OverlayVisible { get; set; }
    public bool OverlayInteractive { get; set; }
    public double OverlayOpacity { get; set; } = 0.92;
    public bool TimerSoundsEnabled { get; set; } = true;
    public bool IsTurboMode { get; set; }
    public string? SteamId { get; set; }

    public PanelLayoutSettings PanelLayout { get; set; } = new();

    public static string SettingsDirectory =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "APEM");

    public static string SettingsPath => Path.Combine(SettingsDirectory, SettingsFileName);

    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? CreateDefault();
            }
        }
        catch
        {
            // Fall through to defaults.
        }

        return CreateDefault();
    }

    public void Save()
    {
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }

    private static AppSettings CreateDefault()
    {
        var settings = new AppSettings
        {
            GsiToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant(),
        };
        settings.Save();
        return settings;
    }
}

public sealed class PanelLayoutSettings
{
    public double ScoreboardX { get; set; } = 24;
    public double ScoreboardY { get; set; } = 24;
    public double PlayerX { get; set; } = 24;
    public double PlayerY { get; set; } = 120;
    public double ItemsX { get; set; } = 24;
    public double ItemsY { get; set; } = 320;
    public double AbilitiesX { get; set; } = 24;
    public double AbilitiesY { get; set; } = 460;
    public double TimersX { get; set; } = 420;
    public double TimersY { get; set; } = 24;
    public double DraftX { get; set; } = 420;
    public double DraftY { get; set; } = 260;
    public double BuildX { get; set; } = 420;
    public double BuildY { get; set; } = 520;
}
