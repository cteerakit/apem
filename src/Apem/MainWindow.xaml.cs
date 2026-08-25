using Apem.Interop;
using Apem.Services;
using Apem.Views.Shell;
using Microsoft.UI.Xaml;

namespace Apem;

public sealed partial class MainWindow : Window
{
    private WindowMessageHook? _messageHook;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");
        Title = "Mango";

        RootFrame.Navigate(typeof(ShellPage));

        Closed += OnClosed;

        var hwnd = Win32WindowHelper.GetHandle(this);
        _messageHook = new WindowMessageHook(hwnd, (msg, wParam, lParam) =>
        {
            AppServices.HotkeyService.TryHandleHotkeyMessage(hwnd, msg, wParam);
        });

        AppServices.HotkeyService.Register(this);
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        AppServices.Shutdown();
    }
}
