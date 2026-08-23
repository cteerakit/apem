using System.Security.Cryptography;
using System.Text.Json;

namespace Apem.Services;

public sealed class AppSettings
{
    public const int DefaultGsiPort = 40000;
    public const string SettingsFileName = "settings.json";

    public int GsiPort { get; set; } = DefaultGsiPort;
    public string GsiToken { get; set; } = string.Empty;
    public bool ShowPlayerPanel { get; set; } = true;
    public bool ShowBountyTimer { get; set; } = true;
    public bool ShowPowerTimer { get; set; } = true;
    public bool ShowWisdomTimer { get; set; } = true;
    public bool ShowLotusTimer { get; set; } = true;
    public bool ShowBuildPanel { get; set; } = true;
    public bool OverlayVisible { get; set; }
    public bool OverlayInteractive { get; set; }
    public double OverlayOpacity { get; set; } = 0.92;
    public bool TimerSoundsEnabled { get; set; } = true;
    public bool IsTurboMode { get; set; }
    /// <summary>How many seconds before a rune spawns its countdown widget appears.</summary>
    public int RuneTimerLeadSeconds { get; set; } = 15;
    public bool DebugOverlayPreview { get; set; }
    public string? SteamId { get; set; }
    public Dictionary<string, string> PlayerNotes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public HotkeyBinding ToggleOverlayHotkey { get; set; } = HotkeyBinding.DefaultToggleOverlay();
    public HotkeyBinding ToggleInteractiveHotkey { get; set; } = HotkeyBinding.DefaultToggleInteractive();

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
                var settings = JsonSerializer.Deserialize<AppSettings>(json) ?? CreateDefault();
                settings.Normalize();
                return settings;
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
        Normalize();
        Directory.CreateDirectory(SettingsDirectory);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SettingsPath, json);
    }

    public void Normalize()
    {
        ToggleOverlayHotkey ??= HotkeyBinding.DefaultToggleOverlay();
        ToggleInteractiveHotkey ??= HotkeyBinding.DefaultToggleInteractive();
        PanelLayout ??= new PanelLayoutSettings();
        PlayerNotes = new Dictionary<string, string>(
            PlayerNotes ?? new Dictionary<string, string>(),
            StringComparer.OrdinalIgnoreCase);

        if (ToggleOverlayHotkey.VirtualKey == 0)
        {
            ToggleOverlayHotkey = HotkeyBinding.DefaultToggleOverlay();
        }

        if (ToggleInteractiveHotkey.VirtualKey == 0)
        {
            ToggleInteractiveHotkey = HotkeyBinding.DefaultToggleInteractive();
        }

        RuneTimerLeadSeconds = Math.Clamp(RuneTimerLeadSeconds, 5, 120);
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

/// <summary>
/// Remembered widget positions. A null coordinate means the widget has never been
/// dragged, so the overlay places it at its default screen-edge anchor instead.
/// </summary>
public sealed class PanelLayoutSettings
{
    public double? PlayerX { get; set; }
    public double? PlayerY { get; set; }
    public double? BountyX { get; set; }
    public double? BountyY { get; set; }
    public double? PowerX { get; set; }
    public double? PowerY { get; set; }
    public double? WisdomX { get; set; }
    public double? WisdomY { get; set; }
    public double? LotusX { get; set; }
    public double? LotusY { get; set; }
    public double? BuildX { get; set; }
    public double? BuildY { get; set; }
}
