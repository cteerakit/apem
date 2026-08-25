using Mango.Services;
using Microsoft.UI.Xaml;

namespace Mango;

public partial class App : Application
{
    public static Window Window { get; private set; } = null!;
    public static Microsoft.UI.Dispatching.DispatcherQueue DispatcherQueue { get; private set; } = null!;
    public static nint WindowHandle => WinRT.Interop.WindowNative.GetWindowHandle(Window);

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        DispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        AppServices.Initialize(DispatcherQueue);

        Window = new MainWindow();
        Window.Activate();
    }
}
