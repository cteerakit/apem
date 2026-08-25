using Microsoft.UI.Xaml;

namespace Mango.Views.Shell;

internal static class ShellContentLayout
{
    public const double MinWidth = 560;
    public const double MaxWidth = 720;
    public const double HorizontalPadding = 64;

    /// <summary>
    /// Keep shell pages on a shared column width: grow/shrink between min and max.
    /// Pass <see cref="double.PositiveInfinity"/> to use the full viewport width.
    /// </summary>
    public static void Attach(FrameworkElement viewport, FrameworkElement column, double maxWidth = MaxWidth)
    {
        void Update()
        {
            var available = viewport.ActualWidth - HorizontalPadding;
            if (available <= 0)
            {
                return;
            }

            column.Width = Math.Clamp(available, MinWidth, maxWidth);
        }

        viewport.SizeChanged += (_, _) => Update();
        column.Loaded += (_, _) => Update();
    }

    /// <summary>
    /// Wide tables: fill the viewport when content is narrow, but allow the column to
    /// grow with content so a parent <see cref="ScrollViewer"/> can scroll horizontally.
    /// </summary>
    public static void AttachExpanding(FrameworkElement viewport, FrameworkElement column)
    {
        void Update()
        {
            var available = viewport.ActualWidth - HorizontalPadding;
            if (available <= 0)
            {
                return;
            }

            column.ClearValue(FrameworkElement.WidthProperty);
            column.MinWidth = Math.Max(MinWidth, available);
        }

        viewport.SizeChanged += (_, _) => Update();
        column.Loaded += (_, _) => Update();
    }
}
