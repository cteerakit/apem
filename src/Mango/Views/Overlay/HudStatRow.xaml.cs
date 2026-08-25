using System.Numerics;
using Mango.Services;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI;

namespace Mango.Views.Overlay;

/// <summary>
/// One row of the in-game HUD stat strip: a muted label, a white value, and a
/// left-to-right fading slate background.
/// </summary>
public sealed partial class HudStatRow : UserControl
{
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(HudStatRow), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(HudStatRow), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty LabelWidthProperty =
        DependencyProperty.Register(
            nameof(LabelWidth),
            typeof(double),
            typeof(HudStatRow),
            new PropertyMetadata(67d, OnLabelWidthChanged));

    public static readonly DependencyProperty LabelIconProperty =
        DependencyProperty.Register(
            nameof(LabelIcon),
            typeof(ImageSource),
            typeof(HudStatRow),
            new PropertyMetadata(null, OnLabelIconChanged));

    public static readonly DependencyProperty LabelIconSizeProperty =
        DependencyProperty.Register(
            nameof(LabelIconSize),
            typeof(double),
            typeof(HudStatRow),
            new PropertyMetadata(16d, OnLabelIconSizeChanged));

    public static readonly DependencyProperty RightEdgeProperty =
        DependencyProperty.Register(
            nameof(RightEdge),
            typeof(bool),
            typeof(HudStatRow),
            new PropertyMetadata(false, OnRightEdgeChanged));

    public static readonly DependencyProperty CountdownOnlyProperty =
        DependencyProperty.Register(
            nameof(CountdownOnly),
            typeof(bool),
            typeof(HudStatRow),
            new PropertyMetadata(false, OnCountdownOnlyChanged));

    private bool _shadowsAttached;

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Width of the label column, in pixels.</summary>
    public double LabelWidth
    {
        get => (double)GetValue(LabelWidthProperty);
        set => SetValue(LabelWidthProperty, value);
    }

    /// <summary>Optional minimap-style icon shown instead of the text label.</summary>
    public ImageSource? LabelIcon
    {
        get => (ImageSource?)GetValue(LabelIconProperty);
        set => SetValue(LabelIconProperty, value);
    }

    /// <summary>Rendered size of <see cref="LabelIcon"/>, in pixels.</summary>
    public double LabelIconSize
    {
        get => (double)GetValue(LabelIconSizeProperty);
        set => SetValue(LabelIconSizeProperty, value);
    }

    /// <summary>Fades the background from right to left for widgets docked on the right screen edge.</summary>
    public bool RightEdge
    {
        get => (bool)GetValue(RightEdgeProperty);
        set => SetValue(RightEdgeProperty, value);
    }

    /// <summary>Hides the label and centers the countdown value.</summary>
    public bool CountdownOnly
    {
        get => (bool)GetValue(CountdownOnlyProperty);
        set => SetValue(CountdownOnlyProperty, value);
    }

    public HudStatRow()
    {
        InitializeComponent();

        var fonts = AppServices.HudFonts;
        LabelText.FontFamily = fonts.LabelFont;
        ValueText.FontFamily = fonts.ValueFont;

        Loaded += (_, _) =>
        {
            ApplyLabelPresentation();
            AttachShadows();
            ApplyCountdownLayout();
            RefreshFadeBackground();
        };
    }

    private void ApplyLabelPresentation()
    {
        if (LabelText is null || LabelIconImage is null || LabelShadowHost is null)
        {
            return;
        }

        var hasIcon = LabelIcon is not null;
        LabelIconImage.Visibility = hasIcon ? Visibility.Visible : Visibility.Collapsed;
        LabelText.Visibility = hasIcon ? Visibility.Collapsed : Visibility.Visible;
        LabelShadowHost.Visibility = hasIcon ? Visibility.Collapsed : Visibility.Visible;

        if (hasIcon)
        {
            LabelIconImage.Width = LabelIconSize;
            LabelIconImage.Height = LabelIconSize;
        }
    }

