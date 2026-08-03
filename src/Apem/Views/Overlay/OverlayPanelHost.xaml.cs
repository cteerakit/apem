using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Apem.Views.Overlay;

public sealed partial class OverlayPanelHost : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(OverlayPanelHost),
            new PropertyMetadata(string.Empty, OnChromePropertyChanged));

    public static readonly DependencyProperty PanelContentProperty =
        DependencyProperty.Register(nameof(PanelContent), typeof(object), typeof(OverlayPanelHost), new PropertyMetadata(null));

    public static readonly DependencyProperty MinimalChromeProperty =
        DependencyProperty.Register(
            nameof(MinimalChrome),
            typeof(bool),
            typeof(OverlayPanelHost),
            new PropertyMetadata(false, OnChromePropertyChanged));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? PanelContent
    {
        get => GetValue(PanelContentProperty);
        set => SetValue(PanelContentProperty, value);
    }

    public bool MinimalChrome
    {
        get => (bool)GetValue(MinimalChromeProperty);
        set => SetValue(MinimalChromeProperty, value);
    }

    public OverlayPanelHost()
    {
        InitializeComponent();
        Loaded += (_, _) => ApplyChrome();
    }

    private static void OnChromePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OverlayPanelHost host)
        {
            host.ApplyChrome();
        }
    }

    private void ApplyChrome()
    {
        if (RootBorder is null || TitleBlock is null)
        {
            return;
        }

        if (MinimalChrome)
        {
            RootBorder.Style = (Style)Application.Current.Resources["HudStatPanelStyle"];
            TitleBlock.Visibility = Visibility.Collapsed;
            return;
        }

        RootBorder.Style = (Style)Application.Current.Resources["HudPanelStyle"];
        TitleBlock.Visibility = string.IsNullOrWhiteSpace(Title) ? Visibility.Collapsed : Visibility.Visible;
    }
}
