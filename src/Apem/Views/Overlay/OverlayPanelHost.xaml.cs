using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Apem.Views.Overlay;

public sealed partial class OverlayPanelHost : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(OverlayPanelHost), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty PanelContentProperty =
        DependencyProperty.Register(nameof(PanelContent), typeof(object), typeof(OverlayPanelHost), new PropertyMetadata(null));

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

    public OverlayPanelHost()
    {
        InitializeComponent();
    }
}
