namespace Mango.Services;

public static class AppServices
{
    public static AppSettings Settings { get; private set; } = AppSettings.Load();
    public static MatchStore MatchStore { get; private set; } = new();
    public static GsiListenerService GsiListener { get; private set; } = null!;
    public static GsiConfigInstaller GsiInstaller { get; private set; } = new();
    public static OpenDotaService OpenDota { get; private set; } = new();
    public static SteamWebApiService SteamApi { get; private set; } = new();
    public static TimerService TimerService { get; private set; } = null!;
    public static OverlayWindowService OverlayService { get; private set; } = null!;
    public static HotkeyService HotkeyService { get; private set; } = null!;
    public static HudFontService HudFonts { get; private set; } = new();

    public static void Initialize(Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
    {
        Settings = AppSettings.Load();
        OpenDota = new OpenDotaService();
        SteamApi = new SteamWebApiService();
        MatchStore = new MatchStore();
        MatchStore.Configure(
            dispatcherQueue,
            (matchId, accountId, cancellationToken) =>
                OpenDota.GetMatchRosterAsync(matchId, accountId, cancellationToken));
        HudFonts = new HudFontService();
        HudFonts.Initialize();
        GsiListener = new GsiListenerService(MatchStore, Settings);
        TimerService = new TimerService(MatchStore, Settings, dispatcherQueue);
        OverlayService = new OverlayWindowService(Settings);
        HotkeyService = new HotkeyService(OverlayService, Settings, dispatcherQueue);
        GsiListener.Start();

        // Never restore a blocking overlay across sessions; user enables it explicitly.
        Settings.OverlayVisible = false;
        Settings.OverlayInteractive = false;
        Settings.Save();

        if (Settings.DebugOverlayPreview)
        {
            MatchStore.ApplyDebugPreview();
        }
    }

    public static void Shutdown()
    {
        OverlayService.Close();
        TimerService.Dispose();
        GsiListener.Stop();
        HotkeyService.Dispose();
    }

    public static void ApplyDebugOverlayMode()
    {
        if (Settings.DebugOverlayPreview)
        {
            MatchStore.ApplyDebugPreview();
            OverlayService.ShowOverlay();
            return;
        }

        MatchStore.ClearDebugPreview();
    }

    public static GsiInstallResult EnsureGsiConfigInstalled() => GsiInstaller.Install(Settings);
}
