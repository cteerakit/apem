using Microsoft.UI.Xaml;

namespace Apem.Views.Shell;

internal static class ShellContentLayout
{
    public const double MinWidth = 560;
    public const double MaxWidth = 720;
    public const double HorizontalPadding = 64;

    /// <summary>
    /// Keep every shell page on the same column width: grow/shrink together between min and max.
    /// </summary>
    public static void Attach(FrameworkElement viewport, FrameworkElement column)
    {
        void Update()
        {
            var available = viewport.ActualWidth - HorizontalPadding;
            if (available <= 0)
            {
                return;
            }

            column.Width = Math.Clamp(available, MinWidth, MaxWidth);
        }

        viewport.SizeChanged += (_, _) => Update();
        column.Loaded += (_, _) => Update();
    }
}