    private static void OnLabelIconSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HudStatRow row)
        {
            row.ApplyLabelPresentation();
        }
    }

    private static void OnRightEdgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HudStatRow row)
        {
            row.ApplyCountdownLayout();
            row.RefreshFadeBackground();
        }
    }

    private static void OnLabelIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HudStatRow row)
        {
            row.ApplyLabelPresentation();
            row.ApplyCountdownLayout();
        }
    }

    private static void OnCountdownOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HudStatRow row)
        {
            row.ApplyCountdownLayout();
        }
    }

    private void ApplyCountdownLayout()
    {
        if (LabelHost is null || ValueHost is null || ContentGrid is null)
        {
            return;
        }

        if (CountdownOnly)
        {
            LabelHost.Visibility = Visibility.Collapsed;
            LabelColumn.Width = new GridLength(0);
            ContentGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
            ContentGrid.Padding = new Thickness(8, 0, 4, 0);
            Grid.SetColumn(ValueHost, 0);
            Grid.SetColumnSpan(ValueHost, 2);
            ValueText.HorizontalAlignment = HorizontalAlignment.Center;
            return;
        }

        LabelHost.Visibility = Visibility.Visible;
        LabelColumn.Width = new GridLength(LabelWidth);
        Grid.SetColumn(ValueHost, 1);
        Grid.SetColumnSpan(ValueHost, 1);
        ValueText.HorizontalAlignment = HorizontalAlignment.Left;

        // Right-edge rows mirror the left gutter so the content sits against the
        // opaque end of the fade instead of trailing off into the transparent side.
        ContentGrid.HorizontalAlignment = RightEdge ? HorizontalAlignment.Right : HorizontalAlignment.Stretch;
        ContentGrid.Padding = RightEdge ? new Thickness(0, 0, 10, 0) : new Thickness(10, 0, 0, 0);
    }

    private static void OnLabelWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is HudStatRow row && row.LabelColumn is not null)
        {
            row.LabelColumn.Width = new GridLength(row.LabelWidth);
        }
    }

    private void AttachShadows()
    {
        if (_shadowsAttached)
        {
            return;
        }

        _shadowsAttached = true;
        if (LabelIcon is null)
        {
            AttachTextShadow(LabelShadowHost, LabelText);
        }

        AttachTextShadow(ValueShadowHost, ValueText);
    }

    /// <summary>
    /// Renders a blurred shadow on a sibling layer behind the text. The sprite is
    /// sized and positioned to the text bounds so the mask stays aligned.
    /// </summary>
    private static void AttachTextShadow(Border host, TextBlock text)
    {
        var compositor = ElementCompositionPreview.GetElementVisual(host).Compositor;

        var shadow = compositor.CreateDropShadow();
        shadow.BlurRadius = 5f;
        shadow.Offset = new Vector3(0f, 1f, 0f);
        shadow.Color = Colors.Black;
        shadow.Opacity = 0.85f;

        var sprite = compositor.CreateSpriteVisual();
        sprite.Shadow = shadow;
        ElementCompositionPreview.SetElementChildVisual(host, sprite);

        void RefreshShadow()
        {
            if (host.ActualWidth <= 0 || host.ActualHeight <= 0)
            {
                return;
            }

            text.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            var textSize = text.DesiredSize;
            if (textSize.Width <= 0 || textSize.Height <= 0)
            {
                return;
            }

            var origin = text.TransformToVisual(host).TransformPoint(new Point(0, 0));
            var layoutHeight = text.ActualHeight > 0 ? text.ActualHeight : textSize.Height;
            var y = origin.Y + Math.Max(0, (layoutHeight - textSize.Height) / 2);

            sprite.Offset = new Vector3((float)origin.X, (float)y, 0);
            sprite.Size = new Vector2((float)textSize.Width, (float)textSize.Height);
            shadow.Mask = text.GetAlphaMask();
        }

        text.RegisterPropertyChangedCallback(TextBlock.TextProperty, (_, _) => RefreshShadow());
        text.RegisterPropertyChangedCallback(TextBlock.FontSizeProperty, (_, _) => RefreshShadow());
        text.RegisterPropertyChangedCallback(TextBlock.FontFamilyProperty, (_, _) => RefreshShadow());
        text.RegisterPropertyChangedCallback(TextBlock.FontWeightProperty, (_, _) => RefreshShadow());
        text.SizeChanged += (_, _) => RefreshShadow();
        host.SizeChanged += (_, _) => RefreshShadow();
        RefreshShadow();
    }

    /// <summary>
    /// The fade is drawn with Composition rather than a XAML gradient brush so it
    /// composites correctly against the transparent overlay surface.
    /// </summary>
    private void OnFadeBackgroundSizeChanged(object sender, SizeChangedEventArgs e) =>
        RefreshFadeBackground();

    private void RefreshFadeBackground()
    {
        if (FadeBackground is not Border border || border.ActualWidth <= 0 || border.ActualHeight <= 0)
        {
            return;
        }

        var compositor = ElementCompositionPreview.GetElementVisual(border).Compositor;
        var sprite = compositor.CreateSpriteVisual();
        sprite.Size = new Vector2((float)border.ActualWidth, (float)border.ActualHeight);

        var brush = compositor.CreateLinearGradientBrush();
        if (RightEdge)
        {
            brush.StartPoint = new Vector2(1f, 0.5f);
            brush.EndPoint = new Vector2(0f, 0.5f);
        }
        else
        {
            brush.StartPoint = new Vector2(0f, 0.5f);
            brush.EndPoint = new Vector2(1f, 0.5f);
        }

        // Stops trace the measured falloff of the in-game stat rows: a near-linear
        // decline across most of the bar, then a faster fade to fully transparent.
        var tint = Color.FromArgb(0xFF, 0x3C, 0x4F, 0x5C);
        ReadOnlySpan<(float Position, byte Alpha)> stops =
        [
            (0f, 0x9E),
            (0.18f, 0x91),
            (0.37f, 0x73),
            (0.55f, 0x5C),
            (0.73f, 0x3F),
            (0.86f, 0x1C),
            (0.97f, 0x00),
        ];

        foreach (var (position, alpha) in stops)
        {
            brush.ColorStops.Insert(
                brush.ColorStops.Count,
                compositor.CreateColorGradientStop(position, WithAlpha(tint, alpha)));
        }

        sprite.Brush = brush;
        ElementCompositionPreview.SetElementChildVisual(border, sprite);
    }

    private static Color WithAlpha(Color color, byte alpha) =>
        Color.FromArgb(alpha, color.R, color.G, color.B);
}
