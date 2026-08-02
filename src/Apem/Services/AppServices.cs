namespace Apem.Services;

public static class AppServices
{
    public static AppSettings Settings { get; private set; } = AppSettings.Load();
    public static MatchStore MatchStore { get; private set; } = new();
    public static GsiListenerService GsiListener { get; private set; } = null!;
    public static GsiConfigInstaller GsiInstaller { get; private set; } = new();
    public static OpenDotaService OpenDota { get; private set; } = new();
    public static TimerService TimerService { get; private set; } = null!;
    public static OverlayWindowService OverlayService { get; private set; } = null!;
    public static HotkeyService HotkeyService { get; private set; } = null!;

    public static void Initialize(Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue)
    {
        Settings = AppSettings.Load();
        MatchStore = new MatchStore();
        GsiListener = new GsiListenerService(MatchStore, Settings);
        TimerService = new TimerService(MatchStore, Settings);
        OverlayService = new OverlayWindowService(Settings);
        HotkeyService = new HotkeyService(OverlayService, Settings, dispatcherQueue);
        GsiListener.Start();
    }

    public static void Shutdown()
    {
        OverlayService.Close();
        GsiListener.Stop();
        HotkeyService.Dispose();
    }

    public static GsiInstallResult EnsureGsiConfigInstalled() => GsiInstaller.Install(Settings);
}
