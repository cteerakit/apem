using Apem.Services;
using Apem.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;

namespace Apem.Views.Shell;

public sealed partial class ShortcutsPage : Page
{
    private enum CaptureTarget
    {
        None,
        Overlay,
        Interactive,
    }

    private ShellViewModel? _viewModel;
    private CaptureTarget _captureTarget;

    public ShortcutsPage()
    {
        InitializeComponent();
        ShellContentLayout.Attach(LayoutRoot, ContentColumn);
        KeyDown += OnPageKeyDown;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is not ShellViewModel viewModel)
        {
            return;
        }

        _viewModel = viewModel;
        viewModel.ReloadHotkeysFromSettings();
        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        BindFromViewModel();
        Focus(FocusState.Programmatic);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        StopCapture();
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        base.OnNavigatedFrom(e);
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ShellViewModel.ToggleOverlayHotkeyText)
            or nameof(ShellViewModel.ToggleInteractiveHotkeyText)
            or nameof(ShellViewModel.HotkeyStatusMessage)
            or null)
        {
            BindFromViewModel();
        }
    }

    private void BindFromViewModel()
    {
        if (_viewModel is null)
        {
            return;
        }

        OverlayHotkeyText.Text = _viewModel.ToggleOverlayHotkeyText;
        InteractiveHotkeyText.Text = _viewModel.ToggleInteractiveHotkeyText;
        StatusMessageText.Text = _viewModel.HotkeyStatusMessage;
    }

    private void ChangeOverlayButton_Click(object sender, RoutedEventArgs e) => StartCapture(CaptureTarget.Overlay);

    private void ChangeInteractiveButton_Click(object sender, RoutedEventArgs e) => StartCapture(CaptureTarget.Interactive);

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        StopCapture();
        _viewModel?.SaveHotkeysCommand.Execute(null);
        BindFromViewModel();
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        StopCapture();
        _viewModel?.ResetHotkeysCommand.Execute(null);
        BindFromViewModel();
    }

    private void StartCapture(CaptureTarget target)
    {
        _captureTarget = target;
        OverlayCaptureHint.Visibility = target == CaptureTarget.Overlay ? Visibility.Visible : Visibility.Collapsed;
        InteractiveCaptureHint.Visibility = target == CaptureTarget.Interactive ? Visibility.Visible : Visibility.Collapsed;
        if (_viewModel is not null)
        {
            _viewModel.HotkeyStatusMessage = "Waiting for shortcut…";
            BindFromViewModel();
        }

        Focus(FocusState.Programmatic);
    }

    private void StopCapture()
    {
        _captureTarget = CaptureTarget.None;
        OverlayCaptureHint.Visibility = Visibility.Collapsed;
        InteractiveCaptureHint.Visibility = Visibility.Collapsed;
    }

    private void OnPageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_captureTarget == CaptureTarget.None || _viewModel is null)
        {
            return;
        }

        var vk = (int)e.Key;
        if (HotkeyFormatting.IsModifierKey(vk) || e.Key is VirtualKey.None)
        {
            return;
        }

        // Ignore Escape to cancel capture without assigning.
        if (e.Key == VirtualKey.Escape)
        {
            StopCapture();
            _viewModel.HotkeyStatusMessage = "Capture cancelled.";
            BindFromViewModel();
            e.Handled = true;
            return;
        }

        var binding = new HotkeyBinding
        {
            Ctrl = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down),
            Alt = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down),
            Shift = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down),
            Win = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.LeftWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down)
                || Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.RightWindows).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down),
            VirtualKey = vk,
        };

        if (!binding.Ctrl && !binding.Alt && !binding.Shift && !binding.Win)
        {
            _viewModel.HotkeyStatusMessage = "Hold Ctrl, Alt, Shift, or Win together with the key.";
            BindFromViewModel();
            e.Handled = true;
            return;
        }

        if (_captureTarget == CaptureTarget.Overlay)
        {
            _viewModel.EditingToggleOverlay.CopyFrom(binding);
        }
        else
        {
            _viewModel.EditingToggleInteractive.CopyFrom(binding);
        }

        _viewModel.RefreshHotkeyTexts();
        _viewModel.HotkeyStatusMessage = $"Set to {HotkeyFormatting.ToDisplayString(binding)}. Click Save shortcuts to apply.";
        StopCapture();
        BindFromViewModel();
        e.Handled = true;
    }
}
