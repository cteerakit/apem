using Apem.ViewModels;
using Microsoft.UI.Xaml.Controls;

namespace Apem.Views.Shell;

public sealed partial class ShellPage : Page
{
    public ShellViewModel ViewModel { get; } = new();

    public ShellPage()
    {
        InitializeComponent();
    }
}
